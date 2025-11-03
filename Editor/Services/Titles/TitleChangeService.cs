// <copyright file="TitleChangeService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Titles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Sky.Editor.Domain.Events;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.BlogPublishing;
    using Sky.Editor.Services.Publishing;
    using Sky.Editor.Services.Redirects;
    using Sky.Editor.Services.ReservedPaths;
    using Sky.Editor.Services.Slugs;

    /// <summary>
    /// Coordinates updates required when an article title changes: slug normalization, child URL adjustments,
    /// redirect creation for published articles, version synchronization, and domain event emission.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This service orchestrates the complex side effects of changing an article's title, ensuring
    /// consistency across URLs, slugs, redirects, and related content (especially blog streams and posts).
    /// </para>
    /// <para>
    /// When a title changes, the service:
    /// </para>
    /// <list type="number">
    ///   <item><description>Normalizes the new title into a URL-safe slug</description></item>
    ///   <item><description>Updates the article's <see cref="Article.UrlPath"/> and <see cref="Article.BlogKey"/></description></item>
    ///   <item><description>For blog streams, cascades the change to all associated blog posts</description></item>
    ///   <item><description>Synchronizes all article versions to use the new slug</description></item>
    ///   <item><description>Creates redirects from old URLs to new URLs for published content</description></item>
    ///   <item><description>Republishes affected articles if they were previously published</description></item>
    ///   <item><description>Dispatches domain events to notify subscribers of the change</description></item>
    /// </list>
    /// <para>
    /// The service maintains referential integrity by ensuring that blog posts always reference
    /// their parent blog stream's current slug as their blog key.
    /// </para>
    /// </remarks>
    public sealed class TitleChangeService : ITitleChangeService
    {
        private readonly ApplicationDbContext db;
        private readonly ISlugService slugs;
        private readonly IRedirectService redirects;
        private readonly IClock clock;
        private readonly IDomainEventDispatcher dispatcher;
        private readonly IPublishingService publishingService;
        private readonly IReservedPaths reservedPaths;
        private readonly IBlogRenderingService blogRenderingService;
        private readonly ILogger<TitleChangeService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleChangeService"/> class.
        /// </summary>
        /// <param name="db">EF Core database context used for querying and persisting title/slug changes.</param>
        /// <param name="slugs">Slug normalization service for converting titles to URL-safe segments.</param>
        /// <param name="redirects">Redirect management service for creating permanent redirects from old to new URLs.</param>
        /// <param name="clock">Clock abstraction for obtaining testable, deterministic timestamps.</param>
        /// <param name="dispatcher">Domain event dispatcher for publishing title change events to subscribers.</param>
        /// <param name="publishingService">Publishing service for regenerating static content after title changes.</param>
        /// <param name="reservedPaths">Reserved paths service for validating that new titles don't conflict with system routes.</param>
        /// <param name="blogRenderingService">Blog rendering service for regenerating blog stream HTML content.</param>
        /// <param name="logger">Logger for diagnostic and error events.</param>
        public TitleChangeService(
            ApplicationDbContext db,
            ISlugService slugs,
            IRedirectService redirects,
            IClock clock,
            IDomainEventDispatcher dispatcher,
            IPublishingService publishingService,
            IReservedPaths reservedPaths,
            IBlogRenderingService blogRenderingService,
            ILogger<TitleChangeService> logger)
        {
            this.db = db;
            this.slugs = slugs;
            this.redirects = redirects;
            this.clock = clock;
            this.dispatcher = dispatcher;
            this.publishingService = publishingService;
            this.reservedPaths = reservedPaths;
            this.blogRenderingService = blogRenderingService;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string BuildArticleUrl(Article article)
        {
            if (article.ArticleType == (int)ArticleType.BlogPost)
            {
                return slugs.Normalize(article.Title, article.BlogKey);
            }

            return slugs.Normalize(article.Title);
        }

        /// <inheritdoc/>
        public async Task HandleTitleChangeAsync(Article article, string oldTitle)
        {
            // Use a local dictionary to track URL changes for this operation
            var changedUrls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var oldSlug = slugs.Normalize(oldTitle);
            var newSlug = BuildArticleUrl(article);

            // Only proceed if the slug actually changed
            if (oldSlug.Equals(newSlug, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Validate that the new slug doesn't conflict with existing articles
            var slugConflict = await db.Articles
                .AnyAsync(a =>
                    a.ArticleNumber != article.ArticleNumber &&
                    a.UrlPath == newSlug &&
                    a.StatusCode != (int)StatusCodeEnum.Deleted);

            if (slugConflict)
            {
                logger.LogWarning(
                    "Title change for article {ArticleNumber} would create slug conflict with existing article. Old: {OldSlug}, New: {NewSlug}",
                    article.ArticleNumber,
                    oldSlug,
                    newSlug);
                throw new InvalidOperationException($"The slug '{newSlug}' is already in use by another article.");
            }

            // Track the URL change for redirect creation
            changedUrls.TryAdd(oldSlug, newSlug);

            // Update the article's URL path and blog key (for blog posts and blog streams)
            article.UrlPath = newSlug;

            // If this is a blog stream, the blog key must match the UrlPath (new slug).
            if (article.ArticleType == (int)ArticleType.BlogStream)
            {
                article.BlogKey = newSlug;
            }

            await db.SaveChangesAsync();

            // If this is a blog stream, cascade changes to all associated blog posts
            if (article.ArticleType == (int)ArticleType.BlogStream)
            {
                await HandleBlogStreamEntriesAsync(article, oldSlug, newSlug, changedUrls);
            }
            else if (article.ArticleType == (int)ArticleType.General)
            {
                await HandleTitleChangesForChildren(article, oldSlug, newSlug, changedUrls);
            }

            // Synchronize all versions of this article
            await UpdateVersionsAsync(article, newSlug, oldSlug);

            // Republish if the article is currently published
            if (article.Published != null && article.Published <= clock.UtcNow)
            {
                await publishingService.PublishAsync(article);
            }

            // Create redirects for all changed URLs
            if (changedUrls.Any())
            {
                await CreateRedirectsAsync(changedUrls, article.UserId);
            }

            // Notify subscribers of the title change
            await dispatcher.DispatchAsync(new TitleChangedEvent(article.ArticleNumber, oldTitle, article.Title));
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateTitle(string title, int? articleNumber)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return false;
            }

            var normalizedTitle = title.Trim();

            // Check against reserved paths (system routes that cannot be used for articles)
            var paths = (await reservedPaths.GetReservedPaths()).Select(s => s.Path.ToLower()).ToArray();
            foreach (var reservedPath in paths)
            {
                if (reservedPath.EndsWith('*'))
                {
                    // Wildcard reserved path - check if title starts with the prefix
                    var value = reservedPath.TrimEnd('*').TrimEnd('/');
                    if (normalizedTitle.ToLower().StartsWith(value))
                    {
                        return false;
                    }
                }
                else if (normalizedTitle.Equals(reservedPath, StringComparison.OrdinalIgnoreCase))
                {
                    // Exact match reserved path
                    return false;
                }
            }

            // Check for title conflicts with other existing articles
            Article existingArticle = articleNumber.HasValue
                ? await db.Articles.FirstOrDefaultAsync(a =>
                    a.ArticleNumber != articleNumber &&
                    a.Title.ToLower() == normalizedTitle.ToLower() &&
                    a.StatusCode != (int)StatusCodeEnum.Deleted)
                : await db.Articles.FirstOrDefaultAsync(a =>
                    a.Title.ToLower() == normalizedTitle.ToLower() &&
                    a.StatusCode != (int)StatusCodeEnum.Deleted);

            return existingArticle == null;
        }

        /// <summary>
        /// Handles title changes for blog stream articles by updating the blog key, regenerating blog entry URLs,
        /// creating redirects for published entries, and re-rendering the blog stream content.
        /// </summary>
        /// <param name="blogStreamArticle">The blog stream article whose title has changed. Must be of type <see cref="ArticleType.BlogStream"/>.</param>
        /// <param name="oldBlogKey">The previous normalized slug of the blog stream, derived from the old title.</param>
        /// <param name="newBlogKey">The new normalized slug of the blog stream, derived from the new title.</param>
        /// <param name="changedUrls">The dictionary tracking URL changes for redirect creation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// When a blog stream's title changes, its slug (used as the <see cref="Article.BlogKey"/>) also changes.
        /// This affects all blog posts associated with that stream, as they reference the stream's slug in their URLs.
        /// </para>
        /// <para>
        /// This method performs the following operations:
        /// </para>
        /// <list type="number">
        ///   <item><description>Queries all blog posts (<see cref="ArticleType.BlogPost"/>) associated with the old blog key</description></item>
        ///   <item><description>For each blog post:
        ///     <list type="bullet">
        ///       <item><description>Updates the <see cref="Article.BlogKey"/> to reference the new blog stream slug</description></item>
        ///       <item><description>Recalculates the <see cref="Article.UrlPath"/> using the new blog key</description></item>
        ///       <item><description>Synchronizes all versions of the blog post</description></item>
        ///       <item><description>If published, tracks the URL change for redirect creation</description></item>
        ///       <item><description>If published, republishes the post at its new URL</description></item>
        ///     </list>
        ///   </description></item>
        ///   <item><description>Regenerates the blog stream's HTML content to reflect the new structure</description></item>
        ///   <item><description>Persists all changes to the database</description></item>
        ///   <item><description>Dispatches a <see cref="TitleChangedEvent"/> for the blog stream</description></item>
        /// </list>
        /// <para>
        /// This ensures that the entire blog hierarchy remains consistent and accessible at the correct URLs
        /// after a blog stream title change.
        /// </para>
        /// </remarks>
        private async Task HandleBlogStreamEntriesAsync(
            Article blogStreamArticle,
            string oldBlogKey,
            string newBlogKey,
            Dictionary<string, string> changedUrls)
        {
            // Find all blog posts associated with the old blog key
            var blogEntries = await db.Articles
                .Where(a => a.BlogKey == oldBlogKey && a.ArticleType == (int)ArticleType.BlogPost)
                .ToListAsync();

            // Update each blog entry to use the new blog key and recalculated URL
            foreach (var entry in blogEntries)
            {
                var oldPath = entry.UrlPath;
                var newPath = slugs.Normalize(entry.Title, newBlogKey);

                entry.BlogKey = newBlogKey;
                entry.UrlPath = newPath;

                // Track URL change if paths differ
                if (!oldPath.Equals(newPath, StringComparison.OrdinalIgnoreCase))
                {
                    changedUrls.TryAdd(oldPath, newPath);
                }

                // Synchronize all versions of this blog post
                await UpdateVersionsAsync(entry, newBlogKey, oldPath);

                // If published, republish at new URL
                if (entry.Published != null && entry.Published <= clock.UtcNow)
                {
                    await publishingService.PublishAsync(entry);
                }
            }

            // Save all blog entry changes in a single transaction
            await db.SaveChangesAsync();

            // Regenerate the blog stream's HTML content with updated links
            var generatedHtml = await blogRenderingService.GenerateBlogStreamHtml(blogStreamArticle);

            if (string.IsNullOrEmpty(generatedHtml))
            {
                logger.LogWarning(
                    "Blog rendering service returned null or empty HTML for blog stream article {ArticleNumber}",
                    blogStreamArticle.ArticleNumber);
            }
            else
            {
                blogStreamArticle.Content = generatedHtml;
            }

            await db.SaveChangesAsync();

            // Notify subscribers of the blog stream title change
            await dispatcher.DispatchAsync(new TitleChangedEvent(
                blogStreamArticle.ArticleNumber,
                oldBlogKey,
                blogStreamArticle.Title));
        }

        /// <summary>
        /// Updates all versions of an article to reflect a new slug and URL path.
        /// </summary>
        /// <param name="article">The article whose versions are to be updated. Versions are identified by matching <see cref="Article.ArticleNumber"/>.</param>
        /// <param name="newSlug">The new slug to assign to all versions of the article.</param>
        /// <param name="oldSlug">The old slug for logging purposes.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// Articles can have multiple versions (drafts, historical versions) identified by the same <see cref="Article.ArticleNumber"/>.
        /// When a title (and thus slug) changes, all versions must be synchronized to maintain consistency.
        /// </para>
        /// <para>
        /// This method:
        /// </para>
        /// <list type="number">
        ///   <item><description>Queries all versions with the same <see cref="Article.ArticleNumber"/> (excluding the current article instance)</description></item>
        ///   <item><description>Updates each version's <see cref="Article.BlogKey"/> (for blog posts only) and <see cref="Article.UrlPath"/></description></item>
        ///   <item><description>Republishes any versions that are currently published (published timestamp is in the past)</description></item>
        ///   <item><description>Persists changes in batches of 20 for performance optimization</description></item>
        /// </list>
        /// <para>
        /// Batching ensures that large numbers of versions don't cause transaction timeouts or memory issues.
        /// </para>
        /// </remarks>
        private async Task UpdateVersionsAsync(Article article, string newSlug, string oldSlug)
        {
            var articleNumber = article.ArticleNumber;
            var id = article.Id;

            // Find all other versions of this article
            var versions = await db.Articles
                .Where(av => av.ArticleNumber == articleNumber && av.Id != id)
                .ToListAsync();

            if (!versions.Any())
            {
                return;
            }

            logger.LogInformation(
                "Updating {Count} versions for article {ArticleNumber} from slug '{OldSlug}' to '{NewSlug}'",
                versions.Count,
                articleNumber,
                oldSlug,
                newSlug);

            var count = 0;
            foreach (var version in versions)
            {
                // Only update BlogKey for BlogPost and BlogStream articles
                if (version.ArticleType == (int)ArticleType.BlogPost || version.ArticleType == (int)ArticleType.BlogStream)
                {
                    version.BlogKey = newSlug;
                }

                // Always update the URL path using the BuildArticleUrl logic
                version.UrlPath = version.ArticleType == (int)ArticleType.BlogPost
                    ? slugs.Normalize(version.Title, newSlug)
                    : slugs.Normalize(version.Title);

                // Republish if this version is currently published
                if (version.Published.HasValue && version.Published <= clock.UtcNow)
                {
                    await db.SaveChangesAsync();
                    count = 0;
                    await publishingService.PublishAsync(version);
                }

                // Batch save every 20 records to optimize performance
                if (++count >= 20)
                {
                    await db.SaveChangesAsync();
                    count = 0;
                }
            }

            // Save any remaining changes
            if (count > 0)
            {
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Creates redirect articles for all URL changes accumulated during a title change operation.
        /// </summary>
        /// <param name="changedUrls">Dictionary mapping old URLs (keys) to their new destinations (values).</param>
        /// <param name="userId">The string identifier of the user initiating the title change, used for audit tracking in redirect records.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="userId"/> is not a valid GUID format.</exception>
        /// <remarks>
        /// <para>
        /// This method iterates through all URL changes collected during a title change operation
        /// and creates or updates redirect articles for each mapping. This ensures that visitors
        /// following old links (from bookmarks, search engines, etc.) are automatically redirected
        /// to the new URL.
        /// </para>
        /// <para>
        /// Redirects are permanent (301) and are implemented via the <see cref="IRedirectService"/>,
        /// which creates special redirect-type articles in the database.
        /// </para>
        /// <para>
        /// The <paramref name="userId"/> is validated and converted to a <see cref="Guid"/> before
        /// being passed to the redirect service. If the conversion fails, an <see cref="ArgumentException"/> is thrown.
        /// </para>
        /// </remarks>
        private async Task CreateRedirectsAsync(Dictionary<string, string> changedUrls, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty when creating redirects.", nameof(userId));
            }

            // Validate and parse the user ID to a GUID
            if (!Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException($"User ID '{userId}' is not a valid GUID format.", nameof(userId));
            }

            var distinctUrls = changedUrls.ToList().Distinct();

            foreach (var kvp in distinctUrls)
            {
                try
                {
                    await redirects.CreateOrUpdateRedirectAsync(kvp.Key, kvp.Value, userGuid);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Failed to create redirect from '{OldUrl}' to '{NewUrl}' for user {UserId}",
                        kvp.Key,
                        kvp.Value,
                        userGuid);
                }
            }
        }

        /// <summary>
        /// Handles title changes for general articles by recursively updating all descendant articles
        /// whose URL paths are prefixed by the old slug, adjusting their paths to use the new slug.
        /// </summary>
        /// <param name="article">The general article whose title has changed. Must be of type <see cref="ArticleType.General"/>.</param>
        /// <param name="oldSlug">The previous normalized slug of the article, derived from the old title.</param>
        /// <param name="newSlug">The new normalized slug of the article, derived from the new title.</param>
        /// <param name="changedUrls">The dictionary tracking URL changes for redirect creation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// When a general article's title (and thus slug) changes, any child articles whose URL paths
        /// start with the old slug must have their paths updated to reflect the new parent slug.
        /// This ensures hierarchical URL consistency across the content tree.
        /// </para>
        /// <para>
        /// This method performs the following operations:
        /// </para>
        /// <list type="number">
        ///   <item><description>Queries all articles whose <see cref="Article.UrlPath"/> starts with the old slug followed by "/"</description></item>
        ///   <item><description>For each descendant article:
        ///     <list type="bullet">
        ///       <item><description>Replaces the old slug prefix with the new slug in the <see cref="Article.UrlPath"/></description></item>
        ///       <item><description>Synchronizes all versions of the descendant article</description></item>
        ///       <item><description>If published, tracks the URL change for redirect creation</description></item>
        ///       <item><description>If published, republishes the article at its new URL</description></item>
        ///     </list>
        ///   </description></item>
        ///   <item><description>Persists all changes to the database in batches of 20 for performance optimization</description></item>
        /// </list>
        /// <para>
        /// This cascading update maintains referential integrity throughout the content hierarchy,
        /// ensuring that all child pages remain accessible at their correct relative URLs.
        /// </para>
        /// </remarks>
        private async Task HandleTitleChangesForChildren(Article article, string oldSlug, string newSlug, Dictionary<string, string> changedUrls)
        {
            // Find all articles that have the old slug as a parent (URL path starts with old slug)
            var childArticles = await db.Articles
                .Where(a => a.UrlPath.StartsWith(oldSlug + "/") && a.StatusCode != (int)StatusCodeEnum.Deleted)
                .ToListAsync();

            if (!childArticles.Any())
            {
                return;
            }

            logger.LogInformation(
                "Updating {Count} child articles for parent article {ArticleNumber} from slug '{OldSlug}' to '{NewSlug}'",
                childArticles.Count,
                article.ArticleNumber,
                oldSlug,
                newSlug);

            var count = 0;
            foreach (var child in childArticles)
            {
                var oldChildPath = child.UrlPath;

                // Replace the old slug prefix with the new slug in the URL path
                var newChildPath = newSlug + oldChildPath.Substring(oldSlug.Length);

                child.UrlPath = newChildPath;

                // Track URL change if paths differ and article is published
                if (!oldChildPath.Equals(newChildPath, StringComparison.OrdinalIgnoreCase))
                {
                    changedUrls.TryAdd(oldChildPath, newChildPath);
                }

                // Synchronize all versions of this child article
                await UpdateVersionsAsync(child, newChildPath, oldChildPath);

                // Republish if the child article is currently published
                if (child.Published != null && child.Published <= clock.UtcNow)
                {
                    await db.SaveChangesAsync();
                    count = 0;
                    await publishingService.PublishAsync(child);
                }

                // Batch save every 20 records to optimize performance
                if (++count >= 20)
                {
                    await db.SaveChangesAsync();
                    count = 0;
                }
            }

            // Save any remaining changes
            if (count > 0)
            {
                await db.SaveChangesAsync();
            }
        }
    }
}