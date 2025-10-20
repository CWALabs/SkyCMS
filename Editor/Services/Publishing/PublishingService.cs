// <copyright file="Class.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Publishing
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Cosmos.BlobService;
    using Cosmos.BlobService.Models;
    using Cosmos.Cms.Common.Services.Configurations;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Sky.Editor.Data.Logic;
    using Sky.Editor.Services.CDN;

    /// <inheritdoc/>
    public class PublishingService : IPublishingService
    {
        private readonly ApplicationDbContext _db;
        private readonly StorageContext _storage;
        private readonly IEditorSettings _settings;
        private readonly ILogger<PublishingService> _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly Authors.IAuthorInfoService _authors;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishingService"/> class.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="storage">The storage context.</param>
        /// <param name="settings">The editor settings.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="accessor">The HTTP context accessor.</param>
        /// <param name="authors">The author information service.</param>
        public PublishingService(
            ApplicationDbContext db,
            StorageContext storage,
            IEditorSettings settings,
            ILogger<PublishingService> logger,
            IHttpContextAccessor accessor,
            Authors.IAuthorInfoService authors)
        {
            _db = db;
            _storage = storage;
            _settings = settings;
            _logger = logger;
            _accessor = accessor;
            _authors = authors;
        }

        /// <inheritdoc/>
        public async Task<List<CdnResult>> PublishAsync(Article article)
        {
            if (article.Published == null)
            {
                return new List<CdnResult>();
            }

            // Unpublish other versions that are older than this one.
            await UnpublishOlderVersions(article);

            // Remove prior published (non-redirect) pages for this article number
            var prior = await _db.Pages
                .Where(p => p.ArticleNumber == article.ArticleNumber && p.StatusCode != (int)StatusCodeEnum.Redirect)
                .ToListAsync();

            if (prior.Any())
            {
                _db.Pages.RemoveRange(prior);
                await _db.SaveChangesAsync();

                DeleteStatic(prior);
            }

            var authorInfo = await _authors.GetOrCreateAsync(Guid.Parse(article.UserId));

            var page = new PublishedPage
            {
                Id = Guid.NewGuid(),
                ArticleNumber = article.ArticleNumber,
                StatusCode = article.StatusCode,
                UrlPath = article.UrlPath,
                VersionNumber = article.VersionNumber,
                Published = article.Published,
                Expires = article.Expires,
                Title = article.Title,
                Content = article.Content,
                Updated = article.Updated,
                BannerImage = article.BannerImage,
                HeaderJavaScript = article.HeaderJavaScript,
                FooterJavaScript = article.FooterJavaScript,
                ParentUrlPath = article.UrlPath.Contains('/')
                    ? article.UrlPath[..article.UrlPath.LastIndexOf('/')]
                    : string.Empty,
                AuthorInfo = authorInfo == null ? string.Empty :
                    JsonConvert.SerializeObject(authorInfo).Replace("\"", "'"),
                ArticleType = article.ArticleType,
                Category = article.Category,
                Introduction = article.Introduction
            };

            _db.Pages.Add(page);
            await _db.SaveChangesAsync();

            await CreateStaticFile(page);
            await WriteTocAsync("/");
            return await PurgeCdnAsync(page);
        }

        /// <summary>
        /// Publish multiple pages.
        /// </summary>
        /// <param name="ids">IDs of published pages.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateStaticPages(IEnumerable<Guid> ids)
        {
            var pages = await _db.Pages.Where(w => ids.Contains(w.Id)).ToListAsync();
            foreach (var page in pages)
            {
                await CreateStaticFile(page);
            }

            // Write the table of contents.
            await WriteTocAsync("/");

            // Refresh the CDN if present.
            var cdnService = CdnService.GetCdnService(_db, _logger, _accessor.HttpContext);
            if (cdnService != null)
            {
                await cdnService.PurgeCdn();
            }
        }

        /// <inheritdoc/>
        public async Task UnpublishAsync(Article article)
        {
            var articleNumber = article.ArticleNumber;

            var versions = await _db.Articles.Where(a => a.ArticleNumber == articleNumber && a.Published != null).ToListAsync();
            if (!versions.Any())
            {
                return;
            }

            foreach (var v in versions)
            {
                v.Published = null;
            }

            var pages = await _db.Pages
                .Where(p => p.ArticleNumber == articleNumber && p.StatusCode != (int)StatusCodeEnum.Redirect)
                .ToListAsync();

            _db.Pages.RemoveRange(pages);
            await _db.SaveChangesAsync();
            DeleteStatic(pages);

            foreach (var page in pages)
            {
                await PurgeCdnAsync(page);
            }

            await WriteTocAsync("/");
        }

        /// <inheritdoc/>
        public async Task WriteTocAsync(string prefix = "/")
        {
            if (!_settings.StaticWebPages)
            {
                return;
            }

            var toc = await new ArticleLogic(
                _db,
                Microsoft.Extensions.Options.Options.Create(new CosmosConfig()),
                new MemoryCache(new MemoryCacheOptions()),
                _settings.PublisherUrl,
                _settings.BlobPublicUrl,
                true)
                .GetTableOfContents("/", 0, 500, false);

            if (toc == null)
            {
                return;
            }

            var json = JsonConvert.SerializeObject(toc);
            var target = string.IsNullOrEmpty(prefix) ? "/toc.json" : "/" + prefix + "/toc.json";
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            await _storage.AppendBlob(ms, new FileUploadMetaData
            {
                ChunkIndex = 0,
                ContentType = "application/json",
                FileName = Path.GetFileName(target),
                RelativePath = target,
                TotalChunks = 1,
                TotalFileSize = ms.Length,
                UploadUid = Guid.NewGuid().ToString()
            });
        }

        private async Task UnpublishOlderVersions(Article article)
        {
            var dateTime = article.Published;

            // Unpublish other versions
            var others = await _db.Articles.Where(a =>
                a.ArticleNumber == article.ArticleNumber &&
                a.Published < dateTime &&
                a.Id != article.Id).ToListAsync();

            var ids = others.Select(o => o.Id).ToList();

            foreach (var o in others)
            {
                o.Published = null;
            }

            var doomedPages = await _db.Pages
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            _db.Pages.RemoveRange(doomedPages);
            await _db.SaveChangesAsync();

            DeleteStatic(doomedPages);
        }

        /// <summary>
        /// Deletes the static file for the specified published page.
        /// </summary>
        /// <param name="pages">The published pages.</param>
        private void DeleteStatic(IEnumerable<PublishedPage> pages)
        {
            if (!_settings.StaticWebPages)
            {
                return;
            }

            foreach (var page in pages)
            {
                var rel = page.UrlPath.Equals("root", StringComparison.OrdinalIgnoreCase)
                ? "/index.html"
                : "/" + page.UrlPath.TrimStart('/');
                try
                {
                    _storage.DeleteFile(rel);
                }
                catch
                {
                    /* ignore */
                }
            }
        }

        private async Task CreateStaticFile(PublishedPage page)
        {
            if (!_settings.StaticWebPages)
            {
                return;
            }

            var rel = page.UrlPath.Equals("root", StringComparison.OrdinalIgnoreCase)
                ? "/index.html"
                : "/" + page.UrlPath.TrimStart('/');

            var html = new StringBuilder()
                .Append("<!DOCTYPE html><html lang='en'><head><meta charset='utf-8'><title>")
                .Append(System.Net.WebUtility.HtmlEncode(page.Title))
                .Append("</title>")
                .Append(page.HeaderJavaScript)
                .Append("</head><body>")
                .Append(page.Content)
                .Append(page.FooterJavaScript)
                .Append("</body></html>")
                .ToString();

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(html));
            await _storage.AppendBlob(ms, new FileUploadMetaData
            {
                ChunkIndex = 0,
                ContentType = "text/html",
                FileName = Path.GetFileName(rel),
                RelativePath = rel,
                TotalChunks = 1,
                TotalFileSize = ms.Length,
                UploadUid = Guid.NewGuid().ToString()
            });
        }

        private async Task<List<CdnResult>> PurgeCdnAsync(PublishedPage page)
        {
            var results = new List<CdnResult>();
            try
            {
                var cdnService = CdnService.GetCdnService(_db, _logger, _accessor.HttpContext);
                if (cdnService == null)
                {
                    return results;
                }

                var path = page.UrlPath.Equals("root", StringComparison.OrdinalIgnoreCase)
                    ? "/"
                    : $"{_settings.PublisherUrl.TrimEnd('/')}/{page.UrlPath.TrimStart('/')}";

                var paths = new List<string> { path };

                results = await cdnService.PurgeCdn(paths);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "CDN purge failed");
            }

            return results;
        }

    }
}
