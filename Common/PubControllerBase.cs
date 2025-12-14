// <copyright file="PubControllerBase.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Publisher.Controllers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Cosmos.BlobService;
    using Cosmos.Common;
    using Cosmos.Common.Data;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Secure file access controller and proxy.
    /// </summary>
    public class PubControllerBase : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly StorageContext storageContext;
        private readonly bool requiresAuthentication;
        private readonly ILogger<PubControllerBase> logger;
        private readonly IMemoryCache memoryCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="PubControllerBase"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="dbContext">Database context.</param>
        /// <param name="storageContext">Storage context.</param>
        /// <param name="requiresAuthentication">Indicates if authentication is required for the publisher.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="memoryCache">Memory cache instance.</param>
        public PubControllerBase(
            ApplicationDbContext dbContext,
            StorageContext storageContext,
            bool requiresAuthentication,
            ILogger<PubControllerBase> logger,
            IMemoryCache memoryCache)
        {
            this.requiresAuthentication = requiresAuthentication;
            this.dbContext = dbContext;
            this.storageContext = storageContext;
            this.logger = logger;
            this.memoryCache = memoryCache;
        }

        /// <summary>
        /// Gets a file and validates user authentication.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task<IActionResult> Index()
        {
            var path = HttpContext.Request.Path.ToString();

            if (requiresAuthentication)
            {
                // If the user is not logged in, have them login first.
                if (User.Identity == null || !User.Identity.IsAuthenticated)
                {
                    logger.LogWarning("Unauthorized access attempt to {Path} - User not authenticated", path);
                    return Unauthorized();
                }

                // See if the article is in protected storage.
                if (path.StartsWith("/pub/articles/", StringComparison.OrdinalIgnoreCase))
                {
                    var pathParts = path.TrimStart('/').Split('/');
                    if (pathParts.Length > 2 && int.TryParse(pathParts[2], out var articleNumber))
                    {
                        // Check for user authorization.
                        if (!await CosmosUtilities.AuthUser(dbContext, User, articleNumber))
                        {
                            logger.LogWarning(
                                "Unauthorized access attempt to {Path} - User {UserName} not authorized for article {ArticleNumber}",
                                path,
                                User.Identity.Name,
                                articleNumber);
                            return Unauthorized();
                        }
                    }
                }

                Response.Headers.CacheControl = "private, no-cache, no-store, must-revalidate";
                Response.Headers.Expires = DateTimeOffset.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss 'GMT'");
            }
            else
            {
                // Public files could be cached
                Response.Headers.CacheControl = "public, max-age=3600";
            }

            try
            {
                var cacheKey = $"{HttpContext.Request.Host.Host}-{HttpContext.Request.Path}";

                if (memoryCache.TryGetValue(cacheKey, out CachedFile cachedFile))
                {
                    return File(
                        fileContents: cachedFile.Data,
                        contentType: cachedFile.Metadata.ContentType,
                        lastModified: cachedFile.Metadata.ModifiedUtc,
                        entityTag: cachedFile.ETag);
                }

                // Try to get from cache first
                var properties = await storageContext.GetFileAsync(HttpContext.Request.Path);

                if (properties == null)
                {
                    logger.LogWarning("File not found: {Path}", path);
                    return NotFound();
                }

                var fileStream = await storageContext.GetStreamAsync(HttpContext.Request.Path);
                var contentType = properties.ContentType ?? Utilities.GetContentType(properties.Name);

                // Read to byte array for caching
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                cachedFile = new CachedFile()
                {
                    Data = fileData,
                    Metadata = properties,
                    ETag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue(properties.ETag)
                };

                memoryCache.CreateEntry(cacheKey)
                    .SetValue(cachedFile)
                    .SetSlidingExpiration(TimeSpan.FromMinutes(4));

                return File(
                    fileContents: cachedFile.Data,
                    contentType: cachedFile.Metadata.ContentType,
                    lastModified: cachedFile.Metadata.ModifiedUtc,
                    entityTag: cachedFile.ETag);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error serving file {Path}", path);
                return NotFound();
            }
        }

        private class CachedFile
        {
            public byte[] Data { get; set; }

            public FileManagerEntry Metadata { get; set; }

            public Microsoft.Net.Http.Headers.EntityTagHeaderValue ETag { get; set; }
        }
    }
}
