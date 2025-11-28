// <copyright file="HangFireExtensions.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Scheduling
{
    using System;
    using System.Linq;
    using AspNetCore.Identity.FlexDb;
    using AspNetCore.Identity.FlexDb.Strategies;
    using Hangfire;
    using Hangfire.MySql;
    using Microsoft.Azure.Cosmos;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    ///  HangFire extensions for service collection.
    /// </summary>
    public static class HangFireExtensions
    {
        /// <summary>
        ///  Add HangFire scheduling services using Entity Framework Core storage.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="config">Configuration.</param>
        public static void AddHangFireScheduling(this IServiceCollection services, IConfiguration config)
        {
            var multi = config.GetValue<bool?>("MultiTenantEditor") ?? false;

            if (multi)
            {
                services.AddHangfire(config =>
                {
                    // Do this for now until I get the multi-tenant storage figured out.
                    config.UseInMemoryStorage();
                });
            }
            else
            {
                var connectionString = multi ? config.GetConnectionString("ConfigDbConnectionString")
                        : config.GetConnectionString("ApplicationDbContextConnection");

                // Determine if using Cosmos DB
                var isCosmosDb = CosmosDbOptionsBuilder
                    .GetDefaultStrategies()
                    .OfType<CosmosDbConfigurationStrategy>()
                    .Any(strategy => strategy.CanHandle(connectionString));

                var isMsSql = CosmosDbOptionsBuilder
                    .GetDefaultStrategies()
                    .OfType<SqlServerConfigurationStrategy>()
                    .Any(strategy => strategy.CanHandle(connectionString));

                var isMySql = CosmosDbOptionsBuilder
                    .GetDefaultStrategies()
                    .OfType<MySqlConfigurationStrategy>()
                    .Any(strategy => strategy.CanHandle(connectionString));

                if (isCosmosDb)
                {
                    // Parse Cosmos DB connection details from connection string.
                    services.AddHangfire(hangfireConfig =>
                    {
                        var conn = multi ? config.GetConnectionString("ConfigDbConnectionString")
                                : config.GetConnectionString("ApplicationDbContextConnection");
                        var accountProperties = CosmosDbConfigurationStrategy.GetAccountProperties(conn);
                        hangfireConfig.UseAzureCosmosDbStorage(
                            accountProperties.AccountEndpoint,
                            accountProperties.AccountKey,
                            accountProperties.DatabaseName,
                            "hangfire",
                            new CosmosClientOptions());
                    });
                }
                else if (isMsSql)
                {
                    services.AddHangfire(hangfireConfig =>
                    {
                        hangfireConfig.UseSqlServerStorage(connectionString);
                    });
                }
                else if (isMySql)
                {
                    services.AddHangfire(hangfireConfig =>
                    {
                        hangfireConfig.UseStorage(
                            new MySqlStorage(connectionString + "Allow User Variables=true;", new MySqlStorageOptions()));
                    });
                }
                else
                {
                    throw new InvalidOperationException("Unsupported database provider for Hangfire storage.");
                }
            }

            // Configure Hangfire server options
            services.AddHangfireServer(options =>
            {
                options.Queues = new[] { "critical", "default" };
                options.WorkerCount = Math.Max(Environment.ProcessorCount, 1);
                options.SchedulePollingInterval = TimeSpan.FromMinutes(10);
                options.ShutdownTimeout = TimeSpan.FromMinutes(2);
                options.HeartbeatInterval = TimeSpan.FromMinutes(5);
            });
        }

    }
}
