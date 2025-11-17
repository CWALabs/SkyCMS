// <copyright file="MultiTenant.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Boot
{
    using System;
    using AspNetCore.Identity.FlexDb;
    using Azure.Identity;
    using Cosmos.Common.Data;
    using Cosmos.DynamicConfig;
    using Cosmos.Editor.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

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
        /// <exception cref="InvalidOperationException">Thrown when tenant connection cannot be resolved.</exception>
        private static DbContextOptionsBuilder<ApplicationDbContext> GetDynamicOptionsBuilder(IServiceProvider services)
        {
            var connectionStringProvider = services.GetRequiredService<IDynamicConfigurationProvider>();
            var logger = services.GetService<ILogger<ApplicationDbContext>>();
            
            string? connectionString = null;
            
            try
            {
                connectionString = connectionStringProvider.GetDatabaseConnectionString();
            }
            catch (InvalidOperationException ex)
            {
                logger?.LogError(ex, "Failed to resolve tenant database connection");
                throw new InvalidOperationException(
                    "Cannot create ApplicationDbContext: Tenant connection string resolution failed. " +
                    "This may occur during application startup or in background jobs without HTTP context. " +
                    "Ensure domain context is available or provide domain explicitly.", ex);
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                logger?.LogError("Tenant connection string is null or empty");
                throw new InvalidOperationException(
                    "Cannot create ApplicationDbContext: Tenant connection string is null or empty. " +
                    "Verify that the domain is correctly configured in the multi-tenant configuration database.");
            }

            logger?.LogDebug("Creating DbContext with tenant connection string");
            return CosmosDbOptionsBuilder.GetDbOptionsBuilder<ApplicationDbContext>(connectionString);
        }
    }
}
