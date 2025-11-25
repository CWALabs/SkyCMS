// <copyright file="EditorSettings.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.EditorSettings
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Cosmos.DynamicConfig;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Sky.Cms.Services;
    using Sky.Editor.Models;

    /// <summary>
    ///   Logic for managing settings in the application.
    /// </summary>
    public class EditorSettings : IEditorSettings
    {
        /// <summary>
        /// Editor settings group name.
        /// </summary>
        public static readonly string EDITORSETGROUPNAME = "EDITORSETTINGS";

        private readonly ApplicationDbContext dbContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMemoryCache memoryCache;
        private readonly IConfiguration configuration;
        private readonly bool isMultiTenantEditor;
        private EditorConfig editorConfig;
        private readonly string backupStorageConnectionString;
        private readonly IDynamicConfigurationProvider dynamicConfigurationProvider;
        private readonly SemaphoreSlim _configSemaphore = new SemaphoreSlim(1, 1);
        private readonly string domainName;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorSettings"/> class.
        /// </summary>
        /// <param name="configuration">Web app configuration.</param>
        /// <param name="dbContext">Database context.</param>
        /// <param name="httpContextAccessor">Http context accessor.</param>
        /// <param name="memoryCache">Memory cache.</param>
        /// <param name="serviceProvider">Service provider.</param>
        /// <param name="domainName">Optional domain name for multi-tenant operations.</param>
        public EditorSettings(IConfiguration configuration, ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache, IServiceProvider serviceProvider, string domainName = "")
        {
            this.dbContext = dbContext;
            this.httpContextAccessor = httpContextAccessor;
            this.memoryCache = memoryCache;
            this.configuration = configuration;
            backupStorageConnectionString = this.configuration.GetConnectionString("BackupStorageConnectionString") ?? null;
            this.domainName = domainName ?? string.Empty; // Always initialize
            isMultiTenantEditor = this.configuration.GetValue<bool?>("MultiTenantEditor") ?? false;
            if (isMultiTenantEditor)
            {
                dynamicConfigurationProvider = serviceProvider.GetRequiredService<IDynamicConfigurationProvider>();
            }

            // Configuration will be lazy-loaded on first access
        }

        /// <summary>
        /// Gets allowed file types for the file uploader.
        /// </summary>
        public string AllowedFileTypes
        {
            get
            {
                return ".js,.css,.htm,.html,.htm,.mov,.webm,.avi,.mp4,.mpeg,.ts,.svg,.json";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the website is allowed to perform setup tasks.
        /// </summary>
        public bool AllowSetup
        {
            get
            {
                EnsureConfigLoaded();
                return editorConfig.AllowSetup;
            }
        }

        /// <summary>
        /// Gets the static website URL.
        /// </summary>
        public string BlobPublicUrl
        {
            get
            {
                EnsureConfigLoaded();
                return editorConfig.BlobPublicUrl ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the backup storage connection string.
        /// </summary>
        public string BackupStorageConnectionString
        {
            get
            {
                return backupStorageConnectionString;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the publisher requires authentication.
        /// </summary>
        public bool CosmosRequiresAuthentication
        {
            get
            {
                EnsureConfigLoaded();
                return editorConfig.CosmosRequiresAuthentication;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the editor is a multi-tenant editor.
        /// </summary>
        public bool IsMultiTenantEditor
        {
            get
            {
                return isMultiTenantEditor;
            }
        }

        /// <summary>
        /// Gets the Microsoft application ID used for application verification.
        /// </summary>
        public string MicrosoftAppId
        {
            get
            {
                EnsureConfigLoaded();
                return editorConfig.MicrosoftAppId;
            }
        }

        /// <summary>
        /// Gets the publisher or website URL.
        /// </summary>
        public string PublisherUrl
        {
            get
            {
                EnsureConfigLoaded();
                return editorConfig.PublisherUrl;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the publisher is a static website.
        /// </summary>
        public bool StaticWebPages
        {
            get
            {
                EnsureConfigLoaded();
                return editorConfig.StaticWebPages;
            }
        }

        /// <summary>
        /// Gets the blob absolute URL.
        /// </summary>
        /// <returns>Uri.</returns>
        public Uri GetBlobAbsoluteUrl()
        {
            var htmlUtilities = new HtmlUtilities();

            if (htmlUtilities.IsAbsoluteUri(BlobPublicUrl))
            {
                return new Uri(BlobPublicUrl);
            }
            else
            {
                return new Uri(PublisherUrl.TrimEnd('/') + "/" + BlobPublicUrl.TrimStart('/'));
            }
        }

        /// <summary>
        /// Gets the editor configuration settings synchronously.
        /// </summary>
        /// <returns>Editor configuration.</returns>
        /// <remarks>
        /// This method uses GetAwaiter().GetResult() internally. For async contexts, use GetEditorConfigAsync instead.
        /// </remarks>
        public EditorConfig GetEditorConfig()
        {
            return GetEditorConfigAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the editor configuration settings asynchronously.
        /// </summary>
        /// <returns>Editor configuration.</returns>
        public async Task<EditorConfig> GetEditorConfigAsync()
        {
            // Check cache first with the correct domain name
            if (memoryCache.TryGetValue<EditorConfig>(GetNormalizedKeyName(domainName), out var cachedConfig))
            {
                return cachedConfig;
            }

            EditorConfig newConfig;

            if (!isMultiTenantEditor)
            {
                newConfig = new EditorConfig()
                {
                    AllowSetup = configuration.GetValue<bool?>("AllowSetup") ?? false,
                    IsMultiTenantEditor = isMultiTenantEditor,
                    BlobPublicUrl = configuration.GetValue<string>("AzureBlobStorageEndPoint") ?? "/",
                    CosmosRequiresAuthentication = configuration.GetValue<bool?>("CosmosRequiresAuthentication") ?? false,
                    MicrosoftAppId = configuration.GetValue<string>("MicrosoftAppId") ?? string.Empty,
                    PublisherUrl = configuration.GetValue<string>("CosmosPublisherUrl"),
                    StaticWebPages = configuration.GetValue<bool?>("CosmosStaticWebPages") ?? true,
                };
            }
            else
            {
                string scopedDomainName = domainName;
                if (string.IsNullOrWhiteSpace(domainName))
                {
                    if (httpContextAccessor.HttpContext == null)
                    {
                        throw new InvalidOperationException("No HttpContext available to determine tenant domain name, and the domain name not given.");
                    }

                    scopedDomainName = dynamicConfigurationProvider.GetTenantDomainNameFromRequest();
                }

                // Use await instead of .Result to avoid potential deadlocks
                var connection = await dynamicConfigurationProvider.GetTenantConnectionAsync(scopedDomainName);

                if (connection == null)
                {
                    throw new InvalidOperationException($"No tenant connection found for domain: {scopedDomainName}");
                }

                newConfig = new EditorConfig()
                {
                    AllowSetup = connection.AllowSetup,
                    IsMultiTenantEditor = true,
                    BlobPublicUrl = connection.BlobPublicUrl,
                    CosmosRequiresAuthentication = connection.PublisherRequiresAuthentication,
                    MicrosoftAppId = connection.MicrosoftAppId,
                    PublisherUrl = connection.WebsiteUrl,
                    StaticWebPages = connection.PublisherMode == "Static"
                };
            }

            memoryCache.Set(GetNormalizedKeyName(domainName), newConfig, TimeSpan.FromMinutes(5));

            return newConfig;
        }

        /// <summary>
        /// Ensures the editor configuration is loaded before accessing it (synchronous version).
        /// </summary>
        private void EnsureConfigLoaded()
        {
            if (editorConfig == null)
            {
                _configSemaphore.Wait();
                try
                {
                    if (editorConfig == null)
                    {
                        editorConfig = GetEditorConfig();
                    }
                }
                finally
                {
                    _configSemaphore.Release();
                }
            }
        }

        /// <summary>
        /// Ensures the editor configuration is loaded before accessing it (asynchronous version).
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task EnsureConfigLoadedAsync()
        {
            if (editorConfig == null)
            {
                await _configSemaphore.WaitAsync();
                try
                {
                    if (editorConfig == null)
                    {
                        editorConfig = await GetEditorConfigAsync();
                    }
                }
                finally
                {
                    _configSemaphore.Release();
                }
            }
        }

        private string GetNormalizedKeyName(string domainName)
        {
            return $"edsetting-{domainName.ToLower()}";
        }
    }
}
