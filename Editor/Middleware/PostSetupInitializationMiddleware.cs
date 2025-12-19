// <copyright file="PostSetupInitializationMiddleware.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Cosmos.Common.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sky.Editor.Data;
using Sky.Editor.Data.Logic;
using Sky.Editor.Services.Setup;

namespace Sky.Editor.Middleware
{
    /// <summary>
    /// Middleware that handles per-tenant post-setup initialization.
    /// </summary>
    /// <remarks>
    /// This middleware performs post-setup tasks such as creating the home page after
    /// the application has restarted following setup completion. It does NOT initialize
    /// the database - that is handled by the setup wizard.
    /// </remarks>
    public class PostSetupInitializationMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<PostSetupInitializationMiddleware> logger;
        private readonly bool isMultiTenant;
        private static readonly ConcurrentDictionary<string, bool> ProcessedTenants = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="PostSetupInitializationMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next request delegate.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="configuration">Configuration.</param>
        public PostSetupInitializationMiddleware(
            RequestDelegate next,
            ILogger<PostSetupInitializationMiddleware> logger,
            IConfiguration configuration)
        {
            this.next = next;
            this.logger = logger;
            isMultiTenant = configuration.GetValue<bool?>("MultiTenantEditor") ?? false;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">HTTP context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // Only process if multi-tenant mode is enabled
            if (isMultiTenant)
            {
                try
                {
                    // Get tenant identifier (adjust based on your tenant resolution strategy)
                    var tenantId = GetTenantIdentifier(context);

                    // Check if this tenant has already been processed
                    if (!ProcessedTenants.ContainsKey(tenantId))
                    {
                        // Use a lock per tenant to prevent race conditions
                        if (ProcessedTenants.TryAdd(tenantId, false))
                        {
                            await ProcessTenantSetupAsync(context, tenantId);
                            ProcessedTenants[tenantId] = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process post-setup initialization");
                    // Don't throw - continue with the request
                }
            }

            await next(context);
        }

        private async Task ProcessTenantSetupAsync(HttpContext context, string tenantId)
        {
            // ✅ Resolve scoped services from the request scope
            var dbInitService = context.RequestServices.GetRequiredService<IDatabaseInitializationService>();
            var configuration = context.RequestServices.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("ApplicationDbContextConnection");

            // ✅ VERIFY database is initialized - do NOT initialize during HTTP requests
            // Database initialization should only happen during setup wizard completion
            if (!await dbInitService.IsInitializedAsync(connectionString))
            {
                logger.LogWarning(
                    "Database not initialized for tenant {TenantId}. Please complete setup wizard at /___setup",
                    tenantId);

                // Check if setup is allowed
                var allowSetup = configuration.GetValue<bool?>("CosmosAllowSetup") ?? false;

                if (allowSetup && !context.Request.Path.StartsWithSegments("/___setup"))
                {
                    // Redirect to setup wizard
                    logger.LogInformation("Redirecting tenant {TenantId} to setup wizard", tenantId);
                    context.Response.Redirect("/___setup");
                    return;
                }

                // If setup not allowed or already on setup page, continue but log warning
                return;
            }

            var dbContext = context.RequestServices.GetRequiredService<ApplicationDbContext>();

            // Check if home page creation is pending
            var pendingSetting = await dbContext.Settings
                .FirstOrDefaultAsync(s => s.Group == "SETUP" && s.Name == "PendingHomePageCreation");

            if (pendingSetting?.Value == "true")
            {
                logger.LogInformation("Detected pending home page creation for tenant {TenantId}. Creating home page...", tenantId);

                var userIdSetting = await dbContext.Settings
                    .FirstOrDefaultAsync(s => s.Group == "SETUP" && s.Name == "HomePageUserId");
                var titleSetting = await dbContext.Settings
                    .FirstOrDefaultAsync(s => s.Group == "SETUP" && s.Name == "HomePageTitle");
                var templateIdSetting = await dbContext.Settings
                    .FirstOrDefaultAsync(s => s.Group == "SETUP" && s.Name == "HomePageTemplateId");

                if (userIdSetting != null && titleSetting != null && Guid.TryParse(userIdSetting.Value, out var userId))
                {
                    // Check if home page already exists
                    var existingHomePage = await dbContext.Articles
                        .FirstOrDefaultAsync(a => a.ArticleNumber == 1 && a.UrlPath == "root");

                    if (existingHomePage == null)
                    {
                        var articleLogic = context.RequestServices.GetRequiredService<ArticleEditLogic>();

                        Guid? templateId = null;
                        if (templateIdSetting != null && Guid.TryParse(templateIdSetting.Value, out var parsedTemplateId))
                        {
                            templateId = parsedTemplateId;
                        }

                        // Create the home page
                        var model = await articleLogic.CreateArticle(titleSetting.Value, userId, templateId);

                        logger.LogInformation("Home page created successfully for tenant {TenantId} with article number {ArticleNumber}", tenantId, model.ArticleNumber);
                    }
                    else
                    {
                        logger.LogInformation("Home page already exists for tenant {TenantId}, skipping creation", tenantId);
                    }

                    // Clear the pending flags
                    dbContext.Settings.Remove(pendingSetting);
                    if (userIdSetting != null)
                    {
                        dbContext.Settings.Remove(userIdSetting);
                    }

                    if (titleSetting != null)
                    {
                        dbContext.Settings.Remove(titleSetting);
                    }

                    if (templateIdSetting != null)
                    {
                        dbContext.Settings.Remove(templateIdSetting);
                    }

                    await dbContext.SaveChangesAsync();

                    logger.LogInformation("Post-setup initialization completed successfully for tenant {TenantId}", tenantId);
                }
                else
                {
                    logger.LogWarning("Missing or invalid settings for home page creation for tenant {TenantId}", tenantId);
                }
            }
        }

        private string GetTenantIdentifier(HttpContext context)
        {
            // Adjust this based on your tenant resolution strategy
            // Examples:
            // - Subdomain: context.Request.Host.Host
            // - Header: context.Request.Headers["X-Tenant-Id"]
            // - Path: context.Request.Path segments
            // - For single-tenant: return a constant like "default"

            var host = context.Request.Host.Host;
            return host; // For subdomain-based multi-tenancy
        }
    }
}