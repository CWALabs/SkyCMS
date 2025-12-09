// <copyright file="PubControllerBase.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
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
                var cacheKey = $"file_{HttpContext.Request.Path}";

                // Try to get from cache first
                if (!memoryCache.TryGetValue(cacheKey, out (byte[] data, string contentType, DateTimeOffset? lastModified, string etag) cachedFile))
                {
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

                    cachedFile = (fileData, contentType, properties.ModifiedUtc, properties.ETag);

                    // Cache for 5 minutes for authenticated, 1 hour for public
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                        .SetSize(fileData.Length);

                    memoryCache.Set(cacheKey, cachedFile, cacheOptions);

                    logger.LogDebug("Cached file {Path} ({Size} bytes) with content type {ContentType}", path, fileData.Length, cachedFile.contentType);
                }
                else
                {
                    logger.LogDebug("Serving cached file {Path} ({Size} bytes) with content type {ContentType}", path, cachedFile.data.Length, cachedFile.contentType);
                }

                var etag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue($"\"{cachedFile.etag}\"");

                return File(
                    fileContents: cachedFile.data,
                    contentType: cachedFile.contentType,
                    lastModified: cachedFile.lastModified,
                    entityTag: etag);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error serving file {Path}", path);
                return NotFound();
            }
        }
    }
}
