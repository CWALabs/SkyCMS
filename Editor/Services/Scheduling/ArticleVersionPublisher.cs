// <copyright file="ArticleVersionPublisher.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Scheduling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Sky.Editor.Data.Logic;
    using Sky.Editor.Infrastructure.Time;

    /// <summary>
    /// Scheduled service that activates article versions with multiple published dates,
    /// ensuring only the most recent non-future version is actively published.
    /// </summary>
    public class ArticleVersionPublisher
    {
        private readonly ApplicationDbContext dbContext;
        private readonly ArticleEditLogic articleLogic;
        private readonly IClock clock;
        private readonly ILogger<ArticleVersionPublisher> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleVersionPublisher"/> class.
        /// </summary>
        /// <param name="dbContext">Database context.</param>
        /// <param name="articleLogic">Article editing logic service.</param>
        /// <param name="clock">Clock abstraction for testable time.</param>
        /// <param name="logger">Logger instance.</param>
        public ArticleVersionPublisher(
            ApplicationDbContext dbContext,
            ArticleEditLogic articleLogic,
            IClock clock,
            ILogger<ArticleVersionPublisher> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.articleLogic = articleLogic ?? throw new ArgumentNullException(nameof(articleLogic));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes the scheduled job to process article versions with multiple published dates.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task ExecuteAsync()
        {
            logger.LogInformation("ArticleVersionPublisher: Starting scheduled execution at {ExecutionTime}", clock.UtcNow);

            try
            {
                var now = clock.UtcNow;

                // Find all article numbers that have 2+ versions with non-null Published dates
                var articlesWithMultiplePublishedVersions = await dbContext.Articles
                    .Where(a => a.Published != null && a.StatusCode != (int)Cosmos.Common.Data.Logic.StatusCodeEnum.Deleted)
                    .GroupBy(a => a.ArticleNumber)
                    .Select(g => new { ArticleNumber = g.Key, C = g.Count() })
                    .ToListAsync();

                logger.LogInformation("Found {Count} articles with multiple published versions", articlesWithMultiplePublishedVersions.Count);

                foreach (var articleNumber in articlesWithMultiplePublishedVersions)
                {
                    await ProcessArticleVersions(articleNumber.ArticleNumber, now);
                }

                logger.LogInformation("ArticleVersionPublisher: Completed execution at {CompletionTime}", clock.UtcNow);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ArticleVersionPublisher: Error during scheduled execution");
                throw;
            }
        }

        /// <summary>
        /// Processes all versions of a specific article, activating the most recent non-future version.
        /// </summary>
        /// <param name="articleNumber">The logical article number.</param>
        /// <param name="now">Current UTC timestamp.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ProcessArticleVersions(int articleNumber, DateTimeOffset now)
        {
            try
            {
                // Get all versions with published dates for this article
                var versions = await dbContext.Articles
                    .Where(a => a.ArticleNumber == articleNumber &&
                                a.Published != null &&
                                a.StatusCode != (int)Cosmos.Common.Data.Logic.StatusCodeEnum.Deleted)
                    .OrderByDescending(a => a.Published)
                    .ToListAsync();

                if (versions.Count < 2)
                {
                    // No longer has multiple versions (race condition or concurrent delete)
                    return;
                }

                // Find the most recent version that is not in the future
                var activeVersion = versions.FirstOrDefault(v => v.Published <= now);

                if (activeVersion == null)
                {
                    // All versions are scheduled for the future
                    logger.LogDebug("Article {ArticleNumber}: All versions are scheduled for future publication", articleNumber);
                    return;
                }

                logger.LogInformation(
                    "Article {ArticleNumber}: Activating version {VersionNumber} (Published: {PublishedDate})",
                    articleNumber,
                    activeVersion.VersionNumber,
                    activeVersion.Published);

                // Unpublish all older versions (those published before the active version)
                var oldVersions = versions.Where(v =>
                    v.Published < activeVersion.Published &&
                    v.Id != activeVersion.Id).ToList();

                foreach (var oldVersion in oldVersions)
                {
                    logger.LogInformation(
                        "Article {ArticleNumber}: Unpublishing old version {VersionNumber} (was published: {OldPublishedDate})",
                        articleNumber,
                        oldVersion.VersionNumber,
                        oldVersion.Published);

                    oldVersion.Published = null;
                }

                // Save unpublished changes
                if (oldVersions.Any())
                {
                    await dbContext.SaveChangesAsync();
                }

                // Publish the active version (this will unpublish any other versions and update the published page)
                await articleLogic.PublishArticle(activeVersion.Id, activeVersion.Published);

                logger.LogInformation(
                    "Article {ArticleNumber}: Successfully activated version {VersionNumber}",
                    articleNumber,
                    activeVersion.VersionNumber);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing article versions for ArticleNumber {ArticleNumber}",
                    articleNumber);
            }
        }
    }
}