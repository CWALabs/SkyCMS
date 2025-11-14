// <copyright file="SqliteConfigurationStrategy.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace AspNetCore.Identity.FlexDb.Strategies
{
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Data.Common;
    using System.Linq;

    /// <summary>
    /// Configuration strategy for SQLite.
    /// </summary>
    public class SqliteConfigurationStrategy : IDatabaseConfigurationStrategy
    {
        /// <inheritdoc/>
        public string ProviderName => "Microsoft.EntityFrameworkCore.Sqlite";

        /// <inheritdoc/>
        public int Priority => 40;

        /// <inheritdoc/>
        public bool CanHandle(string connectionString)
        {
            return !string.IsNullOrWhiteSpace(connectionString) &&
                   connectionString.StartsWith("Data Source=", StringComparison.InvariantCultureIgnoreCase) &&
                   connectionString.Contains(".db", StringComparison.InvariantCultureIgnoreCase);
        }

        /// <inheritdoc/>
        public void Configure(DbContextOptionsBuilder optionsBuilder, string connectionString)
        {
            if (optionsBuilder == null)
            {
                throw new ArgumentNullException(nameof(optionsBuilder));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            // Check if password is provided for encrypted SQLite
            if (!connectionString.Contains("Password=", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException(
                    "Encrypted SQLite requires a password in the connection string. " +
                    "Format: Data Source=/data/localdev.db;Password=yourpassword",
                    nameof(connectionString));
            }

            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var dataSource = GetConnectionStringPart(parts, "Data Source=");
            var password = GetConnectionStringPart(parts, "Password=");

            if (string.IsNullOrWhiteSpace(dataSource))
            {
                throw new ArgumentException(
                    "SQLite connection string must contain 'Data Source' parameter.",
                    nameof(connectionString));
            }

            if (connectionString.Contains("Password=InMemory;"))
            {
                // ONLY FOR DEBUGGING AND UNIT TESTS - IN MEMORY DATABASE
                connectionString = connectionString.Replace("Password=InMemory;", string.Empty);
            }
            else
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder
                {
                    DataSource = dataSource,
                    Mode = SqliteOpenMode.ReadWriteCreate,
                    Password = password
                };

                connectionString = connectionStringBuilder.ToString();
            }

            var sqliteConnection = new SqliteConnection(connectionString);
            optionsBuilder.UseSqlite(sqliteConnection);
        }

        private static string GetConnectionStringPart(string[] parts, string prefix)
        {
            var part = parts.FirstOrDefault(p => p.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase));

            if (part == null)
            {
                return null;
            }

            var indexOfEquals = part.IndexOf('=');
            return indexOfEquals >= 0 ? part.Substring(indexOfEquals + 1) : null;
        }
    }
}