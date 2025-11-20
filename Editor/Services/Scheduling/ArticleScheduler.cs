// <copyright file="ArticleVersionPublisher.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Scheduling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.BlobService;
    using Cosmos.Cms.Common.Services.Configurations;
    using Cosmos.Common.Data;
    using Cosmos.DynamicConfig;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Sky.Editor.Data.Logic;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.Catalog;
    using Sky.Editor.Services.Html;
    using Sky.Editor.Services.Publishing;
    using Sky.Editor.Services.Redirects;
    using Sky.Editor.Services.Slugs;
    using Sky.Editor.Services.Templates;
    using Sky.Editor.Services.Titles;

    /// <inheritdoc/>
    /// <remarks>
    /// TODO: Enforce that when a content creator uses calendar date/time scheduling,
    /// they can only publish in the future to prevent conflicts with currently published versions.
    /// This validation should be added in the UI/controller layer before saving.
    /// </remarks>
    public class ArticleScheduler : IArticleScheduler
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IOptions<CosmosConfig> config;
        private readonly IMemoryCache memoryCache;
        private readonly StorageContext storageContext;
        private readonly ILogger<ArticleScheduler> logger;
        private readonly IHttpContextAccessor accessor;
        private readonly IEditorSettings settings;
        private readonly IClock clock;
        private readonly ISlugService slugService;
        private readonly IArticleHtmlService htmlService;
        private readonly ICatalogService catalogService;
        private readonly IPublishingService publishingService;
        private readonly ITitleChangeService titleChangeService;
        private readonly IRedirectService redirectService;
        private readonly ITemplateService templateService;
        private readonly IDynamicConfigurationProvider? configurationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleScheduler"/> class.
        /// </summary>
        /// <param name="clock">Clock abstraction for testable time.</param>
        /// <param name="slugService">Slug service.</param>
        /// <param name="htmlService">HTML Service.</param>
        /// <param name="catalogService">Catalog service.</param>
        /// <param name="publishingService">Publishing service.</param>
        /// <param name="titleChangeService">Title change service.</param>
        /// <param name="redirectService">Redirect service.</param>
        /// <param name="templateService">Template service.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="accessor">HTTP context accessor.</param>
        /// <param name="settings">Editor settings.</param>
        /// <param name="dbContext">Single tenant db context.</param>
        /// <param name="config">Cosmos configuration.</param>
        /// <param name="memoryCache">Memory cache.</param>
        /// <param name="storageContext">Storage context.</param>
        /// <param name="configurationProvider">Configuration provider (optional - only for multi-tenant mode).</param>
        public ArticleScheduler(
            ApplicationDbContext dbContext,
            IOptions<CosmosConfig> config,
            IMemoryCache memoryCache,
            StorageContext storageContext,
            ILogger<ArticleScheduler> logger,
            IHttpContextAccessor accessor,
            IEditorSettings settings,
            IClock clock,
            ISlugService slugService,
            IArticleHtmlService htmlService,
            ICatalogService catalogService,
            IPublishingService publishingService,
            ITitleChangeService titleChangeService,
            IRedirectService redirectService,
            ITemplateService templateService,
            IDynamicConfigurationProvider? configurationProvider = null)
        {
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.storageContext = storageContext ?? throw new ArgumentNullException(nameof(storageContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.slugService = slugService ?? throw new ArgumentNullException(nameof(slugService));
            this.htmlService = htmlService ?? throw new ArgumentNullException(nameof(htmlService));
            this.catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
            this.publishingService = publishingService ?? throw new ArgumentNullException(nameof(publishingService));
            this.titleChangeService = titleChangeService ?? throw new ArgumentNullException(nameof(titleChangeService));
            this.redirectService = redirectService ?? throw new ArgumentNullException(nameof(redirectService));
            this.templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            this.configurationProvider = configurationProvider;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// TODO: Implement notification system to alert content creators when their scheduled
        /// publications go live. Consider email notifications, in-app notifications, or webhook integrations.
        /// </remarks>
        public async Task ExecuteAsync()
        {
            var now = clock.UtcNow;
            logger.LogInformation("ArticleVersionPublisher: Starting scheduled execution at {ExecutionTime}", now);

            if (settings.IsMultiTenantEditor && configurationProvider?.IsMultiTenantConfigured == true)
            {
                var domainNames = await configurationProvider.GetAllDomainNamesAsync();
                foreach (var domainName in domainNames)
                {
                    var connectionString = await configurationProvider.GetDatabaseConnectionStringAsync(domainName);
                    using (var dbContext = new ApplicationDbContext(connectionString))
                    {
                        await Run(dbContext, domainName);
                    }
                }
            }
            else
            {
                await Run(_dbContext, "local-host");
            }
        }

        private async Task Run(ApplicationDbContext dbContext, string domainName = "")
        {
            try
            {
                var now = clock.UtcNow;

                // Note: EF Core for Cosmos DB does not support grouping and counting directly in the database for
                // this scenario, so we retrieve the article numbers first and then filter in-memory.
                // Find all article numbers that have 2+ versions with non-null Published dates
                var articleNumbers = await dbContext.Articles
                    .Where(a => a.Published != null
                                && a.Published <= now
                                && a.StatusCode != (int)Cosmos.Common.Data.Logic.StatusCodeEnum.Deleted)
                    .Select(a => a.ArticleNumber)
                    .ToListAsync();

                // Step 2: Filter in-memory for multiples
                var articlesWithMultiplePublishedVersions = articleNumbers
                    .GroupBy(n => n)
                    .Where(g => g.Count() >= 2)
                    .Select(g => new { ArticleNumber = g.Key, Count = g.Count() })
                    .ToList();

                foreach (var art in articlesWithMultiplePublishedVersions)
                {
                    await ProcessArticleVersions(now, dbContext, art.ArticleNumber);
                }

                logger.LogInformation("ArticleVersionPublisher: Completed execution for domain {Domain} at {CompletionTime}", domainName, clock.UtcNow);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ArticleVersionPublisher: Error during scheduled execution for domain {Domain}", domainName);
                throw;
            }
        }

        /// <summary>
        /// Processes all versions of a specific article, activating the most recent non-future version.
        /// </summary>
        /// <param name="now">Current UTC timestamp.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="articleNumber">Article number</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ProcessArticleVersions(DateTimeOffset now, ApplicationDbContext dbContext, int articleNumber)
        {
            try
            {
                // Get all versions with published dates for this article
                var versions = await dbContext.Articles
                    .Where(a => a.ArticleNumber == articleNumber
                    && a.Published != null && a.Published <= now
                    && a.StatusCode != (int)Cosmos.Common.Data.Logic.StatusCodeEnum.Deleted)
                    .OrderByDescending(a => a.Published)
                    .ToListAsync();

                if (versions.Count < 2)
                {
                    // No longer has multiple versions (race condition or concurrent delete)
                    // TODO: Investigate potential race conditions when users manually publish/unpublish
                    // while the scheduler is running. Consider implementing optimistic concurrency control
                    // or row-level locking to prevent conflicts.
                    return;
                }

                // Find the most recent version that is not in the future
                var activeVersion = versions.FirstOrDefault();

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
                    oldVersion.Published = null;
                }

                // Save unpublished changes
                if (oldVersions.Any())
                {
                    await dbContext.SaveChangesAsync();
                }

                // Publish the active version (this will unpublish any other versions and update the published page)
                var articleEditLogger = logger is ILogger<ArticleEditLogic> editLogger ? editLogger : LoggerFactory.Create(builder => { }).CreateLogger<ArticleEditLogic>();

                // Create a new ArticleEditLogic instance for this operation
                var articleLogic = new ArticleEditLogic(dbContext, config, memoryCache, storageContext, articleEditLogger, accessor, settings, clock, slugService, htmlService, catalogService, publishingService, titleChangeService, redirectService, templateService);

                // Publish the active version
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
                // TODO: Implement retry mechanism for failed publications and/or
                // notification system to alert administrators about publication failures.
                // Consider using Hangfire's automatic retry features or a separate notification service.
            }
        }
    }
}