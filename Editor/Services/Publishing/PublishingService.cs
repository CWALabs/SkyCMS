// <copyright file="PublishingService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
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
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.BlogPublishing;
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
        private readonly IClock _systemClock;
        private readonly IBlogRenderingService blogRenderingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishingService"/> class.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="storage">The storage context.</param>
        /// <param name="settings">The editor settings.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="accessor">The HTTP context accessor.</param>
        /// <param name="authors">The author information service.</param>
        /// <param name="systemClock">The system clock.</param>
        /// <param name="blogRenderingService">The blog stream and post rendering service.</param>
        public PublishingService(
            ApplicationDbContext db,
            StorageContext storage,
            IEditorSettings settings,
            ILogger<PublishingService> logger,
            IHttpContextAccessor accessor,
            Authors.IAuthorInfoService authors,
            IClock systemClock,
            IBlogRenderingService blogRenderingService)
        {
            _db = db;
            _storage = storage;
            _settings = settings;
            _logger = logger;
            _accessor = accessor;
            _authors = authors;
            _systemClock = systemClock;
            this.blogRenderingService = blogRenderingService;
        }

        /// <summary>
        /// Publishes a blog stream article based on the provided blog data and user ID.
        /// </summary>
        /// <param name="blog"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<List<CdnResult>> PublishAsync(Article blog, Guid userId)
        {
            var article = await _db.Articles
                .Where(a => a.BlogKey == blog.BlogKey && a.ArticleType == (int)ArticleType.BlogStream)
                .OrderByDescending(a => a.VersionNumber)
                .FirstOrDefaultAsync();

            if (article == null)
            {
                var articleNumber = (await _db.Articles.AnyAsync()) ?
                    (await _db.Articles.Select(s => s.VersionNumber).MaxAsync()) + 1 : 1;

                article = new Article
                {
                    ArticleNumber = articleNumber,
                    UrlPath = blog.BlogKey,
                    VersionNumber = 1,
                    Published = DateTimeOffset.UtcNow,
                    Expires = null,
                    Title = blog.Title,
                    Content = await blogRenderingService.GenerateBlogStreamHtml(blog),
                    Updated = blog.Updated,
                    BannerImage = blog.BannerImage,
                    HeaderJavaScript = string.Empty,
                    FooterJavaScript = string.Empty,
                    UserId = userId.ToString(),
                    StatusCode = (int)StatusCodeEnum.Active,
                    ArticleType = (int)ArticleType.BlogStream,
                    Category = "blog-stream",
                    Introduction = blog.Introduction,
                    BlogKey = blog.BlogKey
                };

                _db.Articles.Add(article);
            }
            else
            {
                article.UrlPath = blog.BlogKey;
                article.Published = DateTimeOffset.UtcNow;
                article.Title = blog.Title;
                article.Content = await blogRenderingService.GenerateBlogStreamHtml(blog);
                article.Updated = blog.Updated;
                article.BannerImage = blog.BannerImage;
                article.Introduction = blog.Introduction;
                article.UserId = userId.ToString();
                article.StatusCode = (int)StatusCodeEnum.Active;
                article.VersionNumber += 1;
            }

            return await PublishAsync(article);
        }

        /// <inheritdoc/>
        public async Task<List<CdnResult>> PublishAsync(Article article)
        {
            if (article.Published == null)
            {
                article.Published = DateTimeOffset.UtcNow.AddSeconds(-1);
            }

            // Unpublish earlier versions of this article number.
            await UnpublishEalierVersions(article);

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

            PublishedPage page;

            if (article.ArticleType == (int)ArticleType.BlogPost)
            {
                var blogContent = await blogRenderingService.GenerateBlogEntryHtml(article);

                page = new PublishedPage
                {
                    Id = Guid.NewGuid(),
                    ArticleNumber = article.ArticleNumber,
                    StatusCode = article.StatusCode,
                    UrlPath = article.UrlPath,
                    VersionNumber = article.VersionNumber,
                    Published = article.Published,
                    Expires = article.Expires,
                    Title = article.Title,
                    Content = blogContent,
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
                    Introduction = article.Introduction,
                    BlogKey = article.BlogKey
                };
            }
            else
            {
                page = new PublishedPage
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
            }

            _db.Pages.Add(page);
            await _db.SaveChangesAsync();

            await CreateStaticFile(page);
            await WriteTocAsync("/");
            return await PurgeCdnAsync(page);
        }

        /// <summary>
        /// Creates static HTML files for the specified published pages and purges the CDN cache.
        /// </summary>
        /// <param name="ids">Collection of page identifiers to generate static files for.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// This method is used for batch static page generation, typically during republishing operations
        /// or site-wide regeneration events. It performs the following actions:
        /// </para>
        /// <list type="number">
        ///   <item><description>Retrieves all published pages matching the provided IDs from the database</description></item>
        ///   <item><description>Generates and uploads static HTML files for each page to blob storage</description></item>
        ///   <item><description>Regenerates the table of contents (TOC) JSON file</description></item>
        ///   <item><description>Triggers a full CDN cache purge if a CDN service is configured</description></item>
        /// </list>
        /// <para>
        /// Unlike <see cref="PublishAsync"/>, this method performs a full CDN purge rather than selective path purging.
        /// Only processes pages if <see cref="IEditorSettings.StaticWebPages"/> is enabled.
        /// </para>
        /// </remarks>
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


        /// <summary>
        /// Unpublishes earlier versions of an article to ensure only the latest published version is active.
        /// </summary>
        /// <param name="article">The article being published. Must have a valid <see cref="Article.ArticleNumber"/> and <see cref="Article.VersionNumber"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// This method ensures content integrity by removing earlier published versions of the same article
        /// when a newer version is published. It performs the following actions:
        /// </para>
        /// <list type="number">
        ///   <item><description>Validates the article's publish status (must be published now or earlier)</description></item>
        ///   <item><description>Locates all earlier published versions of the same article number</description></item>
        ///   <item><description>Marks those versions as unpublished (sets <c>Published</c> to null)</description></item>
        ///   <item><description>Removes their corresponding published page records from the database</description></item>
        ///   <item><description>Deletes associated static files from storage</description></item>
        /// </list>
        /// <para>
        /// If the article is scheduled for future publication or is not published, this method exits early without changes.
        /// This prevents premature cleanup of existing published content.
        /// </para>
        /// </remarks>
        private async Task UnpublishEalierVersions(Article article)
        {
            var dateTime = _systemClock.UtcNow;

            if (article.Published == null || article.Published > dateTime)
            {
                // Nothing to do.
                // We only publish versions that are published now or earlier.
                return;
            }

            var versionNumber = article.VersionNumber;

            // Find previous versions of this article number that are published before this one.
            var others = await _db.Articles.Where(a =>
                a.ArticleNumber == article.ArticleNumber &&
                a.Published != null &&
                a.VersionNumber < versionNumber).ToListAsync();

            if (!others.Any())
            {
                // There are no previous versions published.
                return;
            }

            var ids = others.Select(s => s.Id).ToList();

            // Unpublish them.
            foreach (var o in others)
            {
                o.Published = null;
            }

            await _db.SaveChangesAsync();

            // Remove their published pages.
            var doomedPages = await _db.Pages
                .Where(p => ids.Contains(p.Id))
                .ToListAsync();

            _db.Pages.RemoveRange(doomedPages);
            await _db.SaveChangesAsync();

            // Remove their static files.
            DeleteStatic(doomedPages);
        }

        /// <summary>
        /// Deletes static HTML files from blob storage for the specified published pages.
        /// </summary>
        /// <param name="pages">Collection of published pages whose static files should be removed.</param>
        /// <remarks>
        /// <para>
        /// This method is called during unpublish operations to clean up static artifacts.
        /// File deletion failures are silently ignored to ensure the unpublish workflow continues
        /// even if storage is temporarily unavailable or files have already been removed.
        /// </para>
        /// <para>
        /// Each page's <see cref="PublishedPage.UrlPath"/> is converted to a storage-relative path:
        /// </para>
        /// <list type="bullet">
        ///   <item><description>"root" → "/index.html"</description></item>
        ///   <item><description>Any other path → "/{urlPath}" (normalized with leading slash)</description></item>
        /// </list>
        /// <para>
        /// Only executes if <see cref="IEditorSettings.StaticWebPages"/> is enabled.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Generates and uploads a static HTML file to blob storage for the specified published page.
        /// </summary>
        /// <param name="page">The published page to generate a static file for. Must have valid <see cref="PublishedPage.UrlPath"/>, <see cref="PublishedPage.Title"/>, and <see cref="PublishedPage.Content"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous upload operation.</returns>
        /// <remarks>
        /// <para>
        /// Constructs a complete, minimal HTML5 document from the page metadata and content.
        /// The generated HTML structure includes:
        /// </para>
        /// <list type="bullet">
        ///   <item><description>HTML5 doctype and language attribute</description></item>
        ///   <item><description>UTF-8 character encoding declaration</description></item>
        ///   <item><description>HTML-encoded page title from <see cref="PublishedPage.Title"/></description></item>
        ///   <item><description>Optional header scripts from <see cref="PublishedPage.HeaderJavaScript"/></description></item>
        ///   <item><description>Page body content from <see cref="PublishedPage.Content"/></description></item>
        ///   <item><description>Optional footer scripts from <see cref="PublishedPage.FooterJavaScript"/></description></item>
        /// </list>
        /// <para>
        /// The file is uploaded to blob storage with MIME type "text/html" at a path determined by the page's URL:
        /// </para>
        /// <list type="bullet">
        ///   <item><description>"root" → "/index.html"</description></item>
        ///   <item><description>Other paths → "/{urlPath}" (normalized with leading slash)</description></item>
        /// </list>
        /// <para>
        /// Only executes if <see cref="IEditorSettings.StaticWebPages"/> is enabled.
        /// </para>
        /// </remarks>
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

        /// <summary>
        /// Purges the CDN cache for the specified published page's URL path.
        /// </summary>
        /// <param name="page">The published page whose CDN cache should be invalidated. Must have a valid <see cref="PublishedPage.UrlPath"/>.</param>
        /// <returns>
        /// A task producing a list of <see cref="CdnResult"/> objects representing the outcome of CDN purge operations.
        /// Returns an empty list if no CDN service is configured or if the operation fails.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method coordinates cache invalidation with configured CDN providers (e.g., Azure CDN, Cloudflare)
        /// to ensure updated content is served immediately rather than after cache expiration.
        /// </para>
        /// <para>
        /// The purge path is constructed from the page's URL:
        /// </para>
        /// <list type="bullet">
        ///   <item><description>"root" → "/" (site homepage)</description></item>
        ///   <item><description>Other paths → "{PublisherUrl}/{urlPath}" (fully qualified URL)</description></item>
        /// </list>
        /// <para>
        /// CDN purge failures are logged as warnings but do not throw exceptions, allowing publish operations
        /// to complete successfully even when CDN communication fails. Callers should inspect the returned
        /// <see cref="CdnResult"/> collection to determine success/failure status for each provider.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="page"/> is null.</exception>
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
