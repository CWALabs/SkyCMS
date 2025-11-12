// <copyright file="Class.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Services
{
    using Cosmos.BlobService;
    using Cosmos.BlobService.Models;
    using Microsoft.AspNetCore.StaticFiles;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.IO;
    using System.Threading.Tasks;

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
        /// Initializes a new instance of the <see cref="FileBackupRestoreService"/> class.
        /// </summary>
        /// <param name="storageConnectionString">Configuration.</param>
        /// <param name="memoryCache">Memory cache.</param>
        public FileBackupRestoreService(string storageConnectionString, IMemoryCache memoryCache)
        {
            this.storageConnectionString = storageConnectionString;
            this.memoryCache = memoryCache;
        }

        /// <summary>
        /// Downloads a blob from storage to a local file.
        /// </summary>
        /// <param name="sqLiteConnectionString">Connection string of the database to download.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task DownloadAsync(string sqLiteConnectionString)
        {
            var storageContext = CreateStorageContext();
            var properties = new SQliteDbBackupProperties(sqLiteConnectionString);

            var result = await DownloadFile(properties.FileName, properties.LocalPath, storageContext);

            if (result)
            {
                // Try to download the -shm and -wal files as well, but ignore if they don't exist.
                await DownloadFile(properties.FileName + "-shm", properties.LocalPath + "-shm", storageContext);
                await DownloadFile(properties.FileName + "-wal", properties.LocalPath + "-wal", storageContext);
            }
        }

        /// <summary>
        /// Uploads a file to the storage.
        /// </summary>
        /// <param name="sqLiteConnectionString">Connection string for the SQLite database to back up.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task UploadAsync(string sqLiteConnectionString)
        {
            var storageContext = CreateStorageContext();

            var properties = new SQliteDbBackupProperties(sqLiteConnectionString);

            await UploadFile(properties.LocalPath, storageContext);
            await UploadFile(properties.LocalPath + "-shm", storageContext);
            await UploadFile(properties.LocalPath + "-wal", storageContext);
        }

        private async Task<bool> DownloadFile(string sourceFile, string destination, StorageContext storageContext)
        {
            var fileName = Path.GetFileName(destination);
            if (File.Exists(destination))
            {
                Console.WriteLine($"Destination file {destination} already exists.");
                return false;
            }

            var blobExists = await storageContext.BlobExistsAsync(fileName);
            if (!blobExists)
            {
                Console.WriteLine($"Source blob {fileName} does not exist.");
                return false;
            }

            using var stream = await storageContext.GetStreamAsync(fileName);
            using var fileStream = File.Create(destination);
            await stream.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
            return true;
        }

        private async Task UploadFile(string filePath, StorageContext storageContext)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"Source file {filePath} does not exist.");
                return;
            }

            var fileName = Path.GetFileName(filePath);

            // Get MIME type using built-in provider
            if (!contentTypeProvider.TryGetContentType(fileName, out var contentType))
            {
                contentType = "application/octet-stream"; // Default fallback
            }

            using var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                await fileStream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;
            await storageContext.AppendBlob(memoryStream, new FileUploadMetaData
            {
                RelativePath = fileName,
                FileName = Path.GetFileName(fileName),
                ContentType = contentType,
                TotalFileSize = memoryStream.Length,
                ChunkIndex = 0,
                TotalChunks = 1,
                UploadUid = Guid.NewGuid().ToString()
            });
        }

        private StorageContext CreateStorageContext()
        {
            var connectionString = storageConnectionString;
            return new StorageContext(connectionString, memoryCache);
        }

        /// <summary>
        /// Gets the backup properties from the database connection string.
        /// </summary>
        public class SQliteDbBackupProperties
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SQliteDbBackupProperties"/> class.
            /// </summary>
            /// <param name="sqLiteConnectionString">SQLite DB connection string.</param>
            public SQliteDbBackupProperties(string sqLiteConnectionString)
            {
                var parts = sqLiteConnectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                var dataSourcePart = Array.Find(parts, p => p.StartsWith("Data Source=", StringComparison.InvariantCultureIgnoreCase));
                LocalPath = dataSourcePart.Split('=')[1];
                FileName = Path.GetFileName(LocalPath);
            }

            /// <summary>
            /// Gets the local path where database is stored.
            /// </summary>
            public string LocalPath { get; private set; }

            /// <summary>
            /// Gets the file name.
            /// </summary>
            public string FileName { get; private set; }
        }
    }
}
