// <copyright file="ArticleEditLogic.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Data.Logic
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using Azure.ResourceManager;
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
    using Sky.Editor.Services.CDN;
    using X.Web.Sitemap.Extensions;

    /// <summary>
    ///     Article Editor Logic.
    /// </summary>
    /// <remarks>
    ///     Is derived from base class <see cref="ArticleLogic" />, adds on content editing functionality.
    /// </remarks>
    public class ArticleEditLogic : ArticleLogic
    {
        private readonly IViewRenderService viewRenderService;
        private readonly StorageContext storageContext;
        private readonly ILogger<ArticleEditLogic> logger;
        private readonly IHttpContextAccessor accessor;
        private readonly EditorSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleEditLogic"/> class.
        ///     Constructor.
        /// </summary>
        /// <param name="dbContext">Database context.</param>
        /// <param name="memoryCache">Memory cache.</param>
        /// <param name="config">Cosmos configuration.</param>
        /// <param name="viewRenderService">View rendering service used to save static web pages.</param>
        /// <param name="storageContext">Storage service used to manage static website blobs.</param>
        /// <param name="logger">Log service.</param>
        /// <param name="accessor">Http context access.</param>
        /// <param name="settings">Editor settings - used with multitenant editor.</param>
        public ArticleEditLogic(
            ApplicationDbContext dbContext,
            IMemoryCache memoryCache,
            IOptions<CosmosConfig> config,
            IViewRenderService viewRenderService,
            StorageContext storageContext,
            ILogger<ArticleEditLogic> logger,
            IHttpContextAccessor accessor,
            IEditorSettings settings)
            : base(
                dbContext,
                config,
                memoryCache,
                ((EditorSettings)settings).PublisherUrl,
                ((EditorSettings)settings).BlobPublicUrl,
                true)
        {
            this.viewRenderService = viewRenderService;
            this.storageContext = storageContext;
            this.logger = logger;
            this.accessor = accessor;
            this.settings = (EditorSettings)settings;
        }

        /// <summary>
        ///     Gets database Context with Synchronize Context.
        /// </summary>
        public new ApplicationDbContext DbContext => base.DbContext;

        /// <summary>
        ///     Validate that the title is not already taken by another article.
        /// </summary>
        /// <param name="title">Title.</param>
        /// <param name="articleNumber">Current article number.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// If article number is given, this checks  all other article
        /// numbers to see if this title is already taken.
        /// If not given, this method returns true if article name already in use.
        /// </remarks>
        public async Task<bool> ValidateTitle(string title, int? articleNumber)
        {
            // Make sure it doesn't conflict with the publi blob path
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
                else if (title.ToLower() == reservedPath.ToLower())
                {
                    return false;
                }
            }

            Article article;
            if (articleNumber.HasValue)
            {
                article = await DbContext.Articles.FirstOrDefaultAsync(a =>
                    a.ArticleNumber != articleNumber && // look only at other article numbers
                    a.Title.ToLower() == title.Trim().ToLower() && // Is the title used already
                    a.StatusCode != (int)StatusCodeEnum.Deleted); // and the page is active (active or is inactive)
            }
            else
            {
                article = await DbContext.Articles.FirstOrDefaultAsync(a =>
                    a.Title.ToLower() == title.Trim().ToLower() && // Is the title used already
                    a.StatusCode != (int)StatusCodeEnum.Deleted); // and the page is active (active or is inactive)
            }

            if (article == null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Creates a new article, save it to the database before returning a copy for editing.
        /// </summary>
        /// <param name="title">Article title.</param>
        /// <param name="userId">ID of the user creating the page.</param>
        /// <param name="templateId">Page template ID.</param>
        /// <returns>Unsaved article ready to edit and save.</returns>
        /// <remarks>
        ///     <para>
        ///         Creates a new article, saves it to the database, and is ready to edit.  Uses <see cref="ArticleLogic.GetDefaultLayout" /> to get the
        ///         layout,
        ///         and builds the <see cref="ArticleViewModel" /> using method
        ///         <seealso cref="ArticleLogic.BuildArticleViewModel(Article, string, bool)" />. Creates a new article number.
        ///     </para>
        ///     <para>
        ///         If a template ID is given, the contents of this article is loaded with content from the <see cref="Template" />.
        ///     </para>
        ///     <para>
        ///         If this is the first article, it is saved as root and published immediately.
        ///     </para>
        ///     <para>
        ///         Also creates a catalog entry for the article.
        ///     </para>
        /// </remarks>
        public async Task<ArticleViewModel> CreateArticle(string title, Guid userId, Guid? templateId = null)
        {
            // Is this the first article? If so, make it the root and publish it.
            var isFirstArticle = (await DbContext.Articles.CountAsync()) == 0;

            var defaultTemplate = string.Empty;

            if (templateId.HasValue)
            {
                var templates = await DbContext.Templates.ToListAsync();
                var template = await DbContext.Templates.FirstOrDefaultAsync(f => f.Id == templateId.Value);

                // For backward compatibility, make sure the templates are properly marked.
                // This ensures if the template are updated, the pages that use this page are properly updated.
                var content = Ensure_ContentEditable_IsMarked(template.Content);
                if (!content.Equals(template.Content))
                {
                    template.Content = content;
                    await DbContext.SaveChangesAsync();
                }

                defaultTemplate = template.Content;
            }

            if (string.IsNullOrEmpty(defaultTemplate))
            {
                defaultTemplate = "<div style='width: 100%;padding-left: 20px;padding-right: 20px;margin-left: auto;margin-right: auto;'>" +
                                  "<div contenteditable='true'><h1>Why Lorem Ipsum?</h1><p>" +
                                   LoremIpsum.WhyLoremIpsum + "</p></div>" +
                                  "</div>" +
                                  "</div>";
            }

            // Max returns the incorrect result.
            int nextArticleNumber = isFirstArticle ? 1 : (await DbContext.ArticleNumbers.MaxAsync(m => m.LastNumber)) + 1;

            // New article
            title = title.Trim('/');

            var article = new Article()
            {
                ArticleNumber = nextArticleNumber,
                Content = Ensure_ContentEditable_IsMarked(defaultTemplate),
                StatusCode = (int)StatusCodeEnum.Active,
                Title = title,
                Updated = DateTimeOffset.Now,
                UrlPath = isFirstArticle ? "root" : NormailizeArticleUrl(title),
                VersionNumber = 1,
                Published = isFirstArticle ? DateTimeOffset.UtcNow : null,
                UserId = userId.ToString(),
                TemplateId = templateId,
                BannerImage = string.Empty,

            };

            DbContext.Articles.Add(article);
            DbContext.ArticleNumbers.Add(new ArticleNumber()
            {
                LastNumber = nextArticleNumber
            });

            await DbContext.SaveChangesAsync();

            // Update the catalog.
            await UpsertCatalogEntry(article);

            if (isFirstArticle)
            {
                await PublishArticle(article.Id, DateTimeOffset.UtcNow);

            }

            return await BuildArticleViewModel(article, "en-US");
        }

        /// <summary>
        ///   Gets or creates a catalog entry for an article.
        /// </summary>
        /// <param name="model">ArticleViewModel.</param>
        /// <returns>CatalogEntry.</returns>
        public async Task<CatalogEntry> GetCatalogEntry(ArticleViewModel model)
        {
            var article = await DbContext.Articles.FirstOrDefaultAsync(f => f.Id == model.Id);

            return await GetCatalogEntry(article);
        }

        /// <summary>
        /// Gets or creates a catalog entry for an article.
        /// </summary>
        /// <param name="article">Article for which to get the entry.</param>
        /// <returns>CatalogEntry.</returns>
        public async Task<CatalogEntry> GetCatalogEntry(Article article)
        {
            var entry = await DbContext.ArticleCatalog.FirstOrDefaultAsync(f => f.ArticleNumber == article.ArticleNumber);

            if (entry == null)
            {
                entry = await UpsertCatalogEntry(article);
            }

            return entry;
        }

        /// <summary>
        ///     Makes an article the new home page and updates the page catalog.
        /// </summary>
        /// <param name="model">New home page post model.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task CreateHomePage(NewHomeViewModel model)
        {
            // Remove the old page from home
            var oldHomeArticle = await DbContext.Articles.Where(w => w.UrlPath.ToLower() == "root").ToListAsync();

            if (oldHomeArticle.Count == 0)
            {
                throw new ArgumentException("No existing home page found.");
            }

            // New page that will become page root
            var newHomeArticle = await DbContext.Articles.Where(w => w.ArticleNumber == model.ArticleNumber).ToListAsync();

            if (newHomeArticle.Count == 0)
            {
                throw new ArgumentException("New home page not found.");
            }

            // Change the path of the old home page (no longer 'root').
            var newUrl = NormailizeArticleUrl(oldHomeArticle.FirstOrDefault()?.Title);
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

            var oldHome = oldHomeArticle.OrderBy(o => o.VersionNumber).LastOrDefault(f => f.Published.HasValue);
            var newHome = newHomeArticle.OrderBy(o => o.VersionNumber).LastOrDefault(f => f.Published.HasValue);

            // Publish the old home page as a regular page (also update catalog entry).
            await PublishArticle(oldHome.Id, DateTimeOffset.UtcNow);
            await UpsertCatalogEntry(oldHome);

            // Publish the new home page as a regular page (also update catalog entry).
            await PublishArticle(newHome.Id, DateTimeOffset.UtcNow);
            await UpsertCatalogEntry(newHome);
        }

        /// <summary>
        ///     This method puts an article into trash, and, all its versions.
        /// </summary>
        /// <param name="articleNumber">Article number.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        ///     <para>This method puts an article into trash. Use <see cref="RestoreArticle" /> to restore an article. </para>
        ///     <para>It also removes it from the page catalog and any published pages..</para>
        ///     <para>WARNING: Make sure the menu MenuController.Index does not reference deleted files.</para>
        /// </remarks>
        public async Task DeleteArticle(int articleNumber)
        {
            var doomed = await DbContext.Articles.Where(w => w.ArticleNumber == articleNumber).ToListAsync();
            var url = doomed.FirstOrDefault()?.UrlPath;

            if (doomed == null)
            {
                throw new KeyNotFoundException($"Article number {articleNumber} not found.");
            }

            if (doomed.Exists(a => a.UrlPath.ToLower() == "root"))
            {
                throw new NotSupportedException(
                    "Cannot trash the home page.  Replace home page with another, then send to trash.");
            }

            foreach (var article in doomed)
            {
                article.StatusCode = (int)StatusCodeEnum.Deleted;
            }

            var doomedPages = await DbContext.Pages.Where(w => w.ArticleNumber == articleNumber).ToListAsync();
            DbContext.Pages.RemoveRange(doomedPages);

            await DbContext.SaveChangesAsync();
            await DeleteCatalogEntry(articleNumber);
            DeleteStaticWebpage(url);
            await CreateStaticTableOfContentsJsonFile();
        }

        /// <summary>
        ///     Retrieves and article and all its versions from trash.
        /// </summary>
        /// <param name="articleNumber">Article number.</param>
        /// <param name="userId">Current user ID.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        ///     <para>
        ///         Please be aware of the following:
        ///     </para>
        ///     <list type="bullet">
        ///         <item><see cref="Article.StatusCode" /> is set to <see cref="StatusCodeEnum.Active" />.</item>
        ///         <item><see cref="Article.Title" /> will be altered if a live article exists with the same title.</item>
        ///         <item>
        ///             If the title changed, the <see cref="Article.UrlPath" /> will be updated using
        ///             <see cref="NormailizeArticleUrl" />.
        ///         </item>
        ///         <item>The article and all its versions are set to unpublished (<see cref="Article.Published" /> set to null).</item>
        ///         <item>Article is added back to the article catalog.</item>
        ///     </list>
        /// </remarks>
        public async Task RestoreArticle(int articleNumber, string userId)
        {
            var redeemed = await DbContext.Articles.Where(w => w.ArticleNumber == articleNumber).ToListAsync();

            if (redeemed == null || redeemed.Count == 0)
            {
                throw new KeyNotFoundException($"Article number {articleNumber} not found.");
            }

            var title = redeemed.FirstOrDefault()?.Title.ToLower();

            // Avoid restoring an article that has a title that collides with a live article.
            if (await DbContext.Articles.Where(a =>
                a.Title.ToLower() == title && a.ArticleNumber != articleNumber &&
                a.StatusCode == (int)StatusCodeEnum.Deleted).CosmosAnyAsync())
            {
                var newTitle = title + " (" + await DbContext.Articles.CountAsync() + ")";
                var url = NormailizeArticleUrl(newTitle);
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

            // Add back to the catalog
            var sample = redeemed.FirstOrDefault();
            DbContext.ArticleCatalog.Add(new CatalogEntry()
            {
                ArticleNumber = sample.ArticleNumber,
                Published = null,
                Status = "Active",
                Title = sample.Title,
                Updated = DateTimeOffset.Now,
                UrlPath = sample.UrlPath
            });

            await DbContext.SaveChangesAsync();
        }

        /// <summary>
        ///     Updates an existing article, or inserts a new one.
        /// </summary>
        /// <param name="model">Article view model.</param>
        /// <param name="userId">ID of the current user.</param>
        /// <remarks>
        ///     <para>
        ///         If the article number is '0', a new article is inserted.  If a version number is '0', then
        ///         a new version is created. Recreates <see cref="ArticleViewModel" /> using method
        ///         <see cref="ArticleLogic.BuildArticleViewModel(Article, string, bool)" />.
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             Published articles will trigger the prior published article to have its Expired property set to this
        ///             article's published property.
        ///         </item>
        ///         <item>
        ///             Title changes (and redirects) are handled by adding a new article with redirect info.
        ///         </item>
        ///         <item>
        ///             The <see cref="ArticleViewModel" /> that is returned, is rebuilt using
        ///             <see cref="ArticleLogic.BuildArticleViewModel(Article, string, bool)" />.
        ///         </item>
        ///         <item>
        ///             Creates or updates the catalog entry.
        ///         </item>
        ///         <item>
        ///            <see cref="Article.Updated"/> property is automatically updated with current UTC date and time.
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<ArticleUpdateResult> SaveArticle(ArticleViewModel model, Guid userId)
        {
            var article = await DbContext.Articles.OrderByDescending(o => o.VersionNumber).FirstOrDefaultAsync(a => a.ArticleNumber == model.ArticleNumber);
            if (article == null)
            {
                throw new NotFoundException($"Article ID: {model.Id} not found.");
            }
            string oldTitle = article.Title;
            model.Content = Ensure_ContentEditable_IsMarked(model.Content);
            UpdateHeadBaseTag(model);
            article.Content = model.Content;
            article.Title = model.Title;
            article.Updated = DateTimeOffset.UtcNow;
            article.HeaderJavaScript = model.HeadJavaScript;
            article.FooterJavaScript = model.FooterJavaScript;
            article.BannerImage = model.BannerImage ?? string.Empty;
            article.UserId = userId.ToString();
            // Blog fields
            article.IsBlogPost = model.IsBlogPost;
            article.Category = model.Category ?? string.Empty;
            article.Introduction = model.Introduction ?? article.Introduction;
            await DbContext.SaveChangesAsync();
            await SaveTitleChange(article, oldTitle);
            await UpsertCatalogEntry(article);
            var result = new ArticleUpdateResult
            {
                ServerSideSuccess = true,
                Model = model,
                CdnResults = new List<CdnResult>()
            };
            return result;
        }

        /// <summary>
        ///   Makes sure the article catalog isn't missing anything.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task CheckCatalogEntries()
        {
            var articleNumbers = await DbContext.Pages.Select(s => s.ArticleNumber).Distinct().ToListAsync();
            var catalogArticleNumbers = await DbContext.ArticleCatalog.Select(s => s.ArticleNumber).Distinct().ToListAsync();

            var missing = catalogArticleNumbers.Except(catalogArticleNumbers).ToList();

            foreach (var articleNumber in missing)
            {
                var last = await DbContext.Articles.Where(w => w.ArticleNumber == articleNumber && w.Published != null).OrderBy(o => o.VersionNumber).LastOrDefaultAsync();
                await UpsertCatalogEntry(last);
            }
        }

        /// <summary>
        ///     Provides a standard method for turning a title into a URL Encoded path.
        /// </summary>
        /// <param name="title">Title to be converted into a URL.</param>
        /// <remarks>
        ///     <para>This is accomplished using <see cref="HttpUtility.UrlEncode(string)" />.</para>
        ///     <para>Blanks are turned into underscores (i.e. "_").</para>
        ///     <para>All strings are normalized to lower case.</para>
        /// </remarks>
        /// <returns>Article title turned into URL.</returns>
        public string NormailizeArticleUrl(string title)
        {
            return title.Trim().Replace(" ", "_").ToLower();
        }

        /// <summary>
        /// Logic handing logic for publishing articles and saves changes to the database.
        /// </summary>
        /// <param name="articleId">Article Id.</param>
        /// <param name="dateTime">Publishing date and time.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// If article is published, it adds the correct versions to the public pages collection. If not, 
        /// the article is removed from the public pages collection. Also updates the catalog entry.
        /// </remarks>
        public async Task<List<CdnResult>> PublishArticle(Guid articleId, DateTimeOffset? dateTime)
        {
            Article article = DbContext.Articles.FirstOrDefault(f => f.Id == articleId);

            // If the article is already published, then remove the other published versions.
            var others = await DbContext.Articles.Where(
                w => w.ArticleNumber == article.ArticleNumber
                && w.Published != null
                && w.Id != article.Id).ToListAsync();

            var now = DateTimeOffset.Now;

            // If published in the future, then keep the last published article
            if (dateTime.HasValue && dateTime.Value > now)
            {
                // Keep the article pulished just before this one
                var oneTokeep = others.Where(
                    w => w.Published <= now // other published date is before the article
                    && w.VersionNumber < article.VersionNumber).OrderByDescending(o => o.VersionNumber).FirstOrDefault();

                if (oneTokeep != null)
                {
                    others.Remove(oneTokeep);
                }

                // Also keep the other articles that are published between now and before the current article
                var othersToKeep = others.Where(
                    w => w.Published.Value > now // Save items published after now, and...
                    && w.Published.Value < article.Published.Value // published before the current article
                    && w.VersionNumber < article.VersionNumber) // and are a version number before this one.
                    .ToList();

                foreach (var o in othersToKeep)
                {
                    others.Remove(o);
                }
            }

            // Now remove the other ones published
            foreach (var item in others)
            {
                item.Published = null;
            }

            article.Published = dateTime ?? now;

            await DbContext.SaveChangesAsync();

            // Resets the expiration dates, based on the last published article
            await UpdateVersionExpirations(article.ArticleNumber);

            // Make sure the catalog is up to date.
            await CheckCatalogEntries();

            // Update the published pages collection
            return await UpsertPublishedPage(article.Id);
        }

        /// <summary>
        /// Gest the publisher URL depending if this is a multi-tenant editor or not.
        /// </summary>
        /// <returns>Publisher URL.</returns>
        private string GetPublisherUrl()
        {
            return settings.GetEditorConfig().PublisherUrl.TrimEnd('/') + "/";
        }

        /// <summary>
        /// Removes a static webpage from the blob storage if enabled.
        /// </summary>
        /// <param name="filePath">Path to page to remove.</param>
        private void DeleteStaticWebpage(string filePath)
        {
            if (settings.StaticWebPages)
            {
                if (filePath.StartsWith("/pub", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Cannot remove web page from path /pub.");
                }

                filePath = filePath.Equals("root", StringComparison.OrdinalIgnoreCase) ? "/index.html" : filePath;
                storageContext.DeleteFile(filePath);
            }
        }

        private async Task CreateStaticRedirectPage(string fromUrl, string toUrl)
        {
            if (settings.StaticWebPages)
            {
                if (fromUrl.Equals(toUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                fromUrl = fromUrl.Equals("root", StringComparison.OrdinalIgnoreCase) ? "/index.html" : fromUrl;
                toUrl = toUrl.Equals("root", StringComparison.OrdinalIgnoreCase) ? "/index.html" : toUrl;

                var model = new RedirectItemViewModel()
                {
                    FromUrl = fromUrl,
                    ToUrl = toUrl,
                    Id = Guid.NewGuid()
                };

                var html = await viewRenderService.RenderToStringAsync("~/Views/Home/Redirect.cshtml", model);
                using var stream = new MemoryStream(Encoding.UTF8.GetBytes(html));

                await storageContext.AppendBlob(stream, new Cosmos.BlobService.Models.FileUploadMetaData()
                {
                    ChunkIndex = 0,
                    ContentType = "text/html",
                    FileName = Path.GetFileName(fromUrl),
                    ImageHeight = string.Empty,
                    ImageWidth = string.Empty,
                    RelativePath = fromUrl,
                    TotalChunks = 1,
                    TotalFileSize = stream.Length,
                    UploadUid = Guid.NewGuid().ToString(),
                });
            }
        }

        /// <summary>
        ///     Gets a template represented as an <see cref="ArticleViewModel" />.
        /// </summary>
        /// <param name="template">Page template model.</param>
        /// <returns>ArticleViewModel.</returns>
        private ArticleViewModel CreateTemplateViewModel(Template template)
        {
            var articleNumber = DbContext.Articles.Max(m => m.ArticleNumber) + 1;

            return new()
            {
                Id = template.Id,
                ArticleNumber = articleNumber,
                UrlPath = HttpUtility.UrlEncode(template.Title.Trim().Replace(" ", "_")),
                VersionNumber = 1,
                Published = DateTime.Now.ToUniversalTime(),
                Title = template.Title,
                Content = template.Content,
                Updated = DateTime.Now.ToUniversalTime(),
                HeadJavaScript = string.Empty,
                FooterJavaScript = string.Empty,
                ReadWriteMode = true
            };
        }

        /// <summary>
        /// Resets the expiration dates, based on the last published article, saves changes to the database.
        /// </summary>
        /// <param name="articleNumber">Article number.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task UpdateVersionExpirations(int articleNumber)
        {
            var list = await DbContext.Articles.Where(a => a.ArticleNumber == articleNumber).ToListAsync();

            foreach (var item in list)
            {
                if (item.Expires.HasValue)
                {
                    item.Expires = null;
                }
            }

            var published = list.Where(a => a.ArticleNumber == articleNumber && a.Published.HasValue)
                .OrderBy(o => o.VersionNumber).TakeLast(2).ToList();

            if (published.Count == 2)
            {
                published[0].Expires = published[1].Published;
            }

            await DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a published page for a set of article versions, both in the database and in the blob storage (if enabled).
        /// </summary>
        /// <param name="id">ID of article to publish.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<List<CdnResult>> UpsertPublishedPage(Guid id)
        {
            // Clean things up a bit.
            var doomed = await DbContext.Pages.Where(w => w.Content == "" || w.Title == "").ToListAsync();

            if (doomed.Any())
            {
                DbContext.Pages.RemoveRange(doomed);
                await DbContext.SaveChangesAsync();
            }

            // One or more versions of the article are going to be published.
            var newVersion = await DbContext.Articles.FirstOrDefaultAsync(
                w => w.Id == id
                && w.Published != null);

            // These published versions are going to be replaced--except for redirects.
            var articleNumber = newVersion.ArticleNumber;
            var publishedVersions = await DbContext.Pages.Where(
                w => w.ArticleNumber == articleNumber
                && w.StatusCode != (int)StatusCodeEnum.Redirect).ToListAsync();


            if (publishedVersions.Count > 0)
            {
                // Mark these for deletion - do this first to avoid any conflicts
                foreach (var item in publishedVersions)
                {
                    DbContext.Pages.Remove(item);
                    await DbContext.SaveChangesAsync();
                    DeleteStaticWebpage(item.UrlPath);
                }
            }

            // Now refresh the published pages
            var authorInfo = await GetAuthorInfoForUserId(Guid.Parse(newVersion.UserId));

            var newPage = new PublishedPage()
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
                IsBlogPost = newVersion.IsBlogPost,
                Category = newVersion.Category,
                Introduction = newVersion.Introduction
            };

            DbContext.Pages.Add(newPage);
            await DbContext.SaveChangesAsync();

            // This holds the new or updated page URLS.
            var purgePaths = new List<string>();
            if (newVersion.UrlPath.Equals("root", StringComparison.OrdinalIgnoreCase))
            {
                purgePaths.Add("/");
            }
            else
            {
                purgePaths.Add($"{settings.PublisherUrl.TrimEnd('/')}/{newVersion.UrlPath.TrimStart('/')}");
            }

            // Publish the static webpage that are published before now (add 5 min)
            if (newPage.Published.Value <= DateTimeOffset.Now.AddMinutes(5))
            {
                await CreateStaticWebpage(newPage);
            }

            // Make sure the catalog is up to date.
            await UpsertCatalogEntry(newVersion);

            // Update TOC file.
            await CreateStaticTableOfContentsJsonFile("/");

            if (purgePaths.Count > 0)
            {
                var cdnService = CdnService.GetCdnService(DbContext, logger, accessor.HttpContext);
                try
                {
                    return await cdnService.PurgeCdn(purgePaths);
                }
                catch (Exception ex)
                {
                    var d = ex.Message; // debugging purposes
                }
            }

            return null;
        }

        /// <summary>
        /// If the title has changed, handle that here.
        /// </summary>
        /// <param name="article">Article.</param>
        /// <param name="oldTitle">Old title.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// Upon title change:
        /// <list type="bullet">
        /// <item>Updates title for article and it's versions</item>
        /// <item>Updates the article catalog for child entries.</item>
        /// <item>Updates title of all child articles</item>
        /// <item>Creates an automatic redirect</item>
        /// <item>Updates base tags for all articles changed</item>
        /// <item>Saves changes to the database</item>
        /// </list>
        /// </remarks>
        private async Task SaveTitleChange(Article article, string oldTitle)
        {
            if (string.Equals(article.Title, oldTitle, StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(article.UrlPath))
            {
                // Nothing to do
                return;
            }

            // Capture the new title
            var newTitle = article.Title;

            var articleNumbersToUpdate = new List<int>
            {
                article.ArticleNumber
            };

            var oldUrl = NormailizeArticleUrl(oldTitle);
            var newUrl = NormailizeArticleUrl(newTitle);

            // If NOT the root, handle any child page updates and redirects
            // that need to be created.
            if (article.UrlPath != "root")
            {
                // Update sub articles.
                var subArticles = await GetAllSubArticles(oldTitle);

                foreach (var subArticle in subArticles)
                {
                    if (!subArticle.Title.Equals("redirect", StringComparison.CurrentCultureIgnoreCase))
                    {
                        subArticle.Title = UpdatePrefix(oldTitle, newTitle, subArticle.Title);
                    }

                    subArticle.UrlPath = UpdatePrefix(oldUrl, newUrl, subArticle.UrlPath);

                    // Make sure base tag is set properly.
                    UpdateHeadBaseTag(subArticle);
                    await DbContext.SaveChangesAsync();
                    await UpsertCatalogEntry(subArticle);
                    articleNumbersToUpdate.Add(article.ArticleNumber);
                }


                // Remove any conflicting redirects
                var conflictingRedirects = await DbContext.Articles.Where(a => a.Content == newUrl && a.Title.ToLower().Equals("redirect")).ToListAsync();

                if (conflictingRedirects.Any())
                {
                    DbContext.Articles.RemoveRange(conflictingRedirects);
                    articleNumbersToUpdate.AddRange(conflictingRedirects.Select(s => s.ArticleNumber).ToList());
                }

                // Update base href
                UpdateHeadBaseTag(article);
                await DbContext.SaveChangesAsync();

                // Add redirects if published
                if (article.Published.HasValue)
                {
                    // Create a redirect
                    var entity = new PublishedPage
                    {
                        ArticleNumber = 0,
                        StatusCode = (int)StatusCodeEnum.Redirect,
                        UrlPath = oldUrl, // Old URL
                        VersionNumber = 0,
                        Published = DateTime.Now.ToUniversalTime().AddDays(-1), // Make sure this sticks!
                        Title = "Redirect",
                        Content = newUrl, // New URL
                        Updated = DateTime.Now.ToUniversalTime(),
                        HeaderJavaScript = null,
                        FooterJavaScript = null
                    };

                    // Create a static redirect page.
                    await CreateStaticRedirectPage(oldUrl, newUrl);

                    // Add redirect here
                    DbContext.Pages.Add(entity);
                    await DbContext.SaveChangesAsync();
                }
            }

            // We have to change the title and paths for all versions now, since the last publish.
            var lastPublished = await DbContext.Articles.OrderBy(o => o.Published)
                    .LastOrDefaultAsync(a => a.ArticleNumber == article.ArticleNumber);

            var versions = await DbContext.Articles.Where(w => w.ArticleNumber == article.ArticleNumber &&
                                article.Published > lastPublished.Published)
                                    .ToListAsync();

            if (versions.Count == 0)
            {
                // If there are no versions since the last publish, then we need to get all versions.
                versions = await DbContext.Articles.Where(w => w.ArticleNumber == article.ArticleNumber)
                                    .ToListAsync();
            }

            if (string.IsNullOrEmpty(article.UrlPath))
            {
                article.UrlPath = NormailizeArticleUrl(article.Title);
            }

            foreach (var art in versions)
            {
                // Update base href (for Angular apps)
                UpdateHeadBaseTag(article);

                art.Title = newTitle;
                art.Updated = DateTime.Now.ToUniversalTime();
                art.UrlPath = article.UrlPath;
            }

            await DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the sub articles for a page.
        /// </summary>
        /// <param name="urlPrefix">URL Prefix.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task<List<Article>> GetAllSubArticles(string urlPrefix)
        {
            if (string.IsNullOrEmpty(urlPrefix) || string.IsNullOrWhiteSpace(urlPrefix) || urlPrefix.Equals("/"))
            {
                urlPrefix = string.Empty;
            }
            else
            {
                urlPrefix = HttpUtility.UrlDecode(urlPrefix.ToLower().Replace("%20", "_").Replace(" ", "_"));
            }

            var query = DbContext.Articles.Where(a => a.UrlPath.StartsWith(urlPrefix));

            var list = await query.ToListAsync();
            return list;
        }

        /// <summary>
        /// Updates the base tag in the head if Angular is being used.
        /// </summary>
        /// <param name="headerJavaScript">Javascript header.</param>
        /// <param name="urlPath">Url path.</param>
        /// <returns>string.</returns>
        private string UpdateHeadBaseTag(string headerJavaScript, string urlPath)
        {
            if (string.IsNullOrEmpty(headerJavaScript))
            {
                return string.Empty;
            }

            var htmlDoc = new HtmlAgilityPack.HtmlDocument();

            htmlDoc.LoadHtml(headerJavaScript);

            // <meta name="ccms:framework" value="angular">
            var meta = htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='ccms:framework']");

            // This only needs to be run if the framework is "Angular"
            if (meta != null && meta.Attributes["value"].Value.ToLower() != "angular")
            {
                return headerJavaScript;
            }

            var element = htmlDoc.DocumentNode.SelectSingleNode("//base");

            urlPath = $"/{HttpUtility.UrlDecode(urlPath.ToLower().Trim('/'))}/";

            if (element == null)
            {
                var metaTag = htmlDoc.CreateElement("base");
                metaTag.SetAttributeValue("href", urlPath);
                htmlDoc.DocumentNode.AppendChild(metaTag);
            }
            else
            {
                var href = element.Attributes["href"];

                if (href == null)
                {
                    element.Attributes.Add("href", urlPath);
                }
                else
                {
                    href.Value = urlPath;
                }
            }

            headerJavaScript = htmlDoc.DocumentNode.OuterHtml;

            return headerJavaScript;
        }

        /// <summary>
        /// Deletes a catalog entry.
        /// </summary>
        /// <param name="articleNumber">Article number.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task DeleteCatalogEntry(int articleNumber)
        {
            var catalogEntry = await DbContext.ArticleCatalog.FirstOrDefaultAsync(f => f.ArticleNumber == articleNumber);
            if (catalogEntry != null)
            {
                DbContext.ArticleCatalog.Remove(catalogEntry);
                await DbContext.SaveChangesAsync();
            }
        }

        private string UpdatePrefix(string oldprefix, string newPrefix, string targetString)
        {
            var updated = newPrefix + targetString.TrimStart(oldprefix.ToArray());
            return updated;
        }

        /// <summary>
        /// Creates or updates a catalog entry.
        /// </summary>
        /// <param name="article">Article from which to derive the catalog entry.</param>
        private async Task<CatalogEntry> UpsertCatalogEntry(Article article)
        {
            var lastVersion = await DbContext.Articles.Where(a => a.ArticleNumber == article.ArticleNumber).OrderByDescending(o => o.VersionNumber).LastOrDefaultAsync();

            var userId = lastVersion.UserId;
            AuthorInfo authorInfo = await GetAuthorInfoForUserId(Guid.Parse(article.UserId));
            var intro = string.IsNullOrWhiteSpace(article.Introduction) ? string.Empty : article.Introduction;
            if (string.IsNullOrWhiteSpace(intro))
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(article.Content))
                    {
                        var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                        htmlDoc.LoadHtml(lastVersion.Content);
                        var paragraphs = htmlDoc.DocumentNode.SelectNodes("//p");
                        if (paragraphs != null)
                        {
                            var first = paragraphs.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.InnerText));
                            if (first != null)
                            {
                                intro = first.InnerText.Trim();
                                article.Introduction = intro.Length > 512 ? intro.Substring(0, 512) : intro;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error parsing article content during catalog entry.");
                }
            }
            var oldEntry = await DbContext.ArticleCatalog.FirstOrDefaultAsync(f => f.ArticleNumber == article.ArticleNumber);
            if (oldEntry != null)
            {
                DbContext.ArticleCatalog.Remove(oldEntry);
                await DbContext.SaveChangesAsync();
            }
            var entry = new CatalogEntry()
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
            };
            DbContext.ArticleCatalog.Add(entry);
            await DbContext.SaveChangesAsync();
            return entry;
        }
    }
}