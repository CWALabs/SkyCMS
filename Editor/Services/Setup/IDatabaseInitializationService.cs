// <copyright file="IDatabaseInitializationService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Setup
{
    using System.Threading.Tasks;

    /// <summary>
    /// Service responsible for initializing database schema.
    /// Handles migrations for SQL databases and container creation for Cosmos DB.
    /// </summary>
    /// <remarks>
    /// <para><strong>Service Lifetime:</strong> Scoped</para>
    /// <para><strong>Usage Patterns:</strong></para>
    /// <list type="bullet">
    /// <item><description><strong>In Controllers/Pages:</strong> Inject via constructor (automatic scoping)</description></item>
    /// <item><description><strong>In Middleware:</strong> Resolve from HttpContext.RequestServices in InvokeAsync</description></item>
    /// <item><description><strong>In Background Jobs:</strong> Create scope via IServiceProvider.CreateScope()</description></item>
    /// <item><description><strong>Never:</strong> Inject into singleton services or middleware constructors</description></item>
    /// </list>
    /// </remarks>
    public interface IDatabaseInitializationService
    {
        /// <summary>
        /// Initializes the database schema for the application.
        /// For SQL databases: applies migrations or creates tables via EnsureCreated.
        /// For Cosmos DB: creates containers if they don't exist.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        /// <param name="forceInitialization">Force initialization even if already completed.</param>
        /// <returns>A task representing the asynchronous operation, with result indicating success.</returns>
        Task<DatabaseInitializationResult> InitializeAsync(string connectionString, bool forceInitialization = false);

        /// <summary>
        /// Checks if the database has been initialized with application schema.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>True if database is initialized, false otherwise.</returns>
        Task<bool> IsInitializedAsync(string connectionString);

        /// <summary>
        /// Gets the database provider type from the connection string.
        /// </summary>
        /// <param name="connectionString">Database connection string.</param>
        /// <returns>The database provider type.</returns>
        DatabaseProviderType GetProviderType(string connectionString);
    }

    /// <summary>
    /// Result of database initialization operation.
    /// </summary>
    public class DatabaseInitializationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether initialization was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message describing the result.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the database provider type.
        /// </summary>
        public DatabaseProviderType ProviderType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tables/containers were created.
        /// </summary>
        public bool SchemaCreated { get; set; }

        /// <summary>
        /// Gets or sets any error that occurred during initialization.
        /// </summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// Database provider types.
    /// </summary>
    public enum DatabaseProviderType
    {
        /// <summary>
        /// Unknown or unsupported provider.
        /// </summary>
        Unknown,

        /// <summary>
        /// Azure Cosmos DB.
        /// </summary>
        CosmosDb,

        /// <summary>
        /// MySQL database.
        /// </summary>
        MySql,

        /// <summary>
        /// Microsoft SQL Server.
        /// </summary>
        SqlServer,

        /// <summary>
        /// PostgreSQL database.
        /// </summary>
        PostgreSql,

        /// <summary>
        /// SQLite database.
        /// </summary>
        Sqlite,
    }
}