// <copyright file="PublishingService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Publishing
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Microsoft.EntityFrameworkCore;
    using Sky.Editor.Domain.Events;
    using Sky.Editor.Infrastructure.Time;

    /// <summary>
    /// Handles the publishing life‑cycle of articles (marking a single version as published,
    /// clearing prior published versions, and raising publication events).
    /// </summary>
    public sealed class PublishingService : IPublishingService
    {
        private readonly ApplicationDbContext db;
        private readonly IClock clock;
        private readonly IDomainEventDispatcher dispatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishingService"/> class.
        /// </summary>
        /// <param name="db">EF Core database context.</param>
        /// <param name="clock">Clock abstraction for testable UTC times.</param>
        /// <param name="dispatcher">Dispatcher used to raise domain events after publishing.</param>
        public PublishingService(ApplicationDbContext db, IClock clock, IDomainEventDispatcher dispatcher)
        {
            this.db = db;
            this.clock = clock;
            this.dispatcher = dispatcher;
        }

        /// <summary>
        /// Publishes the specified article version and unpublishes any other versions in the same logical article series.
        /// </summary>
        /// <param name="article">The article entity (version) to publish.</param>
        /// <param name="when">Optional publication timestamp; if null the current UTC time is used.</param>
        public async Task PublishAsync(Article article, DateTimeOffset? when)
        {
            var now = clock.UtcNow;
            var publishTime = when ?? now;

            // Unpublish other versions
            var others = await db.Articles.Where(a =>
                a.ArticleNumber == article.ArticleNumber &&
                a.Published != null &&
                a.Id != article.Id).ToListAsync();

            foreach (var o in others)
            {
                o.Published = null;
            }

            article.Published = publishTime;
            article.Updated = now;

            await db.SaveChangesAsync();
            await dispatcher.DispatchAsync(new ArticlePublishedEvent(article.ArticleNumber, article.Id));
        }

        /// <summary>
        /// Unpublishes all versions of a logical article (by article number) and removes generated published page records (non‑redirects).
        /// </summary>
        /// <param name="articleNumber">Logical article number.</param>
        public async Task UnpublishAsync(int articleNumber)
        {
            var versions = await db.Articles.Where(a => a.ArticleNumber == articleNumber).ToListAsync();
            if (!versions.Any()) return;

            foreach (var v in versions)
            {
                v.Published = null;
            }

            var pages = await db.Pages
                .Where(p => p.ArticleNumber == articleNumber && p.StatusCode != (int)StatusCodeEnum.Redirect)
                .ToListAsync();

            if (pages.Any())
            {
                db.Pages.RemoveRange(pages);
            }

            await db.SaveChangesAsync();
        }
    }
}