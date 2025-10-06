// <copyright file="TitleChangeService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Titles
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Microsoft.EntityFrameworkCore;
    using Sky.Editor.Domain.Events;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.Redirects;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleChangeService"/> class.
        /// </summary>
        /// <param name="db">EF Core context used for querying and persisting title/slug changes.</param>
        /// <param name="slugs">Slug normalization strategy.</param>
        /// <param name="redirects">Redirect management service (for published title changes).</param>
        /// <param name="clock">Clock abstraction for testable timestamps.</param>
        /// <param name="dispatcher">Domain event dispatcher.</param>
        public TitleChangeService(
            ApplicationDbContext db,
            ISlugService slugs,
            IRedirectService redirects,
            IClock clock,
            IDomainEventDispatcher dispatcher)
        {
            this.db = db;
            this.slugs = slugs;
            this.redirects = redirects;
            this.clock = clock;
            this.dispatcher = dispatcher;
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

            // Update child articles & generate redirect if needed (except for root).
            if (!string.Equals(article.UrlPath, "root", StringComparison.OrdinalIgnoreCase))
            {
                var children = await db.Articles
                    .Where(a => a.UrlPath.StartsWith(oldSlug) && a.ArticleNumber != article.ArticleNumber)
                    .ToListAsync();

                foreach (var c in children)
                {
                    // Adjust child titles only if they derive from the old title and are not redirect placeholders.
                    if (!c.Title.Equals("redirect", StringComparison.OrdinalIgnoreCase) &&
                        c.Title.StartsWith(oldTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        c.Title = article.Title + c.Title.Substring(oldTitle.Length);
                    }

                    if (c.UrlPath.StartsWith(oldSlug, StringComparison.OrdinalIgnoreCase))
                    {
                        c.UrlPath = newSlug + c.UrlPath.Substring(oldSlug.Length);
                    }

                    c.Updated = clock.UtcNow;
                }

                // Create redirect only if the current article was published.
                if (article.Published.HasValue)
                {
                    await redirects.CreateOrUpdateRedirectAsync(oldSlug, newSlug, Guid.Parse(article.UserId));
                    await dispatcher.DispatchAsync(new RedirectCreatedEvent(oldSlug, newSlug));
                }
            }

            // Update selected article and synchronize across versions of same logical article.
            article.UrlPath = newSlug;
            article.Updated = clock.UtcNow;

            var versions = await db.Articles
                .Where(a => a.ArticleNumber == article.ArticleNumber)
                .ToListAsync();

            foreach (var v in versions)
            {
                v.Title = article.Title;
                v.UrlPath = article.UrlPath;
                v.Updated = clock.UtcNow;
            }

            await db.SaveChangesAsync();
            await dispatcher.DispatchAsync(new TitleChangedEvent(article.ArticleNumber, oldTitle, article.Title));
        }
    }
}