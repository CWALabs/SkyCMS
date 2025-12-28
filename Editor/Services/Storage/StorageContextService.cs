//// <copyright file="StorageConfigurationProvider.cs" company="Moonrise Software, LLC">
//// Copyright (c) Moonrise Software, LLC. All rights reserved.
//// Licensed under the MIT License (https://opensource.org/licenses/MIT)
//// See https://github.com/CWALabs/SkyCMS
//// for more information concerning the license and the contributors participating to this project.
//// </copyright>

//namespace Sky.Editor.Services.Storage
//{
//    using System;
//    using System.Collections.Generic;
//    using System.IO;
//    using System.Linq;
//    using System.Threading.Tasks;
//    using Cosmos.BlobService;
//    using Cosmos.BlobService.Models;
//    using Cosmos.Common.Data;
//    using Cosmos.DynamicConfig;
//    using Microsoft.EntityFrameworkCore;
//    using Microsoft.Extensions.Caching.Memory;
//    using Microsoft.Extensions.Configuration;
//    using Microsoft.Extensions.DependencyInjection;

//    /// <summary>
//    /// Handles retrieval of storage context.
//    /// </summary>
//    public class StorageContextService : IStorageContext
//    {
//        private readonly IConfiguration configuration;
//        private readonly ApplicationDbContext dbContext;
//        private readonly IServiceProvider serviceProvider;
//        private readonly bool isMultiTenant;
//        private readonly IDynamicConfigurationProvider dynamicConfigurationProvider;
//        private readonly IMemoryCache memoryCache;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="StorageContextService"/> class.
//        /// </summary>
//        /// <param name="configuration">Application configuration.</param>
//        /// <param name="dbContext">Database context.</param>
//        /// <param name="serviceProvider">Service provider.</param>
//        /// <param name="memoryCache">Memory cache.</param>
//        public StorageContextService(IConfiguration configuration, ApplicationDbContext dbContext, IServiceProvider serviceProvider, IMemoryCache memoryCache)
//        {
//            this.configuration = configuration;
//            this.dbContext = dbContext;
//            this.serviceProvider = serviceProvider;
//            this.isMultiTenant = configuration.GetValue<bool?>("MultiTenant") ?? configuration.GetValue<bool?>("MultiTenantEditor") ?? false;
//            if (this.isMultiTenant)
//            {
//                // Ensure dynamic configuration provider is available
//                this.dynamicConfigurationProvider = serviceProvider.GetService<IDynamicConfigurationProvider>();
//                if (dynamicConfigurationProvider == null)
//                {
//                    throw new InvalidOperationException("Multi-tenant storage requires IDynamicConfigurationProvider to be registered.");
//                }
//            }
//        }

//        /// <inheritdoc/>
//        public async Task AppendBlob(MemoryStream stream, FileUploadMetaData fileMetaData, string mode = "append")
//        {
//            await GetStorageContext().AppendBlob(stream, fileMetaData, mode);
//        }

//        /// <inheritdoc/>
//        public async Task<bool> BlobExistsAsync(string path)
//        {
//            return await GetStorageContext().BlobExistsAsync(path);
//        }

//        /// <inheritdoc/>
//        public async Task CopyAsync(string target, string destination)
//        {
//            await GetStorageContext().CopyAsync(target, destination);
//        }

//        /// <inheritdoc/>
//        public async Task<FileManagerEntry> CreateFolder(string path)
//        {
//            return await GetStorageContext().CreateFolder(path);
//        }

//        /// <inheritdoc/>
//        public void DeleteFile(string path)
//        {
//            GetStorageContext().DeleteFile(path);
//        }

//        /// <inheritdoc/>
//        public async Task DeleteFolderAsync(string path)
//        {
//            await GetStorageContext().DeleteFolderAsync(path);
//        }

//        /// <inheritdoc/>
//        public async Task DisableAzureStaticWebsite()
//        {
//            await GetStorageContext().DisableAzureStaticWebsite();
//        }

//        /// <inheritdoc/>
//        public async Task EnableAzureStaticWebsite()
//        {
//            await GetStorageContext().EnableAzureStaticWebsite();
//        }

//        /// <inheritdoc/>
//        public async Task<FileManagerEntry> GetFileAsync(string path)
//        {
//            return await GetStorageContext().GetFileAsync(path);
//        }

//        /// <inheritdoc/>
//        public async Task<List<FileManagerEntry>> GetFilesAndDirectories(string path)
//        {
//            return await GetStorageContext().GetFilesAndDirectories(path);
//        }

//        /// <inheritdoc/>
//        public async Task<List<string>> GetFilesAsync(string path)
//        {
//            return await GetStorageContext().GetFilesAsync(path);
//        }

//        /// <inheritdoc/>
//        public async Task<Stream> GetStreamAsync(string path)
//        {
//            return await GetStorageContext().GetStreamAsync(path);
//        }

//        /// <inheritdoc/>
//        public async Task MoveFileAsync(string sourceFile, string destinationFile)
//        {
//            await GetStorageContext().MoveFileAsync(sourceFile, destinationFile);
//        }

//        /// <inheritdoc/>
//        public async Task MoveFolderAsync(string sourceFolder, string destinationFolder)
//        {
//            await GetStorageContext().MoveFolderAsync(sourceFolder, destinationFolder);
//        }

//        /// <summary>
//        /// Gets the storage context based on the resolved connection string.
//        /// </summary>
//        /// <returns>StorageContext.</returns>
//        private StorageContext GetStorageContext()
//        {
//            var connectionString = string.Empty;

//            // ✅ Priority 1: Multi-tenant: resolve driver per request using dynamic configuration provider.
//            if (isMultiTenant)
//            {
//                connectionString = GetMultiTenantConnectionstring().GetAwaiter().GetResult();
//            }

//            // ✅ Priority 2: Single-tenant: resolve driver using static configuration.
//            connectionString = configuration.GetConnectionString("StorageConnectionString")
//            ?? configuration.GetConnectionString("AzureBlobStorageConnectionString");

//            // ✅ Priority 3: Check database Settings table for connection string.
//            if (string.IsNullOrEmpty(connectionString))
//            {
//                // Check database Settings table
//                connectionString = GetStringFromDatabase().GetAwaiter().GetResult();
//            }

//            return GetDriverFromConnectionString(connectionString);
//        }

//        private async Task<string> GetMultiTenantConnectionstring()
//        {
//            var dynamicConfigurationProvider = serviceProvider.GetRequiredService<IDynamicConfigurationProvider>();
//            var connectionString = await dynamicConfigurationProvider.GetStorageConnectionStringAsync();

//            return connectionString;
//        }

//        private async Task<string> GetStringFromDatabase()
//        {
//            // Look for storage connection string in Settings table
//            var storageSetting = await dbContext.Settings
//                .Where(s => s.Group == "STORAGE" &&
//                           (s.Name == "StorageConnectionString" || s.Name == "AzureBlobStorageConnectionString"))
//                .FirstOrDefaultAsync();

//            return storageSetting?.Value;
//        }

//        private StorageContext GetDriverFromConnectionString(string connectionString)
//        {
//            // For this example, we assume Azure Blob Storage is used.
//            // In a real implementation, you might want to parse the connection string
//            // to determine the storage type and instantiate the appropriate driver.
//            return new StorageContext(connectionString, this.memoryCache);
//        }
//    }
//}