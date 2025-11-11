// <copyright file="MySqlConfigurationStrategy.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace AspNetCore.Identity.FlexDb.Strategies
{
    using System;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Configuration strategy for MySQL.
    /// </summary>
    public class MySqlConfigurationStrategy : IDatabaseConfigurationStrategy
    {
        /// <inheritdoc/>
        public string ProviderName => "MySql.EntityFrameworkCore";

        /// <inheritdoc/>
        public int Priority => 30;

        /// <inheritdoc/>
        public bool CanHandle(string connectionString)
        {
            return !string.IsNullOrWhiteSpace(connectionString) &&
                   connectionString.Contains("uid=", StringComparison.InvariantCultureIgnoreCase);
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

            var serverVersion = ServerVersion.AutoDetectAsync(connectionString).GetAwaiter().GetResult();
            optionsBuilder.UseMySql(
                connectionString,
                serverVersion,
                options => options.EnableRetryOnFailure());
        }
    }
}