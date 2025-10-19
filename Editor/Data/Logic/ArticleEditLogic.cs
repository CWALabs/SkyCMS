// <copyright file="ArticleEditLogic.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Data.Logic
{
    // PATCHED: orchestrates via services; legacy method names preserved
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Cosmos.BlobService;
    using Cosmos.Cms.Common.Services.Configurations;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Cosmos.Common.Models;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;
    using SendGrid.Helpers.Errors.Model;
    using Sky.Cms.Controllers;
    using Sky.Cms.Models;
    using Sky.Cms.Services;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.Catalog;
    using Sky.Editor.Services.CDN;
    using Sky.Editor.Services.Html;
    using Sky.Editor.Services.Publishing;
    using Sky.Editor.Services.Redirects;
    using Sky.Editor.Services.Slugs;
    using Sky.Editor.Services.Titles;

    /// <summary>
    /// Article editing and management logic (editor-facing). Inherits read/view logic from <see cref="ArticleLogic"/>.
    /// Coordinates persistence, publishing, catalog updates, static artifact generation and title/slug change handling.
    /// </summary>
    public partial class ArticleEditLogic : ArticleLogic
    {
        private readonly StorageContext storageContext;
        private readonly ILogger<ArticleEditLogic> logger;
        private readonly IHttpContextAccessor accessor;
        private readonly IMemoryCache localCache;
        private readonly EditorSettings settings;

        // Service dependencies
        private readonly IClock clock;
        private readonly ISlugService slugService;
        private readonly IArticleHtmlService htmlService;
        private readonly ICatalogService catalogService;
        private readonly IPublishingService publishingService;
        private readonly ITitleChangeService titleChangeService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleEditLogic"/> class.
        /// </summary>
        /// <param name="dbContext">Application database context.</param>
        /// <param name="config">Cosmos configuration options.</param>
        /// <param name="memoryCache">Process memory cache for transient items.</param>
        /// <param name="storageContext">Blob/file storage context for static artifacts.</param>
        /// <param name="logger">Logger for diagnostic events.</param>
        /// <param name="accessor">HTTP context accessor (used for CDN integration and environment info).</param>
        /// <param name="settings">Editor (instance) settings.</param>
        /// <param name="clock">Clock abstraction for testable UTC timestamps.</param>
        /// <param name="slugService">Slug normalization service.</param>
        /// <param name="htmlService">HTML transformation / injection service.</param>
        /// <param name="catalogService">Catalog (index) maintenance service.</param>
        /// <param name="publishingService">Publishing state manager.</param>
        /// <param name="titleChangeService">Title change coordinator (redirects, child slugs, events).</param>
        /// <param name="redirectService">Redirect service (kept for DI compatibility; not directly used here).</param>
        public ArticleEditLogic(
            ApplicationDbContext dbContext,
            IOptions<CosmosConfig> config,
            IMemoryCache memoryCache,
            StorageContext storageContext,
            ILogger<ArticleEditLogic> logger,
            IHttpContextAccessor accessor,
            IEditorSettings settings,
            IClock clock,
            ISlugService slugService,
            IArticleHtmlService htmlService,
            ICatalogService catalogService,
            IPublishingService publishingService,
            ITitleChangeService titleChangeService,
            IRedirectService redirectService)
            : base(
                dbContext,
                config,
                memoryCache,
                settings.PublisherUrl,
                settings.BlobPublicUrl,
                true)
        {
            this.storageContext = storageContext ?? throw new ArgumentNullException(nameof(storageContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            this.settings = (EditorSettings)settings ?? throw new ArgumentNullException(nameof(settings));
            this.localCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.slugService = slugService ?? throw new ArgumentNullException(nameof(slugService));
            this.htmlService = htmlService ?? throw new ArgumentNullException(nameof(htmlService));
            this.catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
            this.publishingService = publishingService ?? throw new ArgumentNullException(nameof(publishingService));
            this.titleChangeService = titleChangeService ?? throw new ArgumentNullException(nameof(titleChangeService));
        }

        /// <summary>
        /// Gets the strongly-typed application database context (shadowing base protected context for convenience).
        /// </summary>
        public new ApplicationDbContext DbContext => base.DbContext;

        /// <summary>
        /// Returns the most recent published timestamp (UTC) for the specified logical article number, or null if never published.
        /// </summary>
        /// <param name="articleNumber">Logical article number.</param>
        /// <returns>Latest published <see cref="DateTimeOffset"/> or <c>null</c>.</returns>
        public async Task<DateTimeOffset?> GetLastPublishedDate(int articleNumber) =>
            await DbContext.Articles
                .Where(a => a.ArticleNumber == articleNumber && a.Published != null)
                .OrderByDescending(a => a.Published)
                .Select(a => a.Published)
                .FirstOrDefaultAsync();

        /// <summary>
        /// Retrieves a specific version (or latest) of an article by logical article number for editing contexts.
        /// </summary>
        /// <param name="articleNumber">Article number.</param>
        /// <param name="versionNumber">Target version; if null the latest version is returned.</param>
        /// <returns>Article view model or null if not found.</returns>
        public async Task<ArticleViewModel> GetArticleByArticleNumber(int articleNumber, int? versionNumber)
        {
            IQueryable<Article> q = DbContext.Articles
                .Where(a => a.ArticleNumber == articleNumber && a.StatusCode != (int)StatusCodeEnum.Deleted);

            var entity = versionNumber.HasValue
                ? await q.FirstOrDefaultAsync(a => a.VersionNumber == versionNumber.Value)
                : await q.OrderByDescending(a => a.VersionNumber).FirstOrDefaultAsync();

            return entity == null ? null : await BuildArticleViewModel(entity, "en-US");
        }

        /// <summary>
        /// Retrieves an article by row (GUID) identifier, excluding deleted versions.
        /// </summary>
        /// <param name="id">Article row ID.</param>
        /// <param name="controllerName">Legacy controller hint (unused).</param>
        /// <param name="userId">User context (unused).</param>
        /// <returns>Article view model or null.</returns>
        public async Task<ArticleViewModel> GetArticleById(Guid id, EnumControllerName controllerName, Guid userId)
        {
            var entity = await DbContext.Articles
                .FirstOrDefaultAsync(a => a.Id == id && a.StatusCode != (int)StatusCodeEnum.Deleted);
            return entity == null ? null : await BuildArticleViewModel(entity, "en-US");
        }

        /// <summary>
        /// Retrieves the latest non-deleted article by URL path (slug). Empty path is treated as root.
        /// </summary>
        /// <param name="urlPath">Slug path (or empty/root).</param>
        /// <param name="controllerName">Legacy controller hint (unused).</param>
        /// <param name="userId">User context (unused).</param>
        /// <returns>Article view model or null.</returns>
        public async Task<ArticleViewModel> GetArticleByUrl(string urlPath, EnumControllerName controllerName, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(urlPath))
            {
                urlPath = "root";
            }

            var deletedEnum = (int)StatusCodeEnum.Deleted;
            var entity = await DbContext.Articles
                .Where(a => a.UrlPath == urlPath && a.StatusCode != deletedEnum)
                .OrderByDescending(a => a.VersionNumber)
                .FirstOrDefaultAsync();

            return entity == null ? null : await BuildArticleViewModel(entity, "en-US");
        }

        /// <summary>
        /// Convenience overload returning latest article version by slug.
        /// </summary>
        /// <param name="urlPath">Slug path.</param>
        /// <returns>Article view model or null.</returns>
        public Task<ArticleViewModel> GetArticleByUrl(string urlPath) =>
            GetArticleByUrl(urlPath, EnumControllerName.Edit, Guid.Empty);

        /// <summary>
        /// Convenience overload with a (currently ignored) published-only flag for API symmetry.
        /// </summary>
        /// <param name="urlPath">Slug path.</param>
        /// <param name="publishedOnly">If true would filter to published; ignored in editor mode.</param>
        /// <returns>Article view model or null.</returns>
        public Task<ArticleViewModel> GetArticleByUrl(string urlPath, bool publishedOnly) =>
            GetArticleByUrl(urlPath, EnumControllerName.Edit, Guid.Empty);

        /// <summary>
        /// Retrieves reserved paths (static + database-defined) for title validation. Results cached briefly.
        /// </summary>
        /// <returns>List of reserved path records.</returns>
        public async Task<List<ReservedPath>> GetReservedPaths()
        {
            const string cacheKey = "reserved-paths";
            if (localCache.TryGetValue(cacheKey, out List<ReservedPath> cached))
            {
                return cached;
            }

            var staticReserved = new List<ReservedPath>
            {
                new () { Path = "root", CosmosRequired = true, Notes = "Home page alias" },
                new () { Path = "admin", CosmosRequired = true, Notes = "Admin path" },
                new () { Path = "account", CosmosRequired = true, Notes = "Identity path" },
                new () { Path = "login", CosmosRequired = true, Notes = "Identity path" },
                new () { Path = "logout", CosmosRequired = true, Notes = "Identity path" },
                new () { Path = "register", CosmosRequired = true, Notes = "Identity path" },
                new () { Path = "blog", CosmosRequired = true, Notes = "Blog root" },
                new () { Path = "blog/rss", CosmosRequired = true, Notes = "Blog RSS" },
                new () { Path = "api", CosmosRequired = true, Notes = "API route" },
                new () { Path = "pub", CosmosRequired = true, Notes = "Static assets" },
                new () { Path = "rss", CosmosRequired = true, Notes = "RSS" },
                new () { Path = "sitemap.xml", CosmosRequired = true, Notes = "Sitemap" }
            };

            List<ReservedPath> dbReserved;
            try
            {
                dbReserved = await DbContext.Set<ReservedPath>().ToListAsync();
            }
            catch
            {
                dbReserved = new List<ReservedPath>();
            }

            var result = staticReserved
                .Concat(dbReserved)
                .GroupBy(r => r.Path, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(r => r.Path)
                .ToList();

            localCache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }

        /// <summary>
        /// Returns redirect items (articles whose status represents redirect entries).
        /// </summary>
        /// <returns>Queryable redirect view models.</returns>
        public IQueryable<RedirectItemViewModel> GetArticleRedirects() =>
            DbContext.Articles
                .Where(p => p.StatusCode == (int)StatusCodeEnum.Redirect)
                .Select(p => new RedirectItemViewModel
                {
                    Id = p.Id,
                    FromUrl = p.UrlPath,
                    ToUrl = p.BannerImage,
                });

        /// <summary>
        /// Produces a standalone HTML document for the provided article view model (no sanitization beyond what is stored).
        /// </summary>
        /// <param name="article">Article model.</param>
        /// <param name="blobPublicUri">Public blob root (unused presently).</param>
        /// <param name="renderer">Optional renderer (unused; placeholder for future layout wrapping).</param>
        /// <returns>HTML string (empty if model null).</returns>
        public async Task<string> ExportArticle(ArticleViewModel article, Uri blobPublicUri, IViewRenderService renderer)
        {
            if (article == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder()
                .AppendLine("<!DOCTYPE html>")
                .AppendLine("<html lang=\"en\">\n<head>")
                .AppendLine("<meta charset=\"utf-8\" />")
                .AppendLine("<title>" + System.Net.WebUtility.HtmlEncode(article.Title) + "</title>");

            if (!string.IsNullOrWhiteSpace(article.HeadJavaScript))
            {
                sb.AppendLine(article.HeadJavaScript);
            }

            sb.AppendLine("</head><body>")
              .AppendLine(article.Content);

            if (!string.IsNullOrWhiteSpace(article.FooterJavaScript))
            {
                sb.AppendLine(article.FooterJavaScript);
            }

            sb.AppendLine("</body></html>");
            return await Task.FromResult(sb.ToString());
        }

        /// <summary>
        /// Validates whether a proposed title is usable (not reserved and not used by a different article).
        /// </summary>
        /// <param name="title">Proposed title.</param>
        /// <param name="articleNumber">Current article number (null when creating new).</param>
        /// <returns>True if available; false if conflict.</returns>
        public async Task<bool> ValidateTitle(string title, int? articleNumber)
        {
            var reservedPaths = (await GetReservedPaths()).Select(s => s.Path.ToLower()).ToArray();
            foreach (var reservedPath in reservedPaths)
            {
                if (reservedPath.EndsWith('*'))
                {
                    var value = reservedPath.TrimEnd('*');
                    if (title.ToLower().StartsWith(value))
                    {
                        return false;
                    }
                }
                else if (title.Equals(reservedPath, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            Article article = articleNumber.HasValue
                ? await DbContext.Articles.FirstOrDefaultAsync(a =>
                    a.ArticleNumber != articleNumber &&
                    a.Title.ToLower() == title.Trim().ToLower() &&
                    a.StatusCode != (int)StatusCodeEnum.Deleted)
                : await DbContext.Articles.FirstOrDefaultAsync(a =>
                    a.Title.ToLower() == title.Trim().ToLower() &&
                    a.StatusCode != (int)StatusCodeEnum.Deleted);

            return article == null;
        }

        /// <summary>
        /// Creates a new article (optionally from template) and returns its initial editable view model.
        /// First article becomes the root and is auto-published.
        /// </summary>
        /// <param name="title">Title text.</param>
        /// <param name="userId">Author user id.</param>
        /// <param name="templateId">Optional template ID.</param>
        /// <param name="blogKey">Optional blog key (default "default").</param>
        /// <returns>Article view model for editing.</returns>
        public async Task<ArticleViewModel> CreateArticle(string title, Guid userId, Guid? templateId = null, string blogKey = "default")
        {
            var isFirstArticle = (await DbContext.Articles.CountAsync()) == 0;
            var defaultTemplate = string.Empty;

            if (templateId.HasValue)
            {
                var template = await DbContext.Templates.FirstOrDefaultAsync(f => f.Id == templateId.Value);
                if (template != null)
                {
                    var content = htmlService.EnsureEditableMarkers(template.Content);
                    if (!content.Equals(template.Content))
                    {
                        template.Content = content;
                        await DbContext.SaveChangesAsync();
                    }

                    defaultTemplate = template.Content;
                }
            }

            if (string.IsNullOrEmpty(defaultTemplate))
            {
                defaultTemplate =
                    "<div style='width: 100%;padding-left: 20px;padding-right: 20px;margin-left: auto;margin-right: auto;'>" +
                    "<div contenteditable='true'><h1>Why Lorem Ipsum?</h1><p>" +
                    LoremIpsum.WhyLoremIpsum + "</p></div></div></div>";
            }

            int nextArticleNumber = isFirstArticle
                ? 1
                : (await DbContext.ArticleNumbers.MaxAsync(m => m.LastNumber)) + 1;

            title = title.Trim('/');

            var article = new Article
            {
                BlogKey = blogKey,
                ArticleNumber = nextArticleNumber,
                Content = htmlService.EnsureEditableMarkers(defaultTemplate),
                StatusCode = (int)StatusCodeEnum.Active,
                Title = title,
                Updated = DateTimeOffset.UtcNow,
                UrlPath = isFirstArticle ? "root" : slugService.Normalize(title),
                VersionNumber = 1,
                Published = isFirstArticle ? DateTimeOffset.UtcNow : null,
                UserId = userId.ToString(),
                TemplateId = templateId,
                BannerImage = string.Empty
            };

            DbContext.Articles.Add(article);
            DbContext.ArticleNumbers.Add(new ArticleNumber { LastNumber = nextArticleNumber });
            await DbContext.SaveChangesAsync();

            await UpsertCatalogEntry(article);

            if (isFirstArticle)
            {
                await PublishArticle(article.Id, DateTimeOffset.UtcNow);
            }

            return await BuildArticleViewModel(article, "en-US");
        }

        /// <summary>
        /// Gets (or creates) a catalog entry for an article view model identifier.
        /// </summary>
        /// <param name="model">Article view model referencing an article ID.</param>
        /// <returns>Catalog entry.</returns>
        public async Task<CatalogEntry> GetCatalogEntry(ArticleViewModel model)
        {
            var article = await DbContext.Articles.FirstOrDefaultAsync(f => f.Id == model.Id);
            return await GetCatalogEntry(article);
        }

        /// <summary>
        /// Gets (or creates) a catalog entry for an article entity.
        /// </summary>
        /// <param name="article">Article entity.</param>
        /// <returns>Catalog entry.</returns>
        public async Task<CatalogEntry> GetCatalogEntry(Article article)
        {
            var entry = await DbContext.ArticleCatalog
                .FirstOrDefaultAsync(f => f.ArticleNumber == article.ArticleNumber);

            return entry ?? await UpsertCatalogEntry(article);
        }

        /// <summary>
        /// Reassigns the root (home) page to the specified article number and republish both old and new root pages.
        /// </summary>
        /// <param name="model">New home page request model.</param>
        /// <returns>Awaitable task.</returns>
        public async Task CreateHomePage(NewHomeViewModel model)
        {
            var oldHomeArticle = await DbContext.Articles
                .Where(w => w.UrlPath.ToLower() == "root").ToListAsync();
            if (oldHomeArticle.Count == 0)
            {
                throw new ArgumentException("No existing home page found.");
            }

            var newHomeArticle = await DbContext.Articles
                .Where(w => w.ArticleNumber == model.ArticleNumber).ToListAsync();
            if (newHomeArticle.Count == 0)
            {
                throw new ArgumentException("New home page not found.");
            }

            var newUrl = slugService.Normalize(oldHomeArticle.First().Title);
            foreach (var article in oldHomeArticle)
            {
                article.UrlPath = newUrl;
            }

            await DbContext.SaveChangesAsync();

            foreach (var article in newHomeArticle)
            {
                article.UrlPath = "root";
            }

            await DbContext.SaveChangesAsync();

            var oldHome = oldHomeArticle
                .OrderBy(o => o.VersionNumber)
                .LastOrDefault(f => f.Published.HasValue);
            var newHome = newHomeArticle
                .OrderBy(o => o.VersionNumber)
                .LastOrDefault(f => f.Published.HasValue);

            await PublishArticle(oldHome.Id, DateTimeOffset.UtcNow);
            await UpsertCatalogEntry(oldHome);

            await PublishArticle(newHome.Id, DateTimeOffset.UtcNow);
            await UpsertCatalogEntry(newHome);
        }

        /// <summary>
        /// Soft-deletes (trashes) all versions of an article and removes related published artifacts and catalog entry.
        /// </summary>
        /// <param name="articleNumber">Target article number.</param>
        /// <returns>Awaitable task.</returns>
        public async Task DeleteArticle(int articleNumber)
        {
            var doomed = await DbContext.Articles
                .Where(w => w.ArticleNumber == articleNumber).ToListAsync();
            var url = doomed.FirstOrDefault()?.UrlPath;

            if (doomed == null || doomed.Count == 0)
            {
                throw new KeyNotFoundException($"Article number {articleNumber} not found.");
            }

            if (doomed.Exists(a => a.UrlPath.Equals("root", StringComparison.OrdinalIgnoreCase)))
            {
                throw new NotSupportedException("Cannot trash the home page. Replace it then delete.");
            }

            foreach (var article in doomed)
            {
                article.StatusCode = (int)StatusCodeEnum.Deleted;
            }

            var doomedPages = await DbContext.Pages
                .Where(w => w.ArticleNumber == articleNumber).ToListAsync();
            DbContext.Pages.RemoveRange(doomedPages);

            await DbContext.SaveChangesAsync();
            await DeleteCatalogEntry(articleNumber);
            DeleteStaticWebpage(url);
            await publishingService.WriteTocAsync();
        }

        /// <summary>
        /// Restores a previously deleted article (all versions) to active status, assigning new title if conflict exists.
        /// </summary>
        /// <param name="articleNumber">Article number.</param>
        /// <param name="userId">User restoring the article (unused currently).</param>
        /// <returns>Awaitable task.</returns>
        public async Task RestoreArticle(int articleNumber, string userId)
        {
            var redeemed = await DbContext.Articles
                .Where(w => w.ArticleNumber == articleNumber).ToListAsync();
            if (redeemed == null || redeemed.Count == 0)
            {
                throw new KeyNotFoundException($"Article number {articleNumber} not found.");
            }

            var title = redeemed.First().Title.ToLower();
            if (await DbContext.Articles.Where(a =>
                    a.Title.ToLower() == title &&
                    a.ArticleNumber != articleNumber &&
                    a.StatusCode == (int)StatusCodeEnum.Deleted).CosmosAnyAsync())
            {
                var newTitle = title + " (" + await DbContext.Articles.CountAsync() + ")";
                var url = slugService.Normalize(newTitle);
                foreach (var article in redeemed)
                {
                    article.Title = newTitle;
                    article.UrlPath = url;
                    article.StatusCode = (int)StatusCodeEnum.Active;
                    article.Published = null;
                }
            }
            else
            {
                foreach (var article in redeemed)
                {
                    article.StatusCode = (int)StatusCodeEnum.Active;
                    article.Published = null;
                }
            }

            var sample = redeemed.First();
            DbContext.ArticleCatalog.Add(new CatalogEntry
            {
                ArticleNumber = sample.ArticleNumber,
                Published = null,
                Status = "Active",
                Title = sample.Title,
                Updated = DateTimeOffset.UtcNow,
                UrlPath = sample.UrlPath
            });
            await DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Saves user edits to an existing article version, applies title change workflow and catalog update.
        /// </summary>
        /// <param name="model">Incoming article edit view model.</param>
        /// <param name="userId">User performing the save.</param>
        /// <returns>Update result including CDN purge info (if any).</returns>
        public async Task<ArticleUpdateResult> SaveArticle(ArticleViewModel model, Guid userId)
        {
            var article = await DbContext.Articles
                .OrderByDescending(o => o.VersionNumber)
                .FirstOrDefaultAsync(a => a.ArticleNumber == model.ArticleNumber);

            if (article == null)
            {
                throw new NotFoundException($"Article ID: {model.Id} not found.");
            }

            var oldTitle = article.Title;

            model.Content = htmlService.EnsureEditableMarkers(model.Content);

            htmlService.EnsureAngularBase(model.HeadJavaScript, model.UrlPath ?? string.Empty);

            article.Content = model.Content;
            article.Title = model.Title;
            article.Updated = clock.UtcNow;
            article.HeaderJavaScript = model.HeadJavaScript;
            article.FooterJavaScript = model.FooterJavaScript;
            article.BannerImage = model.BannerImage ?? string.Empty;
            article.UserId = userId.ToString();
            article.ArticleType = (int)model.ArticleType;
            article.Category = model.Category ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(model.Introduction))
            {
                article.Introduction = model.Introduction;
            }

            var saved = false;
            for (int attempt = 0; attempt < 2 && !saved; attempt++)
            {
                try
                {
                    await DbContext.SaveChangesAsync();
                    saved = true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (attempt == 1)
                    {
                        throw;
                    }

                    DbContext.Entry(article).Reload();
                }
            }

            await titleChangeService.HandleTitleChangeAsync(article, oldTitle);
            await catalogService.UpsertAsync(article);

            if (article.Published.HasValue)
            {
                var cdnResults = await UpsertPublishedPage(article.Id);
                return new ArticleUpdateResult
                {
                    ServerSideSuccess = true,
                    Model = model,
                    CdnResults = cdnResults
                };
            }

            return new ArticleUpdateResult
            {
                ServerSideSuccess = true,
                Model = model,
                CdnResults = new List<CdnResult>()
            };
        }

        /// <summary>
        /// Publishes the specified article version (unpublishing others), updates catalog, and refreshes published artifacts.
        /// </summary>
        /// <param name="articleId">Article row ID.</param>
        /// <param name="dateTime">Optional explicit publish time (UTC); if null current time is used.</param>
        /// <returns>List of CDN purge results (empty if none).</returns>
        public async Task<List<CdnResult>> PublishArticle(Guid articleId, DateTimeOffset? dateTime)
        {
            var article = await DbContext.Articles.FirstOrDefaultAsync(a => a.Id == articleId);
            if (article == null)
            {
                return new List<CdnResult>();
            }

            article.Published = dateTime ?? clock.UtcNow;
            var cdnResults = await publishingService.PublishAsync(article);
            await UpsertCatalogEntry(article);
            //var cdnResults = await UpsertPublishedPage(article.Id) ?? new List<CdnResult>();
            return cdnResults;
        }

        /// <summary>
        /// Retrieves (or creates) cached author info for a given user id.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>Author info or null if user not found.</returns>
        private async Task<AuthorInfo> GetAuthorInfoForUserId(Guid userId)
        {
            var key = userId.ToString();
            var cacheKey = "authorinfo:" + key;
            if (localCache.TryGetValue(cacheKey, out AuthorInfo cached))
            {
                return cached;
            }

            var existing = await DbContext.AuthorInfos.FirstOrDefaultAsync(a => a.Id == key);
            if (existing == null)
            {
                var identity = await DbContext.Users.FirstOrDefaultAsync(u => u.Id == key);
                if (identity == null)
                {
                    return null;
                }

                existing = new AuthorInfo
                {
                    Id = key,
                    AuthorName = identity.UserName ?? identity.Email ?? key,
                    AuthorDescription = string.Empty
                };
                DbContext.AuthorInfos.Add(existing);
                await DbContext.SaveChangesAsync();
            }

            localCache.Set(cacheKey, existing, TimeSpan.FromMinutes(10));
            return existing;
        }

        /// <summary>
        /// Deletes a static HTML page artifact if static mode is enabled (except under /pub which is protected).
        /// </summary>
        /// <param name="filePath">File path or slug (root -> index.html).</param>
        private void DeleteStaticWebpage(string filePath)
        {
            if (!settings.StaticWebPages)
            {
                return;
            }

            if (filePath.StartsWith("/pub", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Cannot remove web page from path /pub.");
            }

            filePath = filePath.Equals("root", StringComparison.OrdinalIgnoreCase) ? "/index.html" : filePath;
            storageContext.DeleteFile(filePath);
        }

        /// <summary>
        /// Upserts (replaces) the published page record for a given article version, regenerates static artifacts and purges CDN.
        /// </summary>
        /// <param name="id">Article version ID.</param>
        /// <returns>List of CDN purge results or null.</returns>
        private async Task<List<CdnResult>> UpsertPublishedPage(Guid id)
        {
            var doomed = await DbContext.Pages
                .Where(w => w.Content == "" || w.Title == "").ToListAsync();
            if (doomed.Any())
            {
                DbContext.Pages.RemoveRange(doomed);
                await DbContext.SaveChangesAsync();
            }

            var newVersion = await DbContext.Articles
                .FirstOrDefaultAsync(w => w.Id == id && w.Published != null);

            var articleNumber = newVersion.ArticleNumber;
            var publishedVersions = await DbContext.Pages.Where(
                w => w.ArticleNumber == articleNumber &&
                     w.StatusCode != (int)StatusCodeEnum.Redirect).ToListAsync();

            if (publishedVersions.Count > 0)
            {
                foreach (var item in publishedVersions)
                {
                    DbContext.Pages.Remove(item);
                    await DbContext.SaveChangesAsync();
                    DeleteStaticWebpage(item.UrlPath);
                }
            }

            var authorInfo = await GetAuthorInfoForUserId(Guid.Parse(newVersion.UserId));
            var newPage = new PublishedPage
            {
                ArticleNumber = newVersion.ArticleNumber,
                BannerImage = newVersion.BannerImage,
                Content = newVersion.Content,
                Expires = newVersion.Expires,
                FooterJavaScript = newVersion.FooterJavaScript,
                HeaderJavaScript = newVersion.HeaderJavaScript,
                Id = Guid.NewGuid(),
                Published = newVersion.Published,
                StatusCode = newVersion.StatusCode,
                Title = newVersion.Title,
                Updated = newVersion.Updated,
                UrlPath = newVersion.UrlPath,
                ParentUrlPath = newVersion.UrlPath.Substring(0, Math.Max(newVersion.UrlPath.LastIndexOf('/'), 0)),
                VersionNumber = newVersion.VersionNumber,
                AuthorInfo = authorInfo == null ? string.Empty : JsonConvert.SerializeObject(authorInfo).Replace("\"", "'"),
                ArticleType = newVersion.ArticleType,
                Category = newVersion.Category,
                Introduction = newVersion.Introduction,
                BlogKey = newVersion.BlogKey,
            };

            DbContext.Pages.Add(newPage);
            await DbContext.SaveChangesAsync();

            var purgePaths = new List<string>
            {
                newVersion.UrlPath.Equals("root", StringComparison.OrdinalIgnoreCase)
                    ? "/"
                    : $"{settings.PublisherUrl.TrimEnd('/')}/{newVersion.UrlPath.TrimStart('/')}"
            };

            await UpsertCatalogEntry(newVersion);
            await publishingService.WriteTocAsync();

            if (purgePaths.Count > 0)
            {
                var cdnService = CdnService.GetCdnService(DbContext, logger, accessor.HttpContext);
                try
                {
                    return await cdnService.PurgeCdn(purgePaths);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "CDN purge failed.");
                }
            }

            return null;
        }

        /// <summary>
        /// Creates or replaces a catalog entry for the supplied article based on current top version state.
        /// Generates an introduction if missing.
        /// </summary>
        /// <param name="article">Article entity reference.</param>
        /// <returns>Up-to-date catalog entry.</returns>
        private async Task<CatalogEntry> UpsertCatalogEntry(Article article)
        {
            var lastVersion = await DbContext.Articles
                .Where(a => a.ArticleNumber == article.ArticleNumber)
                .OrderByDescending(o => o.VersionNumber)
                .FirstOrDefaultAsync();

            var userId = lastVersion?.UserId ?? article.UserId;
            var authorInfo = await GetAuthorInfoForUserId(Guid.Parse(userId));

            if (string.IsNullOrWhiteSpace(article.Introduction))
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(lastVersion?.Content))
                    {
                        var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                        htmlDoc.LoadHtml(lastVersion.Content);
                        var paragraphs = htmlDoc.DocumentNode.SelectNodes("//p");
                        var first = paragraphs?
                            .FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.InnerText));
                        if (first != null)
                        {
                            var intro = first.InnerText.Trim();
                            article.Introduction = intro.Length > 512 ? intro[..512] : intro;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error parsing article content during catalog entry.");
                }
            }

            var oldEntry = await DbContext.ArticleCatalog
                .FirstOrDefaultAsync(f => f.ArticleNumber == article.ArticleNumber);
            if (oldEntry != null)
            {
                DbContext.ArticleCatalog.Remove(oldEntry);
                await DbContext.SaveChangesAsync();
            }

            var entry = new CatalogEntry
            {
                ArticleNumber = article.ArticleNumber,
                BannerImage = article.BannerImage,
                Published = article.Published,
                Status = article.StatusCode == (int)StatusCodeEnum.Active ? "Active" : "Inactive",
                Title = article.Title,
                Updated = article.Updated,
                UrlPath = article.UrlPath,
                TemplateId = article.TemplateId,
                AuthorInfo = authorInfo == null ? string.Empty : JsonConvert.SerializeObject(authorInfo).Replace("\"", "'"),
                Introduction = article.Introduction,
                BlogKey = article.BlogKey,
            };

            DbContext.ArticleCatalog.Add(entry);
            await DbContext.SaveChangesAsync();
            return entry;
        }

        /// <summary>
        /// Deletes a catalog entry (if it exists) for a logical article number.
        /// </summary>
        /// <param name="articleNumber">Article number.</param>
        /// <returns>Awaitable task.</returns>
        private async Task DeleteCatalogEntry(int articleNumber)
        {
            var catalogEntry = await DbContext.ArticleCatalog
                .FirstOrDefaultAsync(f => f.ArticleNumber == articleNumber);
            if (catalogEntry != null)
            {
                DbContext.ArticleCatalog.Remove(catalogEntry);
                await DbContext.SaveChangesAsync();
            }
        }
    }
}