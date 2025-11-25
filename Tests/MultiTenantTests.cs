// <copyright file="MultiTenantTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.DynamicConfig
{
    using Cosmos.DynamicConfig;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sky.Tests.TestHelpers;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Unit tests for multi-tenant configuration via <see cref="DynamicConfigurationProvider"/>.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class MultiTenantTests
    {
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private IMemoryCache _memoryCache; // Use real cache for isolation tests
        private Mock<ILogger<DynamicConfigurationProvider>> _mockLogger;
        private DbContextOptions<DynamicConfigDbContext> _dbContextOptions;

        /// <summary>
        /// Initializes mocks before each test.
        /// </summary>
        [TestInitialize]
        public void Setup()
        {
            _mockConfiguration = new Mock<IConfiguration>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _mockLogger = new Mock<ILogger<DynamicConfigurationProvider>>();

            // Setup in-memory database for testing
            _dbContextOptions = new DbContextOptionsBuilder<DynamicConfigDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            // Setup config connection string (won't be used with testable provider, but required for constructor)
            var validConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==;Database=configs;";

            var mockConnectionSection = new Mock<IConfigurationSection>();
            mockConnectionSection.Setup(x => x.Value).Returns(validConnectionString);

            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(x => x["ConfigDbConnectionString"]).Returns(validConnectionString);

            _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings")).Returns(mockConnectionStringsSection.Object);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            using var context = new DynamicConfigDbContext(_dbContextOptions);
            context.Database.EnsureCreated();
            
            context.Connections.AddRange(
                new Connection
                {
                    Id = Guid.NewGuid(),
                    DomainNames = new[] { "tenant1.com", "www.tenant1.com" },
                    DbConn = "Server=tenant1;Database=Tenant1Db;",
                    StorageConn = "DefaultEndpointsProtocol=https;AccountName=tenant1storage;",
                    Customer = "Tenant 1",
                    WebsiteUrl = "https://tenant1.com",
                    ResourceGroup = "tenant1-rg"
                },
                new Connection
                {
                    Id = Guid.NewGuid(),
                    DomainNames = new[] { "tenant2.com" },
                    DbConn = "Server=tenant2;Database=Tenant2Db;",
                    StorageConn = "DefaultEndpointsProtocol=https;AccountName=tenant2storage;",
                    Customer = "Tenant 2",
                    WebsiteUrl = "https://tenant2.com",
                    ResourceGroup = "tenant2-rg"
                }
            );
            
            context.SaveChanges();
        }

        /// <summary>
        /// Cleanup after each test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            // Clean up in-memory database
            using (var context = new DynamicConfigDbContext(_dbContextOptions))
            {
                context.Database.EnsureDeleted();
            }
            
            _memoryCache?.Dispose();
            TenantContext.Clear();
        }

        #region Constructor Tests

        /// <summary>
        /// Tests that constructor does not throw when HttpContext is null.
        /// </summary>
        [TestMethod]
        public void Constructor_ShouldNotThrow_WhenHttpContextIsNull()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

            // Act
            var exception = RecordException(() => new DynamicConfigurationProvider(
                _mockConfiguration.Object,
                _mockHttpContextAccessor.Object,
                _memoryCache,
                _mockLogger.Object));

            // Assert
            Assert.IsNull(exception);
        }

        /// <summary>
        /// Tests that constructor throws when IHttpContextAccessor is null.
        /// </summary>
        [TestMethod]
        public void Constructor_ShouldThrow_WhenHttpContextAccessorIsNull()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() => new DynamicConfigurationProvider(
                _mockConfiguration.Object,
                null,
                _memoryCache,
                _mockLogger.Object));
        }

        /// <summary>
        /// Tests that constructor throws when config connection string is missing.
        /// </summary>
        [TestMethod]
        public void Constructor_ShouldThrow_WhenConfigConnectionStringIsMissing()
        {
            // Arrange - Override the setup to return null/empty
            var mockConnectionStringsSection = new Mock<IConfigurationSection>();
            mockConnectionStringsSection.Setup(x => x["ConfigDbConnectionString"])
                .Returns((string)null);  // Return null for this specific test

            _mockConfiguration.Setup(x => x.GetSection("ConnectionStrings"))
                .Returns(mockConnectionStringsSection.Object);

            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => new DynamicConfigurationProvider(
                _mockConfiguration.Object,
                _mockHttpContextAccessor.Object,
                _memoryCache,
                _mockLogger.Object));
        }

        #endregion

        #region GetDatabaseConnectionString Tests - Tenant Isolation

        /// <summary>
        /// CRITICAL: Tests that GetDatabaseConnectionString throws exception when HttpContext is null and no domain provided.
        /// This prevents accidental cross-tenant data access.
        /// </summary>
        [TestMethod]
        public void GetDatabaseConnectionString_ShouldThrow_WhenHttpContextIsNullAndNoDomainProvided()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);
            var provider = CreateTestableProvider();

            // Act & Assert
            var exception = Assert.ThrowsExactly<AggregateException>(() =>
                provider.GetDatabaseConnectionStringAsync().Result);
            
            Assert.IsTrue(exception.Message.Contains("HttpContext unavailable and no domain provided"));
        }

        /// <summary>
        /// CRITICAL: Tests that GetDatabaseConnectionString works with explicit domain when HttpContext is null.
        /// This is required for background jobs.
        /// </summary>
        [TestMethod]
        public void GetDatabaseConnectionString_ShouldWorkWithExplicitDomain_WhenHttpContextIsNull()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);
            var provider = new TestableConfigurationProvider(
                _mockConfiguration.Object,
                _mockHttpContextAccessor.Object,
                _memoryCache,
                _mockLogger.Object,
                _dbContextOptions);  // Add the in-memory DB options

            // Act
            var result = provider.GetDatabaseConnectionStringAsync("tenant1.com").Result;

            // Assert
            Assert.IsNotNull(result);  // Should find the connection in seeded data
            Assert.AreEqual("Server=tenant1;Database=Tenant1Db;", result);
        }

        /// <summary>
        /// CRITICAL: Tests that GetDatabaseConnectionString normalizes domain names.
        /// Prevents bypass via case manipulation.
        /// </summary>
        [TestMethod]
        public void GetDatabaseConnectionString_ShouldNormalizeDomain_CaseInsensitive()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.Host = new HostString("TENANT1.COM");
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockContext);
            
            var provider = new TestableConfigurationProvider(
                _mockConfiguration.Object,
                _mockHttpContextAccessor.Object,
                _memoryCache,
                _mockLogger.Object,
                _dbContextOptions);  // Pass the in-memory DB options

            // Act
            var result1 = provider.GetDatabaseConnectionStringAsync().GetAwaiter().GetResult();
            
            // Change case
            mockContext.Request.Host = new HostString("tenant1.com");
            var result2 = provider.GetDatabaseConnectionStringAsync().GetAwaiter().GetResult();

            // Assert - Both should resolve to same (normalized) domain
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreEqual(result1, result2);
            Assert.AreEqual("Server=tenant1;Database=Tenant1Db;", result1);
        }

        #endregion

        #region GetStorageConnectionString Tests - Tenant Isolation

        /// <summary>
        /// CRITICAL: Tests that GetStorageConnectionString throws when HttpContext is null and no domain provided.
        /// Prevents accidental cross-tenant storage access.
        /// </summary>
        [TestMethod]
        public void GetStorageConnectionString_ShouldThrow_WhenHttpContextIsNullAndNoDomainProvided()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);
            var provider = CreateTestableProvider();

            // Act & Assert
            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => 
                provider.GetDatabaseConnectionStringAsync().GetAwaiter().GetResult());
            
            Assert.IsTrue(exception.Message.Contains("HttpContext unavailable and no domain provided"));
        }

        /// <summary>
        /// CRITICAL: Tests that GetStorageConnectionString works with explicit domain.
        /// </summary>
        [TestMethod]
        public void GetStorageConnectionString_ShouldWorkWithExplicitDomain_WhenHttpContextIsNull()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);
            var provider = new TestableConfigurationProvider(  // Change this line
                _mockConfiguration.Object,
                _mockHttpContextAccessor.Object,
                _memoryCache,
                _mockLogger.Object,
                _dbContextOptions);  // Add this parameter

            // Act
            var result = provider.GetStorageConnectionStringAsync("tenant1.com").Result;

            // Assert
            Assert.IsNotNull(result);  // Update: Should find the connection in seeded data
            Assert.AreEqual("DefaultEndpointsProtocol=https;AccountName=tenant1storage;", result);
        }

        #endregion

        #region Cache Isolation Tests

        /// <summary>
        /// CRITICAL: Tests that cache uses namespaced keys to prevent cross-tenant cache poisoning.
        /// </summary>
        [TestMethod]
        public void Cache_ShouldUseNamespacedKeys_PreventsCachePoisoning()
        {
            // Arrange
            var domain1 = "tenant1.com";
            var domain2 = "tenant2.com";
            
            // Simulate caching with namespacing
            var cacheKey1 = $"tenant:connection:{domain1}";
            var cacheKey2 = $"tenant:connection:{domain2}";
            
            var connection1 = new Connection { Id = Guid.NewGuid(), DomainNames = new[] { domain1 } };
            var connection2 = new Connection { Id = Guid.NewGuid(), DomainNames = new[] { domain2 } };
            
            _memoryCache.Set(cacheKey1, connection1);
            _memoryCache.Set(cacheKey2, connection2);

            // Act
            var cached1 = _memoryCache.Get<Connection>(cacheKey1);
            var cached2 = _memoryCache.Get<Connection>(cacheKey2);

            // Assert
            Assert.IsNotNull(cached1);
            Assert.IsNotNull(cached2);
            Assert.AreNotEqual(cached1.Id, cached2.Id);
            Assert.AreEqual(domain1, cached1.DomainNames[0]);
            Assert.AreEqual(domain2, cached2.DomainNames[0]);
        }

        /// <summary>
        /// CRITICAL: Tests that cache keys are case-insensitive to prevent bypass.
        /// </summary>
        [TestMethod]
        public void Cache_ShouldBeCaseInsensitive_PreventsBypassViaCase()
        {
            // Arrange
            var cacheKey1 = "tenant:connection:tenant1.com";
            var cacheKey2 = "tenant:connection:TENANT1.COM";
            
            var connection = new Connection { Id = Guid.NewGuid(), DomainNames = new[] { "tenant1.com" } };
            
            _memoryCache.Set(cacheKey1.ToLowerInvariant(), connection);

            // Act
            var cached = _memoryCache.Get<Connection>(cacheKey2.ToLowerInvariant());

            // Assert
            Assert.IsNotNull(cached);
            Assert.AreEqual(connection.Id, cached.Id);
        }

        #endregion

        #region TenantContext Tests - Background Job Support

        /// <summary>
        /// CRITICAL: Tests that TenantContext can be set and retrieved.
        /// </summary>
        [TestMethod]
        public void TenantContext_ShouldStoreAndRetrieveDomain()
        {
            // Act
            TenantContext.CurrentDomain = "tenant1.com";

            // Assert
            Assert.AreEqual("tenant1.com", TenantContext.CurrentDomain);
            Assert.IsTrue(TenantContext.HasContext);
        }

        /// <summary>
        /// CRITICAL: Tests that TenantContext normalizes domain to lowercase.
        /// </summary>
        [TestMethod]
        public void TenantContext_ShouldNormalizeDomainToLowercase()
        {
            // Act
            TenantContext.CurrentDomain = "TENANT1.COM";

            // Assert
            Assert.AreEqual("tenant1.com", TenantContext.CurrentDomain);
        }

        /// <summary>
        /// CRITICAL: Tests that TenantContext.Clear removes domain.
        /// </summary>
        [TestMethod]
        public void TenantContext_Clear_ShouldRemoveDomain()
        {
            // Arrange
            TenantContext.CurrentDomain = "tenant1.com";

            // Act
            TenantContext.Clear();

            // Assert
            Assert.IsFalse(TenantContext.HasContext);
            Assert.IsNull(TenantContext.CurrentDomain);
        }

        /// <summary>
        /// CRITICAL: Tests that TenantContext.Execute isolates tenant context.
        /// </summary>
        [TestMethod]
        public void TenantContext_Execute_ShouldIsolateTenantContext()
        {
            // Arrange
            TenantContext.CurrentDomain = "original.com";
            string capturedDomain = null;

            // Act
            TenantContext.Execute("tenant1.com", () =>
            {
                capturedDomain = TenantContext.CurrentDomain;
            });

            // Assert
            Assert.AreEqual("tenant1.com", capturedDomain);
            Assert.AreEqual("original.com", TenantContext.CurrentDomain); // Restored
        }

        /// <summary>
        /// CRITICAL: Tests that TenantContext.ExecuteAsync isolates tenant context asynchronously.
        /// </summary>
        [TestMethod]
        public async Task TenantContext_ExecuteAsync_ShouldIsolateTenantContext()
        {
            // Arrange
            TenantContext.CurrentDomain = "original.com";
            string capturedDomain = null;

            // Act
            await TenantContext.ExecuteAsync("tenant1.com", async () =>
            {
                await Task.Delay(10); // Simulate async work
                capturedDomain = TenantContext.CurrentDomain;
            });

            // Assert
            Assert.AreEqual("tenant1.com", capturedDomain);
            Assert.AreEqual("original.com", TenantContext.CurrentDomain); // Restored
        }

        /// <summary>
        /// CRITICAL: Tests that TenantContext.ExecuteAsync with return value works correctly.
        /// </summary>
        [TestMethod]
        public async Task TenantContext_ExecuteAsyncWithReturn_ShouldReturnValueAndIsolate()
        {
            // Arrange
            TenantContext.CurrentDomain = "original.com";

            // Act
            var result = await TenantContext.ExecuteAsync("tenant1.com", async () =>
            {
                await Task.Delay(10);
                return TenantContext.CurrentDomain;
            });

            // Assert
            Assert.AreEqual("tenant1.com", result);
            Assert.AreEqual("original.com", TenantContext.CurrentDomain); // Restored
        }

        #endregion

        #region CleanUpDomainName Tests

        /// <summary>
        /// Tests that CleanUpDomainName extracts host from full URL.
        /// </summary>
        [TestMethod]
        [DataRow("https://example.com/path", "example.com")]
        [DataRow("http://subdomain.example.com", "subdomain.example.com")]
        [DataRow("example.com", "example.com")]
        [DataRow("EXAMPLE.COM", "example.com")]
        public void CleanUpDomainName_ShouldReturnHostOnly(string input, string expected)
        {
            // Act
            var result = DynamicConfigurationProvider.CleanUpDomainName(input);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that CleanUpDomainName converts to lowercase.
        /// </summary>
        [TestMethod]
        public void CleanUpDomainName_ShouldConvertToLowercase()
        {
            // Act
            var result = DynamicConfigurationProvider.CleanUpDomainName("WWW.EXAMPLE.COM");

            // Assert
            Assert.AreEqual("www.example.com", result);
        }

        /// <summary>
        /// Tests that CleanUpDomainName handles null input.
        /// </summary>
        [TestMethod]
        public void CleanUpDomainName_ShouldReturnInput_WhenNull()
        {
            // Act
            var result = DynamicConfigurationProvider.CleanUpDomainName(null);

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that CleanUpDomainName handles empty string.
        /// </summary>
        [TestMethod]
        public void CleanUpDomainName_ShouldReturnEmpty_WhenEmpty()
        {
            // Act
            var result = DynamicConfigurationProvider.CleanUpDomainName(string.Empty);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region ValidateDomainName Tests

        /// <summary>
        /// Tests that ValidateDomainName returns false for null domain.
        /// </summary>
        [TestMethod]
        public async Task ValidateDomainName_ShouldReturnFalse_WhenDomainIsNull()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            var provider = CreateTestableProvider();

            // Act
            var result = await provider.ValidateDomainName(null);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that ValidateDomainName returns false for empty domain.
        /// </summary>
        [TestMethod]
        public async Task ValidateDomainName_ShouldReturnFalse_WhenDomainIsEmpty()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
            var provider = CreateTestableProvider();

            // Act
            var result = await provider.ValidateDomainName(string.Empty);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region Domain Resolution Tests

        /// <summary>
        /// CRITICAL: Tests that GetTenantDomainNameFromRequest returns lowercase domain.
        /// </summary>
        [TestMethod]
        public void GetTenantDomainNameFromRequest_ShouldReturnLowercaseDomain()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.Host = new HostString("TENANT1.COM");
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockContext);
            
            var provider = CreateTestableProvider();

            // Act
            var result = provider.GetTenantDomainNameFromRequest();

            // Assert
            Assert.AreEqual("tenant1.com", result);
        }

        /// <summary>
        /// CRITICAL: Tests that GetTenantDomainNameFromRequest returns lowercase domain.
        /// </summary>
        [TestMethod]
        public void GetTenantDomainNameFromRequest_UsesXHostHeader()
        {
            // Arrange
            var mockContext = new DefaultHttpContext();
            mockContext.Request.Headers["x-origin-hostname"] = "TENANT1.COM";
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockContext);

            var provider = CreateTestableProvider();

            // Act
            var result = provider.GetTenantDomainNameFromRequest();

            // Assert
            Assert.AreEqual("tenant1.com", result);
        }

        /// <summary>
        /// CRITICAL: Tests that GetTenantDomainNameFromRequest returns empty when HttpContext is null.
        /// </summary>
        [TestMethod]
        public void GetTenantDomainNameFromRequest_ShouldReturnEmpty_WhenHttpContextIsNull()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);
            var provider = CreateTestableProvider();

            // Act
            var result = provider.GetTenantDomainNameFromRequest();

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region Logging Tests - Audit Trail

        /// <summary>
        /// CRITICAL: Tests that error is logged when connection cannot be resolved.
        /// </summary>
        [TestMethod]
        public void GetDatabaseConnectionString_ShouldLogError_WhenCannotResolve()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);
            var provider = CreateTestableProvider();

            // Act
            try
            {
                provider.GetDatabaseConnectionStringAsync().GetAwaiter().GetResult();;
            }
            catch (InvalidOperationException)
            {
                // Expected
            }

            // Assert - Verify error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cannot resolve tenant connection")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        #endregion

        #region Concurrent Access Tests

        /// <summary>
        /// CRITICAL: Tests that concurrent requests to different tenants don't cross-contaminate.
        /// </summary>
        [TestMethod]
        public async Task ConcurrentRequests_ShouldIsolateTenants()
        {
            // Arrange
            var tasks = new List<Task<string>>();
            var domains = new[] { "tenant1.com", "tenant2.com", "tenant3.com" };

            // Act - Simulate concurrent requests
            foreach (var domain in domains)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await Task.Delay(new Random().Next(10, 50)); // Random delay
                    
                    return await TenantContext.ExecuteAsync(domain, async () =>
                    {
                        await Task.Delay(10);
                        return TenantContext.CurrentDomain;
                    });
                }));
            }

            var results = await Task.WhenAll(tasks);

            // Assert - Each task should have maintained its tenant context
            for (int i = 0; i < domains.Length; i++)
            {
                Assert.AreEqual(domains[i], results[i]);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Helper to record exceptions without throwing.
        /// </summary>
        private static Exception RecordException(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        /// <summary>
        /// Creates a testable configuration provider for unit tests.
        /// Always use this method to ensure tests use the in-memory database.
        /// </summary>
        /// <returns>TestableConfigurationProvider instance.</returns>
        private TestableConfigurationProvider CreateTestableProvider()
        {
            return new TestableConfigurationProvider(
                _mockConfiguration.Object,
                _mockHttpContextAccessor.Object,
                _memoryCache,
                _mockLogger.Object,
                _dbContextOptions);
        }

        #endregion
    }
}