// <copyright file="TestSettings.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests
{
    /// <summary>
    /// Test configuration settings loaded from user secrets or appsettings.
    /// </summary>
    public class TestSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to use cloud databases (Azure SQL, MySQL, CosmosDB) instead of SQLite.
        /// </summary>
        /// <remarks>
        /// When true, tests will use cloud database connections from ConnectionStrings.
        /// When false (default), tests will use SQLite in-memory databases.
        /// </remarks>
        public bool UseCloudDatabases { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to skip slow-running integration tests.
        /// </summary>
        public bool SkipSlowTests { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to clean up test data after tests complete.
        /// </summary>
        public bool CleanupAfterTests { get; set; } = true;
    }
}
