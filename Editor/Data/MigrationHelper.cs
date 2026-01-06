using AspNetCore.Identity.FlexDb;
using Cosmos.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sky.Editor.Data
{
    /// <summary>
    /// Helper service for applying database migrations across different providers.
    /// Supports both single-tenant and multi-tenant scenarios.
    /// </summary>
    public static class MigrationHelper
    {
        /// <summary>
        /// Applies migrations for a single database connection.
        /// This method can be called from single-tenant or multi-tenant contexts.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="logger">Logger for diagnostic output.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when migration fails.</exception>
        public static async Task ApplyMigrationsAsync(string connectionString, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            }

            logger.LogInformation("Analyzing connection string to determine database provider...");

            var provider = DetermineProvider(connectionString);
            logger.LogInformation("Detected provider: {Provider}", provider);

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            switch (provider)
            {
                case DatabaseProvider.CosmosDb:
                    logger.LogInformation("Cosmos DB detected. Migrations are not supported. Using EnsureCreatedAsync() instead.");
                    CosmosDbOptionsBuilder.ConfigureDbOptions(optionsBuilder, connectionString);
                    await EnsureCosmosDbCreatedAsync(optionsBuilder.Options, logger);
                    break;

                case DatabaseProvider.SqlServer:
                    logger.LogInformation("SQL Server detected. Applying migrations...");
                    optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(MigrationHelper).Assembly.FullName);
                    });
                    await ApplyRelationalMigrationsAsync(optionsBuilder.Options, logger, provider);
                    break;

                case DatabaseProvider.MySql:
                    logger.LogInformation("MySQL detected. Applying migrations...");
                    var serverVersion = ServerVersion.AutoDetect(connectionString);
                    optionsBuilder.UseMySql(connectionString, serverVersion, mySqlOptions =>
                    {
                        mySqlOptions.MigrationsAssembly(typeof(MigrationHelper).Assembly.FullName);
                    });
                    await ApplyRelationalMigrationsAsync(optionsBuilder.Options, logger, provider);
                    break;

                case DatabaseProvider.Sqlite:
                    logger.LogInformation("SQLite detected. Applying migrations...");
                    optionsBuilder.UseSqlite(connectionString, sqliteOptions =>
                    {
                        sqliteOptions.MigrationsAssembly(typeof(MigrationHelper).Assembly.FullName);
                    });
                    await ApplyRelationalMigrationsAsync(optionsBuilder.Options, logger, provider);
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported database provider: {provider}");
            }

            logger.LogInformation("Database initialization completed successfully.");
        }

        /// <summary>
        /// Marks a migration as applied without executing it.
        /// Useful for existing databases that were created without migrations.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="migrationId">The migration ID (e.g., "20250105120000_InitialCreate_SqlServer").</param>
        /// <param name="logger">Logger for diagnostic output.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when connectionString or migrationId is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when marking migration fails or provider doesn't support migrations.</exception>
        public static async Task MarkMigrationAsAppliedAsync(string connectionString, string migrationId, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            }

            if (string.IsNullOrWhiteSpace(migrationId))
            {
                throw new ArgumentException("Migration ID cannot be null or empty", nameof(migrationId));
            }

            logger.LogInformation("Marking migration as applied: {MigrationId}", migrationId);

            var provider = DetermineProvider(connectionString);
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            switch (provider)
            {
                case DatabaseProvider.CosmosDb:
                    throw new InvalidOperationException("Cosmos DB does not support migrations.");

                case DatabaseProvider.SqlServer:
                    optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(MigrationHelper).Assembly.FullName);
                    });
                    await MarkMigrationAsAppliedInternalAsync(optionsBuilder.Options, migrationId, logger, "SQL Server");
                    break;

                case DatabaseProvider.MySql:
                    var serverVersion = ServerVersion.AutoDetect(connectionString);
                    optionsBuilder.UseMySql(connectionString, serverVersion, mySqlOptions =>
                    {
                        mySqlOptions.MigrationsAssembly(typeof(MigrationHelper).Assembly.FullName);
                    });
                    await MarkMigrationAsAppliedInternalAsync(optionsBuilder.Options, migrationId, logger, "MySQL");
                    break;

                case DatabaseProvider.Sqlite:
                    optionsBuilder.UseSqlite(connectionString, sqliteOptions =>
                    {
                        sqliteOptions.MigrationsAssembly(typeof(MigrationHelper).Assembly.FullName);
                    });
                    await MarkMigrationAsAppliedInternalAsync(optionsBuilder.Options, migrationId, logger, "SQLite");
                    break;

                default:
                    throw new InvalidOperationException($"Unsupported database provider: {provider}");
            }
        }

        /// <summary>
        /// Checks if a database exists and has been created without migrations.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        /// <param name="logger">Logger for diagnostic output.</param>
        /// <returns>True if database exists without migrations history; otherwise false.</returns>
        public static async Task<bool> DatabaseExistsWithoutMigrationsAsync(string connectionString, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            }

            var provider = DetermineProvider(connectionString);

            if (provider == DatabaseProvider.CosmosDb)
            {
                logger.LogInformation("Cosmos DB does not support migrations - skipping check.");
                return false;
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            switch (provider)
            {
                case DatabaseProvider.SqlServer:
                    optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(MigrationHelper).Assembly.FullName);
                    });
                    break;

                case DatabaseProvider.MySql:
                    var serverVersion = ServerVersion.AutoDetect(connectionString);
                    optionsBuilder.UseMySql(connectionString, serverVersion, mySqlOptions =>
                    {
                        mySqlOptions.MigrationsAssembly(typeof(MigrationHelper).Assembly.FullName);
                    });
                    break;

                case DatabaseProvider.Sqlite:
                    optionsBuilder.UseSqlite(connectionString, sqliteOptions =>
                    {
                        sqliteOptions.MigrationsAssembly(typeof(MigrationHelper).Assembly.FullName);
                    });
                    break;

                default:
                    return false;
            }

            using var context = new ApplicationDbContext(optionsBuilder.Options);

            try
            {
                // Check if database can be connected to
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return false;
                }

                // Check if migrations history table exists
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                
                // If no migrations applied but database exists, it was created without migrations
                return !appliedMigrations.Any();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error checking database migration status: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Applies migrations to a relational database (SQL Server, MySQL, SQLite).
        /// Automatically detects and handles existing databases created without migrations.
        /// </summary>
        /// <param name="options">The database context options.</param>
        /// <param name="logger">Logger for diagnostic output.</param>
        /// <param name="provider">The database provider type.</param>
        private static async Task ApplyRelationalMigrationsAsync(
            DbContextOptions<ApplicationDbContext> options, 
            ILogger logger,
            DatabaseProvider provider)
        {
            using var context = new ApplicationDbContext(options);

            try
            {
                // Check if database exists and can be connected to
                var canConnect = await context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // Check if migrations history exists
                    var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                    var appliedList = appliedMigrations.ToList();
                    
                    // Get all migrations defined in code
                    var allMigrations = context.Database.GetMigrations().ToList();
                    
                    // SCENARIO 1: Database exists but NO migration history
                    if (!appliedList.Any() && allMigrations.Any())
                    {
                        logger.LogWarning("⚠️ Database exists without migration history. This appears to be an existing database.");
                        logger.LogInformation("🔄 Automatically marking initial migration as applied to prevent schema conflicts...");
                        
                        // Get the FIRST migration (initial create) for this provider
                        var initialMigration = allMigrations
                            .OrderBy(m => m)
                            .FirstOrDefault(m => m.Contains($"InitialCreate_{provider}"));
                        
                        if (initialMigration != null)
                        {
                            logger.LogInformation("📋 Initial migration detected: {Migration}", initialMigration);
                            
                            // Mark it as applied without running it
                            var providerName = provider switch
                            {
                                DatabaseProvider.SqlServer => "SQL Server",
                                DatabaseProvider.MySql => "MySQL",
                                DatabaseProvider.Sqlite => "SQLite",
                                _ => provider.ToString()
                            };
                            
                            await MarkMigrationAsAppliedInternalAsync(options, initialMigration, logger, providerName);
                            
                            logger.LogInformation("✅ Initial migration marked as applied. Checking for additional pending migrations...");
                        }
                        else
                        {
                            logger.LogWarning("⚠️ No InitialCreate migration found for {Provider}. Database state may be inconsistent.", provider);
                        }
                    }
                }

                // Now proceed with normal migration application
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var pendingList = pendingMigrations.ToList();

                if (pendingList.Any())
                {
                    logger.LogInformation("Found {Count} pending migration(s):", pendingList.Count);
                    foreach (var migration in pendingList)
                    {
                        logger.LogInformation("  - {Migration}", migration);
                    }

                    logger.LogInformation("Applying migrations...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("✅ Migrations applied successfully.");
                }
                else
                {
                    logger.LogInformation("✅ Database is up to date. No pending migrations.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Migration failed: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Marks a migration as applied in the database.
        /// </summary>
        private static async Task MarkMigrationAsAppliedInternalAsync(
            DbContextOptions<ApplicationDbContext> options,
            string migrationId,
            ILogger logger,
            string providerName)
        {
            using var context = new ApplicationDbContext(options);

            try
            {
                // Get EF version
                var productVersion = typeof(DbContext).Assembly
                    .GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false)
                    .Cast<System.Reflection.AssemblyInformationalVersionAttribute>()
                    .FirstOrDefault()?.InformationalVersion?.Split('+')[0] ?? "9.0.0";

                // Use provider-specific SQL syntax
                string sql;
                
                switch (providerName)
                {
                    case "SQL Server":
                        // SQL Server requires IF NOT EXISTS with different syntax
                        sql = @"
                            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[__EFMigrationsHistory]') AND type = 'U')
                            BEGIN
                                CREATE TABLE [__EFMigrationsHistory] (
                                    [MigrationId] NVARCHAR(150) NOT NULL PRIMARY KEY,
                                    [ProductVersion] NVARCHAR(32) NOT NULL
                                );
                            END
                            
                            IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = @p0)
                            BEGIN
                                INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) 
                                VALUES (@p0, @p1);
                            END";
                        break;

                    case "MySQL":
                    case "SQLite":
                        // MySQL and SQLite support CREATE TABLE IF NOT EXISTS
                        sql = @"
                            CREATE TABLE IF NOT EXISTS __EFMigrationsHistory (
                                MigrationId TEXT NOT NULL PRIMARY KEY,
                                ProductVersion TEXT NOT NULL
                            );
                            
                            INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
                            SELECT @p0, @p1
                            WHERE NOT EXISTS (
                                SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = @p0
                            );";
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported database provider: {providerName}");
                }

                var rowsAffected = await context.Database.ExecuteSqlRawAsync(sql, migrationId, productVersion);

                if (rowsAffected > 0)
                {
                    logger.LogInformation("✅ Migration '{MigrationId}' marked as applied in {Provider}", migrationId, providerName);
                }
                else
                {
                    logger.LogInformation("ℹ️ Migration '{MigrationId}' was already marked as applied in {Provider}", migrationId, providerName);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to mark migration as applied: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to mark migration as applied: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ensures Cosmos DB database and containers are created.
        /// </summary>
        private static async Task EnsureCosmosDbCreatedAsync(DbContextOptions<ApplicationDbContext> options, ILogger logger)
        {
            using var context = new ApplicationDbContext(options);

            try
            {
                logger.LogInformation("Ensuring Cosmos DB database and containers exist...");
                var created = await context.Database.EnsureCreatedAsync();

                if (created)
                {
                    logger.LogInformation("✅ Cosmos DB database and containers created successfully.");
                }
                else
                {
                    logger.LogInformation("✅ Cosmos DB database already exists.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to ensure Cosmos DB creation: {Message}", ex.Message);
                throw new InvalidOperationException($"Failed to ensure Cosmos DB creation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Determines the database provider from a connection string.
        /// </summary>
        private static DatabaseProvider DetermineProvider(string connectionString)
        {
            if (connectionString.Contains("AccountEndpoint", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseProvider.CosmosDb;
            }

            // Check for SQL Server-specific keywords BEFORE checking for "Data Source="
            // SQL Server uses: Initial Catalog, Integrated Security, User ID, TrustServerCertificate, etc.
            if (connectionString.Contains("Initial Catalog", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("Integrated Security", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("TrustServerCertificate", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("Trust Server Certificate", StringComparison.OrdinalIgnoreCase) ||
                (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) && 
                 connectionString.Contains("\\", StringComparison.OrdinalIgnoreCase))) // Named instances like COMPUTER\SQLEXPRESS
            {
                return DatabaseProvider.SqlServer;
            }

            if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) &&
                !connectionString.Contains("Port=3306", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseProvider.SqlServer;
            }

            if (connectionString.Contains("Port=3306", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) &&
                connectionString.Contains("Uid=", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseProvider.MySql;
            }

            // SQLite check LAST (since it only uses "Data Source=" without SQL Server keywords)
            if (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
            {
                return DatabaseProvider.Sqlite;
            }

            throw new InvalidOperationException("Unable to determine database provider from connection string");
        }

        /// <summary>
        /// Database provider types.
        /// </summary>
        private enum DatabaseProvider
        {
            CosmosDb,
            SqlServer,
            MySql,
            Sqlite
        }
    }
}