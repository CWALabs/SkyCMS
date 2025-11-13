// <copyright file="ConnectionStringProvider.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Cosmos.DynamicConfig
{
    /// <summary>
    /// Gets connection strings and configuration values from the configuration file.
    /// </summary>
    /// <remarks>
    /// If in a multi-tenant environment, the connection string names are prefixed by the domain name.
    /// </remarks>
    public class DynamicConfigurationProvider : IDynamicConfigurationProvider
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMemoryCache memoryCache;
        private readonly StringBuilder errorMessages = new();
        private readonly string connectionString;
        private readonly ILogger<DynamicConfigurationProvider> _logger;
        private const string CacheKeyPrefix = "tenant:connection:";

        /// <summary>
        /// Gets the database connection
        /// </summary>
        //private readonly Connection? connection;

        /// <summary>
        /// Gets a value indicating whether the connection is configured for multi-tenant.
        /// </summary>
        public bool IsMultiTenantConfigured { get { return GetTenantConnectionAsync("").GetAwaiter().GetResult() != null; } }

        /// <summary>
        /// Gets a value indicating the error messages that may exist.
        /// </summary>
        public string ErrorMesages => errorMessages.ToString();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionStringProvider"/> class.
        /// </summary>
        /// <param name="configuration">Connection configuration.</param>
        /// <param name="httpContextAccessor">HTTP context accessor.</param>
        /// <param name="logger">Log service.</param>
        /// <param name="memoryCache">Memory cache.</param>
        public DynamicConfigurationProvider(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache,
            ILogger<DynamicConfigurationProvider> logger)
        {
            this.configuration = configuration;
            this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            
            connectionString = this.configuration.GetConnectionString("ConfigDbConnectionString") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string 'ConfigDbConnectionString' not found or is empty.");
            }
            this.memoryCache = memoryCache;
            _logger = logger;
        }

        /// <summary>
        /// Gets the database connection string.
        /// /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <returns>Database connection string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when HttpContext is unavailable and no domain is provided.</exception>
        public string? GetDatabaseConnectionString(string domainName = "")
        {
            if (httpContextAccessor.HttpContext == null)
            {
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    _logger?.LogError("Cannot resolve tenant connection: HttpContext unavailable and no domain provided");
                    throw new InvalidOperationException(
                        "Cannot resolve tenant connection: HttpContext unavailable and no domain provided. " +
                        "For background jobs or operations outside HTTP context, you must explicitly provide the domain name.");
                }

                _logger?.LogWarning("HttpContext not available - using provided domain: {Domain}", domainName);
            }

            if (string.IsNullOrWhiteSpace(domainName))
            {
                domainName = GetTenantDomainNameFromRequest();
            }
            
            // Normalize domain name
            domainName = NormalizeDomainName(domainName);
            
            var connection = GetTenantConnectionAsync(domainName).GetAwaiter().GetResult();
            
            if (connection == null)
            {
                _logger?.LogWarning("No connection found for domain: {Domain}", domainName);
                return null;
            }
            
            return connection.DbConn;
        }

        /// <summary>
        /// Gets the storage connection string.
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <returns>Database connection string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when HttpContext is unavailable and no domain is provided.</exception>
        public string? GetStorageConnectionString(string domainName = "")
        {
            if (httpContextAccessor.HttpContext == null)
            {
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    _logger?.LogError("Cannot resolve tenant storage connection: HttpContext unavailable and no domain provided");
                    throw new InvalidOperationException(
                        "Cannot resolve tenant storage connection: HttpContext unavailable and no domain provided. " +
                        "For background jobs or operations outside HTTP context, you must explicitly provide the domain name.");
                }

                _logger?.LogWarning("HttpContext not available for storage connection - using provided domain: {Domain}", domainName);
            }

            if (string.IsNullOrWhiteSpace(domainName))
            {
                domainName = GetTenantDomainNameFromRequest();
            }
            
            // Normalize domain name
            domainName = NormalizeDomainName(domainName);
            
            var connection = GetTenantConnectionAsync(domainName).GetAwaiter().GetResult();
            
            if (connection == null)
            {
                _logger?.LogWarning("No storage connection found for domain: {Domain}", domainName);
                return null;
            }
            
            return connection.StorageConn;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="key">Key name.</param>
        /// <returns>Key value.</returns>
        public string? GetConfigurationValue(string key)
        {
            return configuration.GetValue<string>(key);
        }

        /// <summary>
        /// Gets the connection string by its name.
        /// </summary>
        /// <param name="name">Connection string name.</param>
        /// <returns>Database connection string.</returns>
        public string? GetConnectionStringByName(string name)
        {
            return configuration.GetConnectionString(name);
        }

        /// <summary>
        /// Gets the tenant website domain name from the request.
        /// </summary>
        /// <param name="useReferer">Get the domain name from the referer instead of the domain name of website.</param>
        /// <returns>Domain Name.</returns>
        /// <remarks>
        /// <para>Returns the domain name by looking at the incomming request.  Here is the order:</para>
        /// <list type="number">
        /// <item>Query string value 'website'. Sets the standard cookie if it exists.</item>
        /// <item>Referer request header value if requested.</item>
        /// <item>Otherwise returns the host name of the request.</item>
        /// </list>
        /// <para>Note: This should ONLY be used for multi-tenant, single editor website setup.</para>
        /// </remarks>
        public string GetTenantDomainNameFromRequest(bool useReferer = false)
        {
            if (httpContextAccessor.HttpContext == null)
            {
                _logger?.LogWarning("HttpContext is null when attempting to get tenant domain name from request");
                return string.Empty;
            }

            if (useReferer)
            {
                var referer = httpContextAccessor.HttpContext.Request.Headers.Referer.ToString();
                if (!string.IsNullOrWhiteSpace(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
                {
                    var domain = refererUri.Host.ToLowerInvariant();
                    return domain;
                }
            }
            
            if (httpContextAccessor.HttpContext.Request == null)
            {
                throw new InvalidOperationException("HTTP request is not available.");
            }
            
            var hostDomain = httpContextAccessor.HttpContext.Request.Host.Host.ToLowerInvariant();
            return hostDomain;
        }

        /// <summary>
        /// Handles possibility that a user entered a URI instead of a domain name, and returns just the host name.
        /// </summary>
        /// <param name="value">URI or domain value.</param>
        /// <returns>Host name only.</returns>
        public static string CleanUpDomainName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (Uri.TryCreate(value, UriKind.Absolute, out var referrerUri))
            {
                return referrerUri.Host.ToLowerInvariant();
            }

            return value.ToLowerInvariant();
        }

        /// <summary>
        /// Tests to see if there is a connection defined for the specified domain name.
        /// </summary>
        /// <param name="domainName">Domain name to validate.</param>
        /// <returns>Domain is valid (true) or not (false).</returns>
        /// <exception cref="ArgumentException">Thrown when ConfigDbConnectionString is not configured.</exception>
        public async Task<bool> ValidateDomainName(string domainName)
        {
            if (string.IsNullOrWhiteSpace(domainName))
            {
                _logger?.LogWarning("ValidateDomainName called with null or empty domain name");
                return false;
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string 'ConfigDbConnectionString' not found.");
            }
            
            // Normalize domain name for consistency
            domainName = NormalizeDomainName(domainName);
                        
            using var dbContext = GetDbContext();
            var allConnections = await dbContext.Connections.ToListAsync();
            var result = allConnections.FirstOrDefault(c => c.DomainNames != null && c.DomainNames.Contains(domainName, StringComparer.OrdinalIgnoreCase));
            
            var isValid = result != null;
            
            if (!isValid)
            {
                _logger?.LogWarning("Domain validation failed for: {Domain}", domainName);
            }
            
            return isValid;
        }

        /// <summary>
        /// Normalizes a domain name to lowercase for consistent comparison and caching.
        /// </summary>
        /// <param name="domainName">Domain name to normalize.</param>
        /// <returns>Normalized domain name.</returns>
        private static string NormalizeDomainName(string domainName)
        {
            if (string.IsNullOrWhiteSpace(domainName))
            {
                return domainName;
            }
            
            return domainName.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Gets the cache key for a domain name with proper namespacing.
        /// </summary>
        /// <param name="domainName">Domain name.</param>
        /// <returns>Cache key.</returns>
        private static string GetCacheKey(string domainName)
        {
            return $"{CacheKeyPrefix}{NormalizeDomainName(domainName)}";
        }

        private DynamicConfigDbContext GetDbContext()
        {
            var options = AspNetCore.Identity.FlexDb.CosmosDbOptionsBuilder.GetDbOptions<DynamicConfigDbContext>(this.connectionString);
            return new DynamicConfigDbContext(options);
        }

        private async Task<Connection?> GetTenantConnectionAsync(string domainName)
        {
            if (string.IsNullOrWhiteSpace(domainName))
            {
                _logger?.LogDebug("GetTenantConnection called with null or empty domain name");
                return null;
            }

            // Normalize domain name
            domainName = NormalizeDomainName(domainName);
            
            // Use namespaced cache key to prevent cache poisoning
            var cacheKey = GetCacheKey(domainName);
            
            if (memoryCache.TryGetValue<Connection>(cacheKey, out var connection))
            {
                _logger?.LogDebug("Cache hit for domain: {Domain}, ConnectionId: {ConnectionId}", domainName, connection.Id);
                
                // Validate cached connection still has this domain (prevents stale cache issues)
                if (connection.DomainNames != null && connection.DomainNames.Contains(domainName, StringComparer.OrdinalIgnoreCase))
                {
                    return connection;
                }
                
                _logger?.LogWarning("Cached connection for domain {Domain} no longer contains this domain - removing from cache", domainName);
                memoryCache.Remove(cacheKey);
            }

            _logger?.LogDebug("Cache miss for domain: {Domain}, querying database", domainName);
            
            using var dbContext = GetDbContext();

            try
            {
                // Load all connections asynchronously, then filter client-side
                // Cosmos DB doesn't support complex LINQ queries with .Any() inside Where clauses
                var allConnections = await dbContext.Connections.ToListAsync();
                connection = allConnections.FirstOrDefault(c => c.DomainNames != null && 
                                                               c.DomainNames.Contains(domainName, StringComparer.OrdinalIgnoreCase));

                if (connection != null)
                {
                    _logger?.LogInformation("Found connection for domain: {Domain}, ConnectionId: {ConnectionId}, caching for 10 seconds", 
                        domainName, connection.Id);
                    
                    // Cache with absolute expiration to ensure fresh data
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(10))
                        .SetPriority(CacheItemPriority.High);
                    
                    memoryCache.Set(cacheKey, connection, cacheOptions);
                }
                else
                {
                    _logger?.LogWarning("No connection found in database for domain: {Domain}", domainName);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving connection for domain: {Domain}", domainName);
                return null;
            }

            return connection;
        }
    }
}
