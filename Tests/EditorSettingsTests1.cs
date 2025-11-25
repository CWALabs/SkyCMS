// <copyright file="EditorSettingsTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Settings
{
    using Cosmos.Common.Data;
    using Cosmos.DynamicConfig;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sky.Editor.Services.EditorSettings;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Unit tests for the <see cref="EditorSettings"/> class.
    /// Tests single-tenant and multi-tenant configuration scenarios.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class EditorSettingsTests
    {
        private ApplicationDbContext _db;
        private IMemoryCache _cache;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private IConfiguration _configuration;
        private IServiceProvider _serviceProvider;
        private const string TestPublisherUrl = "https://example.com";
        private const string TestBlobUrl = "https://blob.example.com";

        /// <summary>
        /// Initializes test dependencies before each test.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            // In-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            // Memory cache - create a fresh instance for each test
            _cache = new MemoryCache(new MemoryCacheOptions());

            // Mock HTTP context
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var mockContext = new DefaultHttpContext();
            mockContext.Request.Host = new HostString("example.com");
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockContext);

            // Service provider (initially without DynamicConfigurationProvider)
            _serviceProvider = new ServiceCollection().BuildServiceProvider();
        }

        /// <summary>
        /// Cleanup after each test.
        /// </summary>
        [TestCleanup]
        public async Task Cleanup()
        {
            await _db.DisposeAsync();
            _cache.Dispose();
        }

        #region Single-Tenant Configuration Tests

        /// <summary>
        /// Tests that EditorSettings properly loads single-tenant configuration from appsettings.
        /// </summary>
        [TestMethod]
        public void SingleTenant_LoadsConfigurationFromAppSettings()
        {
            // Arrange
            _configuration = BuildConfiguration(multiTenant: false);

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.AreEqual(TestPublisherUrl, settings.PublisherUrl);
            Assert.AreEqual(TestBlobUrl, settings.BlobPublicUrl);
            Assert.IsTrue(settings.StaticWebPages);
            Assert.IsFalse(settings.CosmosRequiresAuthentication);
            Assert.IsFalse(settings.IsMultiTenantEditor);
        }

        /// <summary>
        /// Tests that EditorSettings returns correct allowed file types.
        /// </summary>
        [TestMethod]
        public void SingleTenant_AllowedFileTypes_ReturnsCorrectTypes()
        {
            // Arrange
            _configuration = BuildConfiguration(multiTenant: false);
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Act
            var fileTypes = settings.AllowedFileTypes;

            // Assert
            Assert.IsNotNull(fileTypes);
            StringAssert.Contains(fileTypes, ".js");
            StringAssert.Contains(fileTypes, ".css");
            StringAssert.Contains(fileTypes, ".json");
            StringAssert.Contains(fileTypes, ".svg");
        }
                
        /// <summary>
        /// Tests that EditorSettings defaults BlobPublicUrl to "/" when not specified.
        /// </summary>
        [TestMethod]
        public void SingleTenant_DefaultsBlobUrlToSlash_WhenNotSpecified()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["MultiTenantEditor"] = "false",
                ["CosmosPublisherUrl"] = TestPublisherUrl,
                ["CosmosStaticWebPages"] = "true"
                // AzureBlobStorageEndPoint is intentionally missing
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.AreEqual("/", settings.BlobPublicUrl);
        }

        /// <summary>
        /// Tests that EditorSettings caches configuration by host.
        /// </summary>
        [TestMethod]
        public void SingleTenant_CachesConfiguration_ByHost()
        {
            // Arrange
            _configuration = BuildConfiguration(multiTenant: false);

            // Act - First call
            var settings1 = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);
            var url1 = settings1.PublisherUrl;

            // Act - Second call (should use cache)
            var settings2 = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);
            var url2 = settings2.PublisherUrl;

            // Assert
            Assert.AreEqual(url1, url2);
            Assert.AreEqual(TestPublisherUrl, url2);
        }

        /// <summary>
        /// Tests GetBlobAbsoluteUrl with absolute blob URL.
        /// </summary>
        [TestMethod]
        public void SingleTenant_GetBlobAbsoluteUrl_WithAbsoluteUrl()
        {
            // Arrange
            _configuration = BuildConfiguration(multiTenant: false);
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Act
            var absoluteUrl = settings.GetBlobAbsoluteUrl();

            // Assert
            Assert.IsNotNull(absoluteUrl);
            Assert.AreEqual(TestBlobUrl, absoluteUrl.ToString().TrimEnd('/'));
        }

        /// <summary>
        /// Tests GetBlobAbsoluteUrl with relative blob URL.
        /// </summary>
        [TestMethod]
        public void SingleTenant_GetBlobAbsoluteUrl_WithRelativeUrl()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["MultiTenantEditor"] = "false",
                ["CosmosPublisherUrl"] = TestPublisherUrl,
                ["AzureBlobStorageEndPoint"] = "/static",
                ["CosmosStaticWebPages"] = "true"
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Act
            var absoluteUrl = settings.GetBlobAbsoluteUrl();

            // Assert
            Assert.IsNotNull(absoluteUrl);
            Assert.AreEqual($"{TestPublisherUrl}/static", absoluteUrl.ToString());
        }

        /// <summary>
        /// Tests that AllowSetup defaults to false when not specified.
        /// </summary>
        [TestMethod]
        public void SingleTenant_AllowSetup_DefaultsToFalse()
        {
            // Arrange
            _configuration = BuildConfiguration(multiTenant: false);

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.IsFalse(settings.AllowSetup);
        }

        /// <summary>
        /// Tests that AllowSetup respects configuration value.
        /// </summary>
        [TestMethod]
        public void SingleTenant_AllowSetup_RespectsConfiguration()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["MultiTenantEditor"] = "false",
                ["CosmosPublisherUrl"] = TestPublisherUrl,
                ["AzureBlobStorageEndPoint"] = TestBlobUrl,
                ["CosmosStaticWebPages"] = "true",
                ["AllowSetup"] = "true"
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.IsTrue(settings.AllowSetup);
        }

        #endregion

        #region Multi-Tenant Configuration Tests

        /// <summary>
        /// Tests that EditorSettings properly initializes in multi-tenant mode.
        /// </summary>
        [TestMethod]
        public void MultiTenant_InitializesCorrectly()
        {
            // Arrange
            _configuration = BuildConfiguration(multiTenant: true);
            var mockDynamicConfig = SetupMockDynamicConfigurationProvider("tenant1.com");
            _serviceProvider = new ServiceCollection()
                .AddSingleton(mockDynamicConfig.Object)
                .BuildServiceProvider();

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.IsTrue(settings.IsMultiTenantEditor);
        }

        /// <summary>
        /// Tests that multi-tenant configuration loads from DynamicConfigurationProvider.
        /// </summary>
        [TestMethod]
        public void MultiTenant_LoadsConfiguration_FromDynamicProvider()
        {
            // Arrange
            var domain = "tenant1.com";
            _configuration = BuildConfiguration(multiTenant: true);
            var mockDynamicConfig = SetupMockDynamicConfigurationProvider(domain);
            _serviceProvider = new ServiceCollection()
                .AddSingleton(mockDynamicConfig.Object)
                .BuildServiceProvider();

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.AreEqual("https://tenant1.com", settings.PublisherUrl);
            Assert.AreEqual("https://tenant1-blob.com", settings.BlobPublicUrl);
            Assert.IsTrue(settings.StaticWebPages);
            Assert.IsFalse(settings.CosmosRequiresAuthentication);
        }

        /// <summary>
        /// Tests that multi-tenant configuration caches by domain.
        /// </summary>
        [TestMethod]
        public void MultiTenant_CachesConfiguration_ByDomain()
        {
            // Arrange
            var domain = "tenant1.com";
            _configuration = BuildConfiguration(multiTenant: true);
            var mockDynamicConfig = SetupMockDynamicConfigurationProvider(domain);
            _serviceProvider = new ServiceCollection()
                .AddSingleton(mockDynamicConfig.Object)
                .BuildServiceProvider();

            // Act - First call
            var settings1 = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);
            var url1 = settings1.PublisherUrl;

            // Act - Second call (should use cache)
            var settings2 = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);
            var url2 = settings2.PublisherUrl;

            // Assert
            Assert.AreEqual(url1, url2);
            mockDynamicConfig.Verify(x => x.GetTenantConnectionAsync(It.IsAny<string>(), default), Times.Once);
        }

        /// <summary>
        /// Tests that different tenants get different configurations.
        /// </summary>
        [TestMethod]
        public void MultiTenant_DifferentTenants_GetDifferentConfigurations()
        {
            // Arrange - Tenant 1
            var domain1 = "tenant1.com";
            _configuration = BuildConfiguration(multiTenant: true);
            var mockDynamicConfig1 = SetupMockDynamicConfigurationProvider(domain1);
            var serviceProvider1 = new ServiceCollection()
                .AddSingleton(mockDynamicConfig1.Object)
                .BuildServiceProvider();

            var settings1 = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, serviceProvider1, domain1);

            // Arrange - Tenant 2
            var domain2 = "tenant2.com";
            var mockHttpContext2 = new Mock<IHttpContextAccessor>();
            var httpContext2 = new DefaultHttpContext();
            httpContext2.Request.Host = new HostString(domain2);
            mockHttpContext2.Setup(x => x.HttpContext).Returns(httpContext2);

            var mockDynamicConfig2 = SetupMockDynamicConfigurationProvider(domain2);
            var serviceProvider2 = new ServiceCollection()
                .AddSingleton(mockDynamicConfig2.Object)
                .BuildServiceProvider();

            var settings2 = new EditorSettings(_configuration, _db, mockHttpContext2.Object, _cache, serviceProvider2, domain2);

            // Assert
            Assert.AreNotEqual(settings1.PublisherUrl, settings2.PublisherUrl);
            Assert.AreEqual("https://tenant1.com", settings1.PublisherUrl);
            Assert.AreEqual("https://tenant2.com", settings2.PublisherUrl);
        }

        /// <summary>
        /// Tests that multi-tenant configuration handles missing DynamicConfigurationProvider.
        /// </summary>
        [TestMethod]
        public void MultiTenant_ThrowsException_WhenDynamicProviderMissing()
        {
            // Arrange
            _configuration = BuildConfiguration(multiTenant: true);
            _serviceProvider = new ServiceCollection().BuildServiceProvider(); // No provider registered

            // Act & Assert
            var exception = Assert.ThrowsExactly<InvalidOperationException>(() =>
                new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider));

            StringAssert.Contains(exception.Message, "IDynamicConfigurationProvider");
        }

        /// <summary>
        /// Tests multi-tenant cache key normalization (lowercase domain).
        /// </summary>
        [TestMethod]
        public void MultiTenant_CacheKey_NormalizedToLowercase()
        {
            // Arrange
            var domain = "TENANT1.COM";
            var mockHttpContext = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Host = new HostString(domain);
            mockHttpContext.Setup(x => x.HttpContext).Returns(httpContext);

            _configuration = BuildConfiguration(multiTenant: true);
            var mockDynamicConfig = SetupMockDynamicConfigurationProvider(domain.ToLower());
            _serviceProvider = new ServiceCollection()
                .AddSingleton(mockDynamicConfig.Object)
                .BuildServiceProvider();

            // Act
            var settings = new EditorSettings(_configuration, _db, mockHttpContext.Object, _cache, _serviceProvider);

            // Assert
            Assert.IsNotNull(settings.PublisherUrl);
            mockDynamicConfig.Verify(x => x.GetTenantDomainNameFromRequest(), Times.Once);
        }

        /// <summary>
        /// Tests that multi-tenant configuration supports PublisherMode "Static".
        /// </summary>
        [TestMethod]
        public void MultiTenant_StaticMode_SetsStaticWebPagesTrue()
        {
            // Arrange
            var domain = "tenant1.com";
            _configuration = BuildConfiguration(multiTenant: true);
            var mockDynamicConfig = new Mock<IDynamicConfigurationProvider>();
            mockDynamicConfig.Setup(x => x.GetTenantDomainNameFromRequest()).Returns(domain);
            mockDynamicConfig.Setup(x => x.GetTenantConnectionAsync(domain, It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new Connection
                {
                    DomainNames = new[] { domain },
                    WebsiteUrl = $"https://{domain}",
                    BlobPublicUrl = $"https://{domain.Replace(".", "-")}-blob.com",
                    PublisherRequiresAuthentication = false,
                    AllowSetup = false,
                    MicrosoftAppId = string.Empty,
                    PublisherMode = "Static"
                });

            _serviceProvider = new ServiceCollection()
                .AddSingleton(mockDynamicConfig.Object)
                .BuildServiceProvider();

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.IsTrue(settings.StaticWebPages);
        }

        /// <summary>
        /// Tests that multi-tenant configuration supports PublisherMode "Dynamic".
        /// </summary>
        [TestMethod]
        public void MultiTenant_DynamicMode_SetsStaticWebPagesFalse()
        {
            // Arrange
            var domain = "tenant1.com";
            _configuration = BuildConfiguration(multiTenant: true);
            var mockDynamicConfig = new Mock<IDynamicConfigurationProvider>();
            mockDynamicConfig.Setup(x => x.GetTenantDomainNameFromRequest()).Returns(domain);
            mockDynamicConfig.Setup(x => x.GetTenantConnectionAsync(domain, It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new Connection
                {
                    DomainNames = new[] { domain },
                    WebsiteUrl = $"https://{domain}",
                    BlobPublicUrl = $"https://{domain.Replace(".", "-")}-blob.com",
                    PublisherRequiresAuthentication = false,
                    AllowSetup = false,
                    MicrosoftAppId = string.Empty,
                    PublisherMode = "Dynamic" // Changed from "Static" to "Dynamic"
                });

            _serviceProvider = new ServiceCollection()
                .AddSingleton(mockDynamicConfig.Object)
                .BuildServiceProvider();

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.IsFalse(settings.StaticWebPages);
        }

        #endregion

        #region GetEditorConfig Tests

        /// <summary>
        /// Tests GetEditorConfig returns consistent configuration.
        /// </summary>
        [TestMethod]
        public void GetEditorConfig_ReturnsConsistentConfiguration()
        {
            // Arrange
            _configuration = BuildConfiguration(multiTenant: false);
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Act
            var config = settings.GetEditorConfig();

            // Assert
            Assert.IsNotNull(config);
            Assert.AreEqual(TestPublisherUrl, config.PublisherUrl);
            Assert.AreEqual(TestBlobUrl, config.BlobPublicUrl);
            Assert.IsTrue(config.StaticWebPages);
            Assert.IsFalse(config.CosmosRequiresAuthentication);
        }

        /// <summary>
        /// Tests that BackupStorageConnectionString returns null when not configured.
        /// </summary>
        [TestMethod]
        public void BackupStorageConnectionString_ReturnsNull_WhenNotConfigured()
        {
            // Arrange
            _configuration = BuildConfiguration(multiTenant: false);
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Act
            var backupConnectionString = settings.BackupStorageConnectionString;

            // Assert
            Assert.IsNull(backupConnectionString);
        }

        /// <summary>
        /// Tests that BackupStorageConnectionString returns configured value.
        /// </summary>
        [TestMethod]
        public void BackupStorageConnectionString_ReturnsValue_WhenConfigured()
        {
            // Arrange
            var backupConnectionString = "DefaultEndpointsProtocol=https;AccountName=backup;AccountKey=key;";
            var configData = new Dictionary<string, string>
            {
                ["MultiTenantEditor"] = "false",
                ["CosmosPublisherUrl"] = TestPublisherUrl,
                ["AzureBlobStorageEndPoint"] = TestBlobUrl,
                ["CosmosStaticWebPages"] = "true",
                ["ConnectionStrings:BackupStorageConnectionString"] = backupConnectionString
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.AreEqual(backupConnectionString, settings.BackupStorageConnectionString);
        }

        /// <summary>
        /// Tests that MicrosoftAppId returns configured value.
        /// </summary>
        [TestMethod]
        public void MicrosoftAppId_ReturnsConfiguredValue()
        {
            // Arrange
            var appId = "test-app-id-12345";
            var configData = new Dictionary<string, string>
            {
                ["MultiTenantEditor"] = "false",
                ["CosmosPublisherUrl"] = TestPublisherUrl,
                ["AzureBlobStorageEndPoint"] = TestBlobUrl,
                ["CosmosStaticWebPages"] = "true",
                ["MicrosoftAppId"] = appId
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.AreEqual(appId, settings.MicrosoftAppId);
        }

        #endregion

        #region Edge Cases and Error Handling

        /// <summary>
        /// Tests EditorSettings when HttpContext is null.
        /// </summary>
        [TestMethod]
        public void EditorSettings_HandlesNullHttpContext()
        {
            // Arrange
            _configuration = BuildConfiguration(multiTenant: false);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.IsNotNull(settings);
            Assert.AreEqual(TestPublisherUrl, settings.PublisherUrl);
        }

        /// <summary>
        /// Tests that configuration is case-insensitive for boolean values.
        /// </summary>
        [TestMethod]
        public void Configuration_IsCaseInsensitive_ForBooleans()
        {
            // Arrange
            var configData = new Dictionary<string, string>
            {
                ["MultiTenantEditor"] = "FALSE",
                ["CosmosPublisherUrl"] = TestPublisherUrl,
                ["AzureBlobStorageEndPoint"] = TestBlobUrl,
                ["CosmosStaticWebPages"] = "TRUE",
                ["CosmosRequiresAuthentication"] = "False",
                ["AllowSetup"] = "True"
            };
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            // Act
            var settings = new EditorSettings(_configuration, _db, _mockHttpContextAccessor.Object, _cache, _serviceProvider);

            // Assert
            Assert.IsFalse(settings.IsMultiTenantEditor);
            Assert.IsTrue(settings.StaticWebPages);
            Assert.IsFalse(settings.CosmosRequiresAuthentication);
            Assert.IsTrue(settings.AllowSetup);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Builds test configuration.
        /// </summary>
        private IConfiguration BuildConfiguration(bool multiTenant, bool includeBackupStorage = false)
        {
            var configData = new Dictionary<string, string>
            {
                ["MultiTenantEditor"] = multiTenant.ToString(),
                ["CosmosPublisherUrl"] = TestPublisherUrl,
                ["AzureBlobStorageEndPoint"] = TestBlobUrl,
                ["CosmosStaticWebPages"] = "true",
                ["CosmosRequiresAuthentication"] = "false"
            };

            if (includeBackupStorage)
            {
                configData["ConnectionStrings:BackupStorageConnectionString"] = "DefaultEndpointsProtocol=https;AccountName=backup;";
            }

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        /// <summary>
        /// Sets up mock DynamicConfigurationProvider for multi-tenant tests.
        /// </summary>
        private Mock<IDynamicConfigurationProvider> SetupMockDynamicConfigurationProvider(string domain)
        {
            var mockDynamicConfig = new Mock<IDynamicConfigurationProvider>();
            mockDynamicConfig.Setup(x => x.GetTenantDomainNameFromRequest()).Returns(domain);
            mockDynamicConfig.Setup(x => x.GetTenantConnectionAsync(domain, It.IsAny<System.Threading.CancellationToken>()))
                .ReturnsAsync(new Connection
                {
                    DomainNames = new[] { domain },
                    WebsiteUrl = $"https://{domain}",
                    BlobPublicUrl = $"https://{domain.Split('.')[0]}-blob.com",
                    PublisherRequiresAuthentication = false,
                    AllowSetup = false,
                    MicrosoftAppId = string.Empty,
                    PublisherMode = "Static"
                });

            return mockDynamicConfig;
        }

        #endregion
    }
}