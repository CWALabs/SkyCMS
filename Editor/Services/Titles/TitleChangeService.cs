// <copyright file="TitleChangeService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Titles
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Microsoft.EntityFrameworkCore;
    using Sky.Editor.Domain.Events;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.Publishing;
    using Sky.Editor.Services.Redirects;
    using Sky.Editor.Services.ReservedPaths;
    using Sky.Editor.Services.Slugs;

    /// <summary>
    /// Coordinates updates required when an article title changes: slug normalization, child URL adjustments,
    /// redirect creation for published articles, version synchronization, and domain event emission.
    /// </summary>
    public sealed class TitleChangeService : ITitleChangeService
    {
        private readonly ApplicationDbContext db;
        private readonly ISlugService slugs;
        private readonly IRedirectService redirects;
        private readonly IClock clock;
        private readonly IDomainEventDispatcher dispatcher;
        private readonly IPublishingService publishingService;
        private readonly IReservedPaths reservedPaths;

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleChangeService"/> class.
        /// </summary>
        /// <param name="db">EF Core context used for querying and persisting title/slug changes.</param>
        /// <param name="slugs">Slug normalization strategy.</param>
        /// <param name="redirects">Redirect management service (for published title changes).</param>
        /// <param name="clock">Clock abstraction for testable timestamps.</param>
        /// <param name="dispatcher">Domain event dispatcher.</param>
        /// <param name="publishingService">Publishing service.</param>
        /// <param name="reservedPaths">Reserved paths service.</param>
        public TitleChangeService(
            ApplicationDbContext db,
            ISlugService slugs,
            IRedirectService redirects,
            IClock clock,
            IDomainEventDispatcher dispatcher,
            IPublishingService publishingService,
            IReservedPaths reservedPaths)
        {
            this.db = db;
            this.slugs = slugs;
            this.redirects = redirects;
            this.clock = clock;
            this.dispatcher = dispatcher;
            this.publishingService = publishingService;
            this.reservedPaths = reservedPaths;
        }

        /// <inheritdoc/>
        public async Task HandleTitleChangeAsync(Article article, string oldTitle)
        {
            // If only case changed and URL already set, skip heavy work.
            if (string.Equals(article.Title, oldTitle, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(article.UrlPath))
            {
                return;
            }

            var oldSlug = slugs.Normalize(oldTitle);
            var newSlug = slugs.Normalize(article.Title);

            // Update selected article and synchronize across versions of same logical article.
            article.UrlPath = newSlug;
            article.Updated = clock.UtcNow;

            var versions = await db.Articles
                .Where(a => a.ArticleNumber == article.ArticleNumber)
                .ToListAsync();

            var counter = 0;
            foreach (var v in versions)
            {
                v.Title = article.Title;
                v.UrlPath = article.UrlPath;
                v.Updated = clock.UtcNow;
                counter++;
                if (counter == 20)
                {
                    await db.SaveChangesAsync();
                    counter = 0;
                }
            }

            await db.SaveChangesAsync();

            // Update the published articles if they exist.
            var publishedArticles = await db.Articles.Where(p => p.ArticleNumber == article.ArticleNumber &&
                  p.Published != null)
                .ToListAsync();

            if (publishedArticles.Any())
            {
                // Unpublish existing published articles.
                foreach (var p in publishedArticles)
                {
                    await publishingService.UnpublishAsync(p);
                }

                // Republish with new URLs.
                foreach (var p in publishedArticles)
                {
                    await publishingService.PublishAsync(p);
                }

                // Create a redirect for any published articles.
                await redirects.CreateOrUpdateRedirectAsync(oldSlug, "/" + newSlug.Trim('/'), new Guid(article.UserId));

            }

            // Update child article URLs.
            await UpdateChildUrlsAsync(article, oldSlug);

            await dispatcher.DispatchAsync(new TitleChangedEvent(article.ArticleNumber, oldTitle, article.Title));
        }

        /// <summary>
        /// Validates whether a proposed title is usable (not reserved and not used by a different article).
        /// </summary>
        /// <param name="title">Proposed title.</param>
        /// <param name="articleNumber">Current article number (null when creating new).</param>
        /// <returns>True if available; false if conflict.</returns>
        public async Task<bool> ValidateTitle(string title, int? articleNumber)
        {
            var paths = (await reservedPaths.GetReservedPaths()).Select(s => s.Path.ToLower()).ToArray();
            foreach (var reservedPath in paths)
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
                ? await db.Articles.FirstOrDefaultAsync(a =>
                    a.ArticleNumber != articleNumber &&
                    a.Title.ToLower() == title.Trim().ToLower() &&
                    a.StatusCode != (int)StatusCodeEnum.Deleted)
                : await db.Articles.FirstOrDefaultAsync(a =>
                    a.Title.ToLower() == title.Trim().ToLower() &&
                    a.StatusCode != (int)StatusCodeEnum.Deleted);

            return article == null;
        }

        /// <summary>
        ///  Updates the URLs of child articles, including all descendants, when a parent article's slug changes.
        /// </summary>
        /// <param name="article">Parent article.</param>
        /// <param name="oldSlug">Old slug.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task UpdateChildUrlsAsync(Article article, string oldSlug)
        {
            // Find all child articles whose URLs start with the old slug.
            // This includes all descendants, not just direct children.
            var childArticles = await db.Articles
                .Where(a => a.UrlPath.StartsWith(oldSlug))
                .ToListAsync();

            var c = 0;
            foreach (var child in childArticles)
            {
                // Recalculate child URL based on new parent slug.
                var oldPath = child.UrlPath;
                var newPath = article.UrlPath.TrimEnd('/') + "/" + child.UrlPath.Substring(oldSlug.Length).TrimStart('/');
                child.UrlPath = newPath;
                child.Updated = clock.UtcNow;
                c++;
                if (c == 20)
                {
                    await db.SaveChangesAsync();
                    c = 0;
                }

                if (child.Published != null)
                {
                    await redirects.CreateOrUpdateRedirectAsync(oldPath, newPath, new Guid(article.UserId));
                    await publishingService.PublishAsync(child);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}