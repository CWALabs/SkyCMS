// <copyright file="StorageContextConfigUtilities.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.BlobStorage
{
    using Cosmos.BlobService;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Utility class for configuring storage contexts for testing across multiple providers.
    /// </summary>
    public class StorageContextConfigUtilities
    {
        /// <summary>
        /// Storage provider types supported by the test suite.
        /// </summary>
        public enum StorageProvider
        {
            /// <summary>Azure Blob Storage.</summary>
            Azure,

            /// <summary>Amazon S3.</summary>
            AmazonS3,

            /// <summary>Cloudflare R2 (S3-compatible).</summary>
            CloudflareR2
        }

        /// <summary>
        /// Gets a StorageContext for the default provider (from configuration).
        /// </summary>
        /// <returns>Configured StorageContext instance.</returns>
        internal static StorageContext GetStorageContext()
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables(); // ✅ ADD: Read environment variables (GitHub Actions)
            var configuration = builder.Build();

            return new StorageContext(configuration, GetMemoryCache(), GetServiceProvider());
        }

        /// <summary>
        /// Gets a StorageContext for a specific storage provider.
        /// </summary>
        /// <param name="provider">The storage provider to use.</param>
        /// <returns>Configured StorageContext instance.</returns>
        internal static StorageContext GetStorageContext(StorageProvider provider)
        {
            var connectionString = GetConnectionString(provider);

            if (string.IsNullOrEmpty(connectionString))
            {
                Assert.Inconclusive($"Connection string for {provider} not configured in user secrets or environment variables. Skipping test for this provider.");
                return null; // Never reached due to Assert.Inconclusive
            }

            return new StorageContext(connectionString, GetMemoryCache());
        }

        /// <summary>
        /// Gets the connection string for a specific storage provider from user secrets or environment variables.
        /// </summary>
        /// <param name="provider">The storage provider.</param>
        /// <returns>Connection string or null if not configured.</returns>
        private static string GetConnectionString(StorageProvider provider)
        {
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
            builder.AddEnvironmentVariables(); // ✅ ADD: Read environment variables (takes precedence over user secrets)
            var configuration = builder.Build();

            return provider switch
            {
                StorageProvider.Azure => configuration.GetConnectionString("AzureBlobStorageConnectionString")
                    ?? configuration.GetConnectionString("StorageConnectionString"),
                StorageProvider.AmazonS3 => configuration.GetConnectionString("AmazonS3ConnectionString"),
                StorageProvider.CloudflareR2 => configuration.GetConnectionString("CloudflareR2ConnectionString"),
                _ => null
            };
        }

        /// <summary>
        /// Gets all configured storage providers for testing.
        /// </summary>
        /// <returns>List of configured storage providers.</returns>
        internal static IEnumerable<StorageProvider> GetConfiguredProviders()
        {
            var providers = new List<StorageProvider>();

            if (!string.IsNullOrEmpty(GetConnectionString(StorageProvider.Azure)))
            {
                providers.Add(StorageProvider.Azure);
            }

            if (!string.IsNullOrEmpty(GetConnectionString(StorageProvider.AmazonS3)))
            {
                providers.Add(StorageProvider.AmazonS3);
            }

            if (!string.IsNullOrEmpty(GetConnectionString(StorageProvider.CloudflareR2)))
            {
                providers.Add(StorageProvider.CloudflareR2);
            }

            return providers;
        }

        /// <summary>
        /// Gets test data for DataTestMethod with all configured providers.
        /// Returns an array of object arrays, each containing a StorageProvider enum value.
        /// </summary>
        /// <returns>Enumerable of object arrays containing provider enum values.</returns>
        public static IEnumerable<object[]> GetTestProviders()
        {
            foreach (var provider in GetConfiguredProviders())
            {
                yield return new object[] { provider };
            }
        }

        /// <summary>
        /// Gets the service provider with required dependencies.
        /// </summary>
        /// <returns>Configured service provider.</returns>
        internal static IServiceProvider GetServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddLogging();
            services.AddSingleton(GetMemoryCache());
            return services.BuildServiceProvider();
        }

        /// <summary>
        /// Gets a configured memory cache instance.
        /// </summary>
        /// <returns>Memory cache instance.</returns>
        public static IMemoryCache GetMemoryCache()
        {
            var options = Options.Create(new MemoryCacheOptions()
            {
                SizeLimit = 20000000 // 20 megabytes decimal
            });
            return new MemoryCache(options);
        }
    }
}