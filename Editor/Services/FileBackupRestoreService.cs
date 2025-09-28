// <copyright file="Class.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.S3.Model;
    using Cosmos.BlobService;
    using Cosmos.BlobService.Models;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using System.Web;
    using System.IO;
    using Microsoft.AspNetCore.StaticFiles;

    /// <summary>
    ///  Backup and restore service for files upon application startup and shutdown.
    /// </summary>
    public class FileBackupRestoreService
    {
        private readonly string storageConnectionString;
        private readonly IMemoryCache memoryCache;
        private readonly FileExtensionContentTypeProvider contentTypeProvider = new FileExtensionContentTypeProvider();


        /// <summary>
        /// Initializes a new instance of the <see cref="FileBackupRestoreService"/> class.
        /// </summary>
        /// <param name="config">Configuration.</param>
        /// <param name="memoryCache">Memory cache.</param>
        public FileBackupRestoreService(IConfiguration config, IMemoryCache memoryCache)
        {
            storageConnectionString = config.GetConnectionString("BackupStorageConnectionString");
            this.memoryCache = memoryCache;
        }

        /// <summary>
        /// Downloads a blob from storage to a local file.
        /// </summary>
        /// <param name="sourcePath">Source path.</param>
        /// <param name="destinationPath">Destination path.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DownloadAsync(string sourcePath, string destinationPath)
        {
            var storageContext = CreateStorageContext();
            var blobExists = await storageContext.BlobExistsAsync(sourcePath);
            if (!blobExists)
            {
                Console.WriteLine($"Source blob {sourcePath} does not exist.");
                return;
            }

            using var stream = await storageContext.GetStreamAsync(sourcePath);
            using var fileStream = System.IO.File.Create(destinationPath);
            await stream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
        }

        /// <summary>
        /// Uploads a file to the storage.
        /// </summary>
        /// <param name="sourcePath">Source path.</param>
        /// <param name="destinationPath">Destination path.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UploadAsync(string sourcePath, string destinationPath)
        {
            var storageContext = CreateStorageContext();
            
            if (!File.Exists(sourcePath))
            {
                Console.WriteLine($"Source file {sourcePath} does not exist.");
                return;
            }

            // Get MIME type using built-in provider
            if (!contentTypeProvider.TryGetContentType(sourcePath, out var contentType))
            {
                contentType = "application/octet-stream"; // Default fallback
            }
            
            using var fileStream = File.OpenRead(sourcePath);
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            await storageContext.AppendBlob(memoryStream, new FileUploadMetaData
            {
                FileName = Path.GetFileName(destinationPath),
                ContentType = contentType
            });
        }

        private StorageContext CreateStorageContext()
        {
            var connectionString = storageConnectionString;
            return new StorageContext(connectionString, memoryCache);
        }
    }
}
