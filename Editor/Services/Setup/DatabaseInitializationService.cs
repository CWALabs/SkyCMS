// <copyright file="DatabaseInitializationService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Setup
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Service for initializing database schema across different database providers.
    /// </summary>
    public class DatabaseInitializationService : IDatabaseInitializationService
    {
        private readonly ILogger<DatabaseInitializationService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInitializationService"/> class.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        public DatabaseInitializationService(ILogger<DatabaseInitializationService> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<DatabaseInitializationResult> InitializeAsync(string connectionString, bool forceInitialization = false)
        {
            var result = new DatabaseInitializationResult
            {
                ProviderType = GetProviderType(connectionString),
            };

            try
            {
                logger.LogInformation("=== Database Initialization Started ===");
                logger.LogInformation("Provider Type: {ProviderType}", result.ProviderType);

                // Check if already initialized
                if (!forceInitialization && await IsInitializedAsync(connectionString))
                {
                    logger.LogInformation("Database already initialized - skipping");
                    result.Success = true;
                    result.Message = "Database already initialized";
                    result.SchemaCreated = false;
                    return result;
                }

                // Create context
                using var context = new ApplicationDbContext(connectionString);

                // Verify connection
                if (!context.Database.CanConnect())
                {
                    throw new InvalidOperationException("Cannot connect to database. Check connection string and network.");
                }

                logger.LogInformation("✅ Database connection successful");

                // Initialize based on provider type
                switch (result.ProviderType)
                {
                    case DatabaseProviderType.CosmosDb:
                        await InitializeCosmosDbAsync(context, result);
                        break;

                    case DatabaseProviderType.MySql:
                    case DatabaseProviderType.SqlServer:
                    case DatabaseProviderType.PostgreSql:
                    case DatabaseProviderType.Sqlite:
                        await InitializeSqlDatabaseAsync(context, result);
                        break;

                    default:
                        throw new NotSupportedException($"Database provider type {result.ProviderType} is not supported");
                }

                // Verify initialization
                if (!await IsInitializedAsync(connectionString))
                {
                    throw new InvalidOperationException("Database initialization completed but verification failed");
                }

                logger.LogInformation("✅ Database initialization verified");
                logger.LogInformation("=== Database Initialization Complete ===");

                result.Success = true;
                result.Message = "Database initialized successfully";
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Database initialization failed");
                result.Success = false;
                result.Error = ex.Message;
                result.Message = $"Database initialization failed: {ex.Message}";
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> IsInitializedAsync(string connectionString)
        {
            try
            {
                using var context = new ApplicationDbContext(connectionString);

                if (!context.Database.CanConnect())
                {
                    return false;
                }

                // Try to query critical tables/containers
                // If any of these fail, database is not initialized
                _ = await context.Settings.AnyAsync();
                _ = await context.Articles.AnyAsync();
                _ = await context.Users.AnyAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogDebug("Database initialization check failed: {Message}", ex.Message);
                return false;
            }
        }

        /// <inheritdoc/>
        public DatabaseProviderType GetProviderType(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return DatabaseProviderType.Unknown;
            }

            var lowerConnectionString = connectionString.ToLowerInvariant();

            // Cosmos DB detection
            if (lowerConnectionString.Contains("accountendpoint") || 
                lowerConnectionString.Contains("cosmosdb") ||
                lowerConnectionString.Contains("documents.azure.com"))
            {
                return DatabaseProviderType.CosmosDb;
            }

            // MySQL detection
            if (lowerConnectionString.Contains("server=") && 
                (lowerConnectionString.Contains("mysql") || 
                 lowerConnectionString.Contains("port=3306")))
            {
                return DatabaseProviderType.MySql;
            }

            // SQL Server detection
            if (lowerConnectionString.Contains("data source=") || 
                lowerConnectionString.Contains("server=") && 
                (lowerConnectionString.Contains("database.windows.net") ||
                 lowerConnectionString.Contains("trustservercertificate")))
            {
                return DatabaseProviderType.SqlServer;
            }

            // PostgreSQL detection
            if (lowerConnectionString.Contains("host=") && 
                (lowerConnectionString.Contains("port=5432") || 
                 lowerConnectionString.Contains("postgres")))
            {
                return DatabaseProviderType.PostgreSql;
            }

            // SQLite detection
            if (lowerConnectionString.Contains("data source=") && 
                (lowerConnectionString.EndsWith(".db") || 
                 lowerConnectionString.EndsWith(".sqlite")))
            {
                return DatabaseProviderType.Sqlite;
            }

            return DatabaseProviderType.Unknown;
        }

        /// <summary>
        /// Initializes Cosmos DB by creating containers.
        /// </summary>
        private async Task InitializeCosmosDbAsync(ApplicationDbContext context, DatabaseInitializationResult result)
        {
            logger.LogInformation("Initializing Cosmos DB containers...");

            // For Cosmos DB, EnsureCreated creates the database and containers
            var created = await context.Database.EnsureCreatedAsync();

            if (created)
            {
                logger.LogInformation("✅ Cosmos DB database and containers created");
                result.SchemaCreated = true;
            }
            else
            {
                logger.LogInformation("Cosmos DB database already exists - verifying containers...");
                
                // Verify containers exist by querying them
                try
                {
                    _ = await context.Settings.AnyAsync();
                    _ = await context.Articles.AnyAsync();
                    logger.LogInformation("✅ Cosmos DB containers verified");
                    result.SchemaCreated = false;
                }
                catch
                {
                    // Containers don't exist but database does - this shouldn't happen with Cosmos
                    throw new InvalidOperationException("Cosmos DB exists but containers are missing. Manual intervention required.");
                }
            }
        }

        /// <summary>
        /// Initializes SQL database by creating tables.
        /// </summary>
        private async Task InitializeSqlDatabaseAsync(ApplicationDbContext context, DatabaseInitializationResult result)
        {
            logger.LogInformation("Initializing SQL database tables...");

            // Check if tables already exist
            var tablesExist = false;
            try
            {
                _ = await context.Settings.AnyAsync();
                tablesExist = true;
                logger.LogInformation("Application tables already exist");
            }
            catch
            {
                logger.LogInformation("Application tables do not exist");
            }

            if (!tablesExist)
            {
                // Try EnsureCreated first (works if database doesn't exist)
                var databaseCreated = await context.Database.EnsureCreatedAsync();

                if (databaseCreated)
                {
                    logger.LogInformation("✅ Database and tables created via EnsureCreated()");
                    result.SchemaCreated = true;
                }
                else
                {
                    // Database exists but tables don't (Hangfire scenario)
                    logger.LogInformation("Database exists but application tables missing - creating schema via script...");

                    try
                    {
                        var script = context.Database.GenerateCreateScript();
                        await context.Database.ExecuteSqlRawAsync(script);
                        logger.LogInformation("✅ Application tables created via SQL script");
                        result.SchemaCreated = true;
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to create tables via SQL script");
                        throw new InvalidOperationException("Failed to create database tables. Check database permissions.", ex);
                    }
                }

                // Verify tables were created
                try
                {
                    _ = await context.Settings.AnyAsync();
                    _ = await context.Articles.AnyAsync();
                    _ = await context.Users.AnyAsync();
                    logger.LogInformation("✅ Table verification successful");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Table creation appeared to succeed but verification failed.", ex);
                }
            }
            else
            {
                result.SchemaCreated = false;
            }
        }
    }
}