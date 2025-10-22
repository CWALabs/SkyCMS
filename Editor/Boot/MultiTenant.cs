// <copyright file="MultiTenant.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Boot
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.RateLimiting;
    using System.Threading.Tasks;
    using System.Web;
    using AspNetCore.Identity.FlexDb;
    using AspNetCore.Identity.FlexDb.Extensions;
    using Azure.Identity;
    using Cosmos.BlobService;
    using Cosmos.Cms.Common.Services.Configurations;
    using Cosmos.Common.Data;
    using Cosmos.Common.Services.Configurations;
    using Cosmos.DynamicConfig;
    using Cosmos.Editor.Services;
    using Cosmos.EmailServices;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.RateLimiting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json.Serialization;
    using Sky.Cms.Hubs;
    using Sky.Cms.Services;
    using Sky.Editor.Data.Logic;

    /// <summary>
    ///  Creates a multi-tenant web application.
    /// </summary>
    internal class MultiTenant
    {
        /// <summary>
        /// Builds a multi-tenant web application.
        /// </summary>
        /// <param name="builder">Web application builder.</param>
        /// <param name="defaultAzureCredential">Default Azure credential.</param>
        internal static void Configure(WebApplicationBuilder builder, DefaultAzureCredential defaultAzureCredential)
        {
            // These services are used to determine the connection string at runtime.
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton<IDynamicConfigurationProvider, DynamicConfigurationProvider>();
            builder.Services.AddSingleton<MultiDatabaseManagementUtilities>();

            // Note that this is transient, meaning for each request this is regenerated.
            // Multi-tenant support is enabled because each request may have a different domain name and connection
            // string information.
            builder.Services.AddTransient(serviceProvider =>
            {
                var optionsBuilder = GetDynamicOptionsBuilder(serviceProvider);
                return new ApplicationDbContext(optionsBuilder.Options);
            });

            // This service is used to run startup tasks asynchronously.
            builder.Services.AddScoped<IStartupTaskService, StartupTaskService>();
        }

        /// <summary>
        /// Gets the DbContext options using the dynamic configuration provider.
        /// </summary>
        /// <param name="services">Services collection.</param>
        /// <returns>DbApplicationContext.</returns>
        private static DbContextOptionsBuilder<ApplicationDbContext> GetDynamicOptionsBuilder(IServiceProvider services)
        {
            var connectionStringProvider = services.GetRequiredService<IDynamicConfigurationProvider>();
            var connectionString = connectionStringProvider.GetDatabaseConnectionString();

            // Note: This may be null if the cookie or website URL has not yet been set.
            if (string.IsNullOrEmpty(connectionString))
            {
                return new DbContextOptionsBuilder<ApplicationDbContext>();
            }

            return CosmosDbOptionsBuilder.GetDbOptionsBuilder<ApplicationDbContext>(connectionString);
        }
    }
}
