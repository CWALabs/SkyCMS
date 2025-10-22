// <copyright file="SingleTenant.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Boot
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.RateLimiting;
    using System.Threading.Tasks;
    using System.Web;
    using AspNetCore.Identity.FlexDb.Extensions;
    using Azure.Identity;
    using Cosmos.BlobService;
    using Cosmos.Cms.Common.Services.Configurations;
    using Cosmos.Common.Data;
    using Cosmos.Common.Services;
    using Cosmos.Common.Services.Configurations;
    using Cosmos.EmailServices;
    using Hangfire;
    using Hangfire.InMemory;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
    using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.RateLimiting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Serialization;
    using Sky.Cms.Hubs;
    using Sky.Cms.Services;
    using Sky.Editor.Data.Logic;
    using Sky.Editor.Domain.Events;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services;
    using Sky.Editor.Services.Authors;
    using Sky.Editor.Services.Catalog;
    using Sky.Editor.Services.Html;
    using Sky.Editor.Services.Publishing;
    using Sky.Editor.Services.Redirects;
    using Sky.Editor.Services.ReservedPaths;
    using Sky.Editor.Services.Slugs;
    using Sky.Editor.Services.Titles;

    /// <summary>
    /// Boots up the multi-tenant editor.
    /// </summary>
    internal static class SingleTenant
    {
        /// <summary>
        /// Builds up the multi-tenant editor.
        /// </summary>
        /// <param name="builder">Web application builder.</param>
        /// <param name="options">Cosmos configuration options.</param>
        internal static void Configure(WebApplicationBuilder builder, IOptions<CosmosConfig> options)
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

            // If this is set, the Cosmos identity provider will:
            // 1. Create the database if it does not already exist.
            // 2. Create the required containers if they do not already exist.
            // IMPORTANT: Remove this variable if after first run. It will improve startup performance.
            // If the following is set, it will create the Cosmos database and
            //  required containers.
            if (options.Value.SiteSettings.AllowSetup)
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
                    //context.Database.MigrateAsync().Wait();
                }
            }

            // Add the DB context using this approach instead of AddDbContext.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                AspNetCore.Identity.FlexDb.CosmosDbOptionsBuilder.ConfigureDbOptions(options, connectionString);
            });
        }
    }
}
