// <copyright file="MultiTenant.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Boot
{
    using System;
    using System.Linq;
    using AspNetCore.Identity.FlexDb;
    using Azure.Identity;
    using Cosmos.BlobService;
    using Cosmos.Common.Data;
    using Cosmos.DynamicConfig;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Multi-tenant configuration for the application.
    /// </summary>
    public static class MultiTenant
    {
        /// <summary>
        /// Configures services for multi-tenant mode.
        /// Ensures database schemas exist for config database and all tenants.
        /// </summary>
        /// <param name="builder">Web application builder.</param>
        /// <param name="credential">Azure credential.</param>
        public static void Configure(WebApplicationBuilder builder, DefaultAzureCredential credential)
        {
            var logger = LoggerFactory.Create(config => config.AddConsole())
                .CreateLogger("MultiTenant.Configure");

            var configConnectionString = builder.Configuration.GetConnectionString("ConfigDbConnectionString");
            var allowSetup = builder.Configuration.GetValue<bool?>("CosmosAllowSetup") ?? false;

            // If no config connection string is provided, don't configure yet
            if (string.IsNullOrEmpty(configConnectionString))
            {
                logger.LogWarning("No config connection string - database will be configured during setup");
                return;
            }

            // Ensure config database schema exists if setup is allowed
            if (allowSetup)
            {
                try
                {
                    logger.LogInformation("Checking config database schema for multi-tenant mode...");
                    EnsureConfigDatabaseSchemaExists(configConnectionString, logger);
                    logger.LogInformation("Config database schema is ready");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Database schema initialization failed - application will continue but may have limited functionality");
                }
            }

            // Configure the DynamicConfigDbContext
            builder.Services.AddDbContext<DynamicConfigDbContext>(options =>
            {
                CosmosDbOptionsBuilder.ConfigureDbOptions(options, configConnectionString);
            });

            // Ensure all tenant database schemas exist if setup is allowed
            if (allowSetup)
            {
                try
                {
                    logger.LogInformation("Checking tenant database schemas...");
                    EnsureTenantSchemasExist(configConnectionString, logger);
                    logger.LogInformation("All tenant database schemas are ready");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Tenant schema initialization failed - some tenants may have limited functionality");
                }
            }
        }

        /// <summary>
        /// Ensures the config database schema exists.
        /// </summary>
        /// <param name="connectionString">Config database connection string.</param>
        /// <param name="logger">Logger instance.</param>
        private static void EnsureConfigDatabaseSchemaExists(string connectionString, ILogger logger)
        {
            try
            {
                using var dbContext = new DynamicConfigDbContext(CosmosDbOptionsBuilder.GetDbOptions<DynamicConfigDbContext>(connectionString));
                
                if (!dbContext.Database.CanConnect())
                {
                    logger.LogInformation("Config database not accessible - creating database and schema");
                    dbContext.Database.EnsureCreated();
                    logger.LogInformation("Config database and schema created successfully");
                    return;
                }

                // Verify schema exists
                try
                {
                    dbContext.Database.EnsureCreated();
                    logger.LogInformation("Config database schema verified");
                }
                catch
                {
                    logger.LogInformation("Config database schema incomplete or missing - creating schema");
                    dbContext.Database.EnsureCreated();
                    logger.LogInformation("Config database schema created successfully");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to ensure config database schema exists");
                throw;
            }
        }

        /// <summary>
        /// Ensures database schemas exist for all tenants.
        /// </summary>
        /// <param name="configConnectionString">Config database connection string.</param>
        /// <param name="logger">Logger instance.</param>
        private static void EnsureTenantSchemasExist(string configConnectionString, ILogger logger)
        {
            try
            {
                using var configDbContext = new DynamicConfigDbContext(CosmosDbOptionsBuilder.GetDbOptions<DynamicConfigDbContext>(configConnectionString));
                
                // Get all tenant configurations from the config database
                var tenantConfigs = configDbContext.Connections.ToList();

                if (!tenantConfigs.Any())
                {
                    logger.LogInformation("No tenants found in config database - skipping tenant schema initialization");
                    return;
                }

                logger.LogInformation("Found {TenantCount} tenant(s) - checking schemas", tenantConfigs.Count);

                foreach (var tenantConfig in tenantConfigs)
                {
                    try
                    {
                        logger.LogInformation("Checking schema for tenant: {TenantName} (ID: {TenantId})", 
                            tenantConfig.DomainNames, tenantConfig.Id);
                        
                        // Get the tenant's database connection string
                        var tenantConnectionString = tenantConfig.DbConn;
                        
                        if (string.IsNullOrEmpty(tenantConnectionString))
                        {
                            logger.LogWarning("Tenant {TenantName} has no connection string - skipping", tenantConfig.DomainNames);
                            continue;
                        }

                        using var tenantDbContext = new ApplicationDbContext(
                            CosmosDbOptionsBuilder.GetDbOptions<ApplicationDbContext>(tenantConnectionString));
                        
                        if (!tenantDbContext.Database.CanConnect())
                        {
                            logger.LogInformation("Tenant database not accessible - creating database and schema for {TenantName}", 
                                tenantConfig.DomainNames);
                            tenantDbContext.Database.EnsureCreated();
                            logger.LogInformation("Tenant database and schema created for {TenantName}", tenantConfig.DomainNames);
                            continue;
                        }

                        // Verify schema by attempting to query a core table
                        try
                        {
                            var testQuery = tenantDbContext.Settings.FirstOrDefault();
                            logger.LogInformation("Tenant schema verified for {TenantName}", tenantConfig.DomainNames);
                        }
                        catch
                        {
                            logger.LogInformation("Tenant schema incomplete - creating schema for {TenantName}", 
                                tenantConfig.DomainNames);
                            tenantDbContext.Database.EnsureCreated();
                            logger.LogInformation("Tenant schema created for {TenantName}", tenantConfig.DomainNames);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to ensure schema for tenant: {TenantName} (ID: {TenantId})", 
                            tenantConfig.DomainNames, tenantConfig.Id);
                        // Don't throw - continue with other tenants
                    }
                }

                logger.LogInformation("Completed tenant schema initialization for all tenants");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to check tenant schemas");
                throw;
            }
        }
    }
}
