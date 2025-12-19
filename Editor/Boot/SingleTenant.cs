// <copyright file="SingleTenant.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Boot
{
    using AspNetCore.Identity.FlexDb.Extensions;
    using Cosmos.BlobService;
    using Cosmos.Common.Data;
    using Cosmos.Common.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Sky.Editor.Data;
    using System;

    /// <summary>
    /// Boots up the single-tenant editor.
    /// </summary>
    internal static class SingleTenant
    {
        /// <summary>
        /// Configures the single-tenant editor.
        /// </summary>
        /// <param name="builder">Web application builder.</param>
        internal static void Configure(WebApplicationBuilder builder)
        {
            // Database connection string
            var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection");

            // Backup storage connection string
            var backupConnectionString = builder.Configuration.GetConnectionString("BackupStorageConnectionString");

            // If there is a backup connection string, then restore the database file now.
            // Also, on shutdown of the application, upload the database file to storage.
            if (!string.IsNullOrEmpty(backupConnectionString))
            {
                // Restore any files from blob storage to local file system.
                var restoreService = new FileBackupRestoreService(builder.Configuration, new MemoryCache(new MemoryCacheOptions()));
                restoreService.DownloadAsync(connectionString).Wait();
            }

            // Read AllowSetup directly from configuration
            // If this is set to true, the setup wizard will be accessible at /___setup
            // IMPORTANT: The database will be initialized during setup wizard completion,
            // NOT at application startup. This improves startup performance and ensures
            // proper initialization through the IDatabaseInitializationService.
            var allowSetup = builder.Configuration.GetValue<bool?>("CosmosAllowSetup") ?? false;
            System.Console.WriteLine(allowSetup 
                ? "Setup mode enabled - database will be initialized during setup wizard completion" 
                : "Setup mode disabled - database should already be initialized");

            // ✅ Database initialization is now handled exclusively by:
            //    SetupService.CompleteSetupAsync() -> IDatabaseInitializationService.InitializeAsync()
            // This ensures:
            // - Consistent initialization across all database providers
            // - Proper error handling and logging
            // - Non-blocking application startup
            // - Idempotent initialization (won't re-initialize if already done)

            // Add the DB context using FlexDb auto-configuration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                AspNetCore.Identity.FlexDb.CosmosDbOptionsBuilder.ConfigureDbOptions(options, connectionString);
            });
        }
    }
}
