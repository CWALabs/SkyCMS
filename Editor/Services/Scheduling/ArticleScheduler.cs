// <copyright file="ArticleScheduler.cs" company="Moonrise Software, LLC">
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
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.EditorSettings;

    /// <inheritdoc/>
    /// <remarks>
    /// TODO: Enforce that when a content creator uses calendar date/time scheduling,
    /// they can only publish in the future to prevent conflicts with currently published versions.
    /// This validation should be added in the UI/controller layer before saving.
    /// </remarks>
    public class ArticleScheduler : IArticleScheduler
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IOptions<CosmosConfig> config;
        private readonly ILogger<ArticleScheduler> logger;
        private readonly IEditorSettings settings;
        private readonly IClock clock;
        private readonly IDynamicConfigurationProvider configurationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArticleScheduler"/> class.
        /// </summary>
        /// <param name="clock">Clock abstraction for testable time.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="settings">Editor settings.</param>
        /// <param name="config">Cosmos configuration.</param>
        /// <param name="serviceProvider">Service provider for creating scoped dependencies.</param>
        public ArticleScheduler(
            IOptions<CosmosConfig> config,
            ILogger<ArticleScheduler> logger,
            IEditorSettings settings,
            IClock clock,
            IServiceProvider serviceProvider)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            if (settings.IsMultiTenantEditor)
            {
                configurationProvider = serviceProvider.GetRequiredService<IDynamicConfigurationProvider>();
            }
            else
            {
                configurationProvider = null;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// TODO: Implement notification system to alert content creators when their scheduled
        /// publications go live. Consider email notifications, in-app notifications, or webhook integrations.
        /// </remarks>
        public async Task ExecuteAsync()
        {
            var now = clock.UtcNow;
            logger.LogInformation("ArticleScheduler: Starting scheduled execution at {ExecutionTime}", now);

            if (!settings.IsMultiTenantEditor)
            {
                try
                {
                    await RunForTenant(string.Empty);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ArticleScheduler: Error processing schduler.");
                }

                return;
            }

            var domainNames = await configurationProvider.GetAllDomainNamesAsync();
            foreach (var domainName in domainNames)
            {
                try
                {
                    await RunForTenant(domainName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ArticleScheduler: Error processing tenant {Domain}", domainName);
                }
            }
        }

        private async Task RunForTenant(string domainName)
        {
            // Create a new scope for each tenant to ensure proper dependency isolation
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var memoryCache = scopedServices.GetRequiredService<IMemoryCache>();

            try
            {
                // These service need to be scoped to a particular domainName.
                ApplicationDbContext dbContext;
                StorageContext storageContext;

                if (settings.IsMultiTenantEditor)
                {
                    // All services must be scoped to the tenant's connection
                    var connection = await configurationProvider.GetTenantConnectionAsync(domainName);
                    
                    // TODO: Create tenant-specific ApplicationDbContext using connection.PrimaryCloud or appropriate connection string
                    dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();
                    storageContext = new StorageContext(connection.StorageConn, memoryCache);
                }
                else
                {
                    // Use the scoped services from DI
                    dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();
                    storageContext = scopedServices.GetRequiredService<StorageContext>();
                }

                await Run(dbContext, storageContext, domainName, scopedServices);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ArticleScheduler: Error during scheduled execution for domain {Domain}", domainName);
                throw;
            }
        }

        private async Task Run(ApplicationDbContext dbContext, StorageContext storageContext, string domainName, IServiceProvider scopedServices)
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
                await ProcessArticleVersions(now, dbContext, storageContext, art.ArticleNumber, domainName, scopedServices);
            }

            logger.LogInformation("ArticleScheduler: Completed execution for domain {Domain} at {CompletionTime}", domainName, clock.UtcNow);
        }

        /// <summary>
        /// Processes all versions of a specific article, activating the most recent non-future version.
        /// </summary>
        /// <param name="now">Current UTC timestamp.</param>
        /// <param name="dbContext">The database context.</param>
        /// <param name="storageContext">The storage context.</param>
        /// <param name="articleNumber">Article number.</param>
        /// <param name="domainName">Domain name for logging.</param>
        /// <param name="scopedServices">Scoped service provider.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ProcessArticleVersions(
            DateTimeOffset now,
            ApplicationDbContext dbContext,
            StorageContext storageContext,
            int articleNumber,
            string domainName,
            IServiceProvider scopedServices)
        {
            try
            {
                var versions = await dbContext.Articles
                    .Where(a => a.ArticleNumber == articleNumber
                    && a.Published != null && a.Published <= now
                    && a.StatusCode != (int)Cosmos.Common.Data.Logic.StatusCodeEnum.Deleted)
                    .OrderByDescending(a => a.Published)
                    .ToListAsync();

                if (versions.Count < 2)
                {
                    return;
                }

                var activeVersion = versions.FirstOrDefault();
                if (activeVersion == null)
                {
                    logger.LogDebug("Article {ArticleNumber}: All versions are scheduled for future publication", articleNumber);
                    return;
                }

                logger.LogInformation(
                    "Article {ArticleNumber} (Domain: {Domain}): Activating version {VersionNumber} (Published: {PublishedDate})",
                    articleNumber,
                    domainName,
                    activeVersion.VersionNumber,
                    activeVersion.Published);

                var oldVersions = versions.Where(v =>
                    v.Published < activeVersion.Published &&
                    v.Id != activeVersion.Id).ToList();

                foreach (var oldVersion in oldVersions)
                {
                    oldVersion.Published = null;
                }

                if (oldVersions.Any())
                {
                    await dbContext.SaveChangesAsync();
                }

                var factory = scopedServices.GetRequiredService<ITenantArticleLogicFactory>();
                var articleLogic = await factory.CreateForTenantAsync(domainName);
                
                await articleLogic.PublishArticle(activeVersion.Id, activeVersion.Published);

                logger.LogInformation(
                    "Article {ArticleNumber} (Domain: {Domain}): Successfully activated version {VersionNumber}",
                    articleNumber,
                    domainName,
                    activeVersion.VersionNumber);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error processing article versions for ArticleNumber {ArticleNumber} (Domain: {Domain})",
                    articleNumber,
                    domainName);
            }
        }
    }
}