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
    using Cosmos.Common.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Sky.Editor.Data.Logic;
    using Sky.Editor.Services.CDN;

    internal sealed class PublishedArtifactService : IPublishedArtifactService
    {
        private readonly ApplicationDbContext _db;
        private readonly StorageContext _storage;
        private readonly EditorSettings _settings;
        private readonly ILogger<PublishedArtifactService> _logger;
        private readonly IHttpContextAccessor _accessor;
        private readonly IAuthorInfoService _authors;

        public PublishedArtifactService(
            ApplicationDbContext db,
            StorageContext storage,
            EditorSettings settings,
            ILogger<PublishedArtifactService> logger,
            IHttpContextAccessor accessor,
            IAuthorInfoService authors)
        {
            _db = db;
            _storage = storage;
            _settings = settings;
            _logger = logger;
            _accessor = accessor;
            _authors = authors;
        }

        public async Task<List<CdnResult>> PublishAsync(Article article)
        {
            if (article.Published == null) return new List<CdnResult>();

            // Remove prior published (non-redirect) pages for this article number
            var prior = await _db.Pages
                .Where(p => p.ArticleNumber == article.ArticleNumber && p.StatusCode != (int)StatusCodeEnum.Redirect)
                .ToListAsync();

            if (prior.Any())
            {
                _db.Pages.RemoveRange(prior);
                await _db.SaveChangesAsync();

                foreach (var p in prior)
                    await DeleteStaticAsync(p.UrlPath);
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

        public async Task PublishRedirectAsync(Article redirectArticle)
        {
            // Minimal static file for redirect if static mode enabled
            if (!_settings.StaticWebPages) return;
            var slug = redirectArticle.UrlPath.Equals("root", StringComparison.OrdinalIgnoreCase)
                ? "index.html"
                : redirectArticle.UrlPath;

            var html = new StringBuilder()
                .Append("<!DOCTYPE html><html><head><meta charset='utf-8'><title>Redirect</title>")
                .Append("<meta http-equiv='refresh' content='0;url=/")
                .Append(redirectArticle.RedirectTarget)
                .Append("' /></head><body>")
                .Append("<p>Redirecting...</p></body></html>")
                .ToString();

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(html));
            await _storage.AppendBlob(ms, new FileUploadMetaData
            {
                ChunkIndex = 0,
                ContentType = "text/html",
                FileName = Path.GetFileName(slug),
                RelativePath = slug.StartsWith("/") ? slug : "/" + slug,
                TotalChunks = 1,
                TotalFileSize = ms.Length,
                UploadUid = Guid.NewGuid().ToString()
            });
        }

        public Task DeleteStaticAsync(string urlPath)
        {
            if (!_settings.StaticWebPages || string.IsNullOrWhiteSpace(urlPath)) return Task.CompletedTask;
            if (urlPath.StartsWith("/pub", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            var rel = urlPath.Equals("root", StringComparison.OrdinalIgnoreCase) ? "/index.html" : "/" + urlPath.TrimStart('/');
            try { _storage.DeleteFile(rel); } catch { /* ignore */ }
            return Task.CompletedTask;
        }

        private async Task CreateStaticFile(PublishedPage page)
        {
            if (!_settings.StaticWebPages) return;

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

        private async Task WriteTocAsync(string prefix)
        {
            if (!_settings.StaticWebPages) return;
            var toc = await new ArticleLogic(
                _db,
                Microsoft.Extensions.Options.Options.Create(new CosmosConfig()),
                new MemoryCache(new MemoryCacheOptions()),
                _settings.PublisherUrl,
                _settings.BlobPublicUrl,
                true)
                .GetTableOfContents("/", 0, 500, false);

            if (toc == null) return;
            var json = JsonConvert.SerializeObject(toc);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            await _storage.AppendBlob(ms, new FileUploadMetaData
            {
                ChunkIndex = 0,
                ContentType = "application/json",
                FileName = "toc.json",
                RelativePath = "/toc.json",
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
                if (cdnService == null) return results;

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
