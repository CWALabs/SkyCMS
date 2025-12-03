// <copyright file="SingleTenant.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Boot
{
    using Cosmos.Common.Data;
    using Cosmos.Common.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

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
            // If this is set, the Cosmos identity provider will:
            // 1. Create the database if it does not already exist.
            // 2. Create the required containers if they do not already exist.
            // IMPORTANT: Remove this variable after first run. It will improve startup performance.
            var allowSetup = builder.Configuration.GetValue<bool?>("CosmosAllowSetup") ?? false;
            
            if (allowSetup)
            {
                using var context = new ApplicationDbContext(connectionString);

                if (context.Database.IsCosmos())
                {
                    // EnsureCreated is necessary for Cosmos DB to create the database and containers.
                    // It does not support migrations.
                    context.Database.EnsureCreatedAsync().Wait();
                }
                else
                {
                    // For relational databases, you can optionally run migrations:
                    // context.Database.MigrateAsync().Wait();
                }
            }

            // Add the DB context using FlexDb auto-configuration
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                AspNetCore.Identity.FlexDb.CosmosDbOptionsBuilder.ConfigureDbOptions(options, connectionString);
            });
        }
    }
}
