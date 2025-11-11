// <copyright file="CosmosDbConfigurationStrategy.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace AspNetCore.Identity.FlexDb.Strategies
{
    using Azure.Identity;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;

    /// <summary>
    /// Configuration strategy for Azure Cosmos DB.
    /// </summary>
    public class CosmosDbConfigurationStrategy : IDatabaseConfigurationStrategy
    {
        /// <inheritdoc/>
        public string ProviderName => "Microsoft.EntityFrameworkCore.Cosmos";

        /// <inheritdoc/>
        public int Priority => 10;

        /// <inheritdoc/>
        public bool CanHandle(string connectionString)
        {
            return !string.IsNullOrWhiteSpace(connectionString) &&
                   connectionString.Contains("AccountEndpoint=", StringComparison.InvariantCultureIgnoreCase);
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

            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
            var databaseName = GetConnectionStringPart(parts, "Database=");
            var accountEndpoint = GetConnectionStringPart(parts, "AccountEndpoint=");
            var accountKey = GetConnectionStringPart(parts, "AccountKey=");

            ValidateRequiredParts(databaseName, accountEndpoint, accountKey);

            if (IsTokenAuthentication(accountKey))
            {
                optionsBuilder.UseCosmos(
                    accountEndpoint: accountEndpoint,
                    tokenCredential: new DefaultAzureCredential(),
                    databaseName: databaseName);
            }
            else
            {
                optionsBuilder.UseCosmos(
                    accountEndpoint: accountEndpoint,
                    accountKey: accountKey,
                    databaseName: databaseName);
            }
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

        private static bool IsTokenAuthentication(string accountKey)
        {
            return accountKey?.Equals("AccessToken", StringComparison.InvariantCultureIgnoreCase) ?? false;
        }

        private static void ValidateRequiredParts(string databaseName, string accountEndpoint, string accountKey)
        {
            if (string.IsNullOrWhiteSpace(databaseName) ||
                string.IsNullOrWhiteSpace(accountEndpoint) ||
                string.IsNullOrWhiteSpace(accountKey))
            {
                throw new ArgumentException(
                    "The provided Cosmos DB connection string is missing required components. " +
                    "Required: AccountEndpoint, AccountKey (or 'AccessToken' for managed identity), and Database.");
            }
        }
    }
}