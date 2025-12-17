// <copyright file="StorageConfigurationProvider.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Storage
{
    using Cosmos.BlobService;
    using Cosmos.Common.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Sky.Editor.Data;
    using System.Linq;

    /// <summary>
    /// Provides storage connection string with priority: Environment Variables > Database > null.
    /// </summary>
    public class StorageConfigurationProvider : IStorageConfigurationProvider
    {
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageConfigurationProvider"/> class.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        /// <param name="dbContext">Database context.</param>
        public StorageConfigurationProvider(IConfiguration configuration, ApplicationDbContext dbContext)
        {
            this.configuration = configuration;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public string GetStorageConnectionString()
        {
            // ✅ Priority 1: Check environment variables/appsettings/user secrets
            var connectionString = configuration.GetConnectionString("StorageConnectionString")
                ?? configuration.GetConnectionString("AzureBlobStorageConnectionString");

            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }

            // ✅ Priority 2: Check database Settings table
            try
            {
                // Check if database is accessible
                if (!dbContext.Database.CanConnectAsync().GetAwaiter().GetResult())
                {
                    return null;
                }

                // Look for storage connection string in Settings table
                var storageSetting = dbContext.Settings
                    .Where(s => s.Group == "STORAGE" &&
                               (s.Name == "StorageConnectionString" || s.Name == "AzureBlobStorageConnectionString"))
                    .FirstOrDefault();

                return storageSetting?.Value;
            }
            catch
            {
                // Database might not be available yet (during initial setup)
                return null;
            }
        }
    }
}