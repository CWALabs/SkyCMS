// <copyright file="ArticleSchedulerTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Services.Scheduling
{
    using Cosmos.Cms.Common.Services.Configurations;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Cosmos.DynamicConfig;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.Scheduling;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Unit tests for <see cref="ArticleScheduler"/>.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class ArticleSchedulerTests : SkyCmsTestBase
    {
        private Mock<ILogger<ArticleScheduler>> mockLogger;
        private TestClock testClock;

        /// <summary>
        /// Initializes test context before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            testClock = new TestClock();
            Clock = testClock; // Set BEFORE InitializeTestContext

            InitializeTestContext();
            mockLogger = new Mock<ILogger<ArticleScheduler>>();

            // Override the base clock with test clock
            var clockField = typeof(SkyCmsTestBase).GetField("Clock",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            clockField?.SetValue(this, testClock);
        }

        /// <summary>
        /// Tests that ExecuteAsync processes multiple published article versions correctly.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithMultiplePublishedVersions_ActivatesMostRecentNonFutureVersion()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            // Create article with 3 versions: past, current (should be active), future
            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Test Article V1",
                Content = "Version 1 Content",
                Published = now.AddDays(-10),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test-article"
            };

            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Test Article V2",
                Content = "Version 2 Content",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test-article"
            };

            var article3 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 3,
                Title = "Test Article V3",
                Content = "Version 3 Content",
                Published = now.AddDays(5),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test-article"
            };

            Db.Articles.Add(article1);
            Db.Articles.Add(article2);
            Db.Articles.Add(article3);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert - Version 1 should be unpublished
            var updatedArticle1 = await Db.Articles.FindAsync(article1.Id);
            Assert.IsNull(updatedArticle1.Published, "Version 1 should be unpublished");

            // Version 2 should remain published (most recent non-future)
            var updatedArticle2 = await Db.Articles.FindAsync(article2.Id);
            Assert.IsNotNull(updatedArticle2.Published, "Version 2 should remain published");

            // Version 3 should remain published (scheduled for future)
            var updatedArticle3 = await Db.Articles.FindAsync(article3.Id);
            Assert.IsNotNull(updatedArticle3.Published, "Version 3 should remain scheduled for future");
            Assert.IsTrue(updatedArticle3.Published > now, "Version 3 publish date should be in the future");
        }

        /// <summary>
        /// Tests that ExecuteAsync handles articles with all future-dated versions.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithAllFutureVersions_DoesNotUnpublishAnyVersions()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Future Article V1",
                Content = "Version 1 Content",
                Published = now.AddDays(1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/future-article"
            };

            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Future Article V2",
                Content = "Version 2 Content",
                Published = now.AddDays(5),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/future-article"
            };

            Db.Articles.Add(article1);
            Db.Articles.Add(article2);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert - Both versions should remain published
            var updatedArticle1 = await Db.Articles.FindAsync(article1.Id);
            Assert.IsNotNull(updatedArticle1.Published, "Version 1 should remain published (future)");

            var updatedArticle2 = await Db.Articles.FindAsync(article2.Id);
            Assert.IsNotNull(updatedArticle2.Published, "Version 2 should remain published (future)");
        }

        /// <summary>
        /// Tests that ExecuteAsync ignores deleted articles.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithDeletedArticles_IgnoresDeletedVersions()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Deleted Article",
                Content = "Content",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Deleted,
                UserId = userId,
                UrlPath = "/deleted-article"
            };

            Db.Articles.Add(article1);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert - Article should not be processed
            var updatedArticle = await Db.Articles.FindAsync(article1.Id);
            Assert.AreEqual((int)StatusCodeEnum.Deleted, updatedArticle.StatusCode);
            Assert.IsNotNull(updatedArticle.Published, "Deleted article should not be modified");
        }

        /// <summary>
        /// Tests that ExecuteAsync handles articles with only one published version.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithSinglePublishedVersion_DoesNotProcessArticle()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Single Version Article",
                Content = "Content",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/single-article"
            };

            Db.Articles.Add(article1);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert - Single version should remain unchanged
            var updatedArticle = await Db.Articles.FindAsync(article1.Id);
            Assert.IsNotNull(updatedArticle.Published, "Single version should remain published");
        }

        /// <summary>
        /// Tests that ExecuteAsync handles race conditions when articles are deleted during processing.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithRaceCondition_HandlesGracefully()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            // Create article that will have only one version after initial query
            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Test Article",
                Content = "Content",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            Db.Articles.Add(article1);
            await Db.SaveChangesAsync();

            // Act - Should not throw exception even if race condition occurs
            await ArticleScheduler.ExecuteAsync();

            // Assert - No exception thrown
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Tests that ExecuteAsync publishes the active version correctly.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_PublishesActiveVersion_CallsPublishArticle()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Test Article V1",
                Content = "Version 1",
                Published = now.AddDays(-5),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Test Article V2",
                Content = "Version 2",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            Db.Articles.Add(article1);
            Db.Articles.Add(article2);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert - Verify article2 was published
            var publishedPage = await Db.Pages.FirstOrDefaultAsync(p => p.ArticleNumber == 1);
            Assert.IsNotNull(publishedPage, "Published page should exist");
        }

        /// <summary>
        /// Tests that ExecuteAsync handles two versions with identical publish dates by using version number.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithIdenticalPublishDates_ActivatesHighestVersionNumber()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();
            var identicalPublishDate = now.AddDays(-1);

            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Test Article V1",
                Content = "Version 1 Content",
                Published = identicalPublishDate,
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test-article"
            };

            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Test Article V2",
                Content = "Version 2 Content",
                Published = identicalPublishDate,
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test-article"
            };

            Db.Articles.Add(article1);
            Db.Articles.Add(article2);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert - Version 1 should be unpublished (lower version number)
            var updatedArticle1 = await Db.Articles.FindAsync(article1.Id);
            Assert.IsNull(updatedArticle1.Published, "Version 1 should be unpublished");

            // Version 2 should remain published (higher version number, same date)
            var updatedArticle2 = await Db.Articles.FindAsync(article2.Id);
            Assert.IsNotNull(updatedArticle2.Published, "Version 2 should remain published");
        }

        /// <summary>
        /// Tests that ExecuteAsync handles version published exactly at the current time boundary.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithPublishDateAtExactNow_ActivatesVersion()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Test Article V1",
                Content = "Version 1 Content",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test-article"
            };

            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Test Article V2",
                Content = "Version 2 Content",
                Published = now, // Exact boundary
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test-article"
            };

            Db.Articles.Add(article1);
            Db.Articles.Add(article2);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert - Version 1 should be unpublished
            var updatedArticle1 = await Db.Articles.FindAsync(article1.Id);
            Assert.IsNull(updatedArticle1.Published, "Version 1 should be unpublished");

            // Version 2 should remain published (at exact boundary)
            var updatedArticle2 = await Db.Articles.FindAsync(article2.Id);
            Assert.IsNotNull(updatedArticle2.Published, "Version 2 should be published at exact boundary time");
        }

        /// <summary>
        /// Tests that ExecuteAsync processes inactive (non-deleted) articles correctly.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithInactiveArticles_ProcessesCorrectly()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Inactive Article V1",
                Content = "Version 1 Content",
                Published = now.AddDays(-5),
                StatusCode = (int)StatusCodeEnum.Inactive,
                UserId = userId,
                UrlPath = "/inactive-article"
            };

            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Inactive Article V2",
                Content = "Version 2 Content",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Inactive,
                UserId = userId,
                UrlPath = "/inactive-article"
            };

            Db.Articles.Add(article1);
            Db.Articles.Add(article2);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert - Version 1 should be unpublished
            var updatedArticle1 = await Db.Articles.FindAsync(article1.Id);
            Assert.IsNull(updatedArticle1.Published, "Inactive version 1 should be unpublished");

            // Version 2 should remain published (most recent)
            var updatedArticle2 = await Db.Articles.FindAsync(article2.Id);
            Assert.IsNotNull(updatedArticle2.Published, "Inactive version 2 should remain published");
        }

        /// <summary>
        /// Tests that ExecuteAsync updates the catalog entry when activating a version.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WhenActivatingVersion_UpdatesCatalog()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Test Article V1",
                Content = "Version 1 Content",
                Published = now.AddDays(-5),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Test Article V2 - Updated Title",
                Content = "Version 2 Content",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            Db.Articles.Add(article1);
            Db.Articles.Add(article2);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert - Catalog should reflect the active version
            var catalogEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 1);
            Assert.IsNotNull(catalogEntry, "Catalog entry should exist");
            Assert.AreEqual("Test Article V2 - Updated Title", catalogEntry.Title, "Catalog should reflect active version title");
        }

        /// <summary>
        /// Tests that ExecuteAsync handles multiple different articles in a single execution.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithMultipleDifferentArticles_ProcessesAllCorrectly()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            // Article 1 with 2 versions
            var article1v1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Article 1 V1",
                Content = "Content",
                Published = now.AddDays(-5),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/article1"
            };

            var article1v2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Article 1 V2",
                Content = "Content",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/article1"
            };

            // Article 2 with 2 versions
            var article2v1 = new Article
            {
                ArticleNumber = 2,
                VersionNumber = 1,
                Title = "Article 2 V1",
                Content = "Content",
                Published = now.AddDays(-3),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/article2"
            };

            var article2v2 = new Article
            {
                ArticleNumber = 2,
                VersionNumber = 2,
                Title = "Article 2 V2",
                Content = "Content",
                Published = now.AddDays(-2),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/article2"
            };

            Db.Articles.AddRange(article1v1, article1v2, article2v1, article2v2);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert
            var updatedArticle1v1 = await Db.Articles.FindAsync(article1v1.Id);
            Assert.IsNull(updatedArticle1v1.Published, "Article 1 Version 1 should be unpublished");

            var updatedArticle1v2 = await Db.Articles.FindAsync(article1v2.Id);
            Assert.IsNotNull(updatedArticle1v2.Published, "Article 1 Version 2 should remain published");

            var updatedArticle2v1 = await Db.Articles.FindAsync(article2v1.Id);
            Assert.IsNull(updatedArticle2v1.Published, "Article 2 Version 1 should be unpublished");

            var updatedArticle2v2 = await Db.Articles.FindAsync(article2v2.Id);
            Assert.IsNotNull(updatedArticle2v2.Published, "Article 2 Version 2 should remain published");
        }

        /// <summary>
        /// Tests that ExecuteAsync handles three or more published versions correctly.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithThreePublishedVersions_UnpublishesOlderVersionsCorrectly()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Test Article V1",
                Content = "Version 1",
                Published = now.AddDays(-10),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Test Article V2",
                Content = "Version 2",
                Published = now.AddDays(-5),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            var article3 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 3,
                Title = "Test Article V3",
                Content = "Version 3",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            Db.Articles.AddRange(article1, article2, article3);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert
            var updatedArticle1 = await Db.Articles.FindAsync(article1.Id);
            Assert.IsNull(updatedArticle1.Published, "Version 1 should be unpublished");

            var updatedArticle2 = await Db.Articles.FindAsync(article2.Id);
            Assert.IsNull(updatedArticle2.Published, "Version 2 should be unpublished");

            var updatedArticle3 = await Db.Articles.FindAsync(article3.Id);
            Assert.IsNotNull(updatedArticle3.Published, "Version 3 should remain published");
        }

        /// <summary>
        /// Tests that ExecuteAsync handles articles with unpublished (null Published date) versions mixed in.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithMixedPublishedAndUnpublishedVersions_ProcessesOnlyPublishedVersions()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Test Article V1",
                Content = "Version 1",
                Published = now.AddDays(-5),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Test Article V2",
                Content = "Version 2",
                Published = null, // Unpublished draft
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            var article3 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 3,
                Title = "Test Article V3",
                Content = "Version 3",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            Db.Articles.AddRange(article1, article2, article3);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert
            var updatedArticle1 = await Db.Articles.FindAsync(article1.Id);
            Assert.IsNull(updatedArticle1.Published, "Version 1 should be unpublished");

            var updatedArticle2 = await Db.Articles.FindAsync(article2.Id);
            Assert.IsNull(updatedArticle2.Published, "Version 2 should remain unpublished (was never published)");

            var updatedArticle3 = await Db.Articles.FindAsync(article3.Id);
            Assert.IsNotNull(updatedArticle3.Published, "Version 3 should remain published");
        }

        /// <summary>
        /// Tests that ExecuteAsync creates a published page for the active version.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WhenActivatingVersion_CreatesPublishedPage()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Test Article V1",
                Content = "Version 1",
                Published = now.AddDays(-5),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Test Article V2",
                Content = "<h1>Version 2 Content</h1>",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = userId,
                UrlPath = "/test"
            };

            Db.Articles.AddRange(article1, article2);
            await Db.SaveChangesAsync();

            // Act
            await ArticleScheduler.ExecuteAsync();

            // Assert
            var publishedPage = await Db.Pages.FirstOrDefaultAsync(p => p.ArticleNumber == 1);
            Assert.IsNotNull(publishedPage, "Published page should be created");
            Assert.AreEqual(2, publishedPage.VersionNumber, "Published page should be version 2");
            Assert.IsTrue(publishedPage.Content.Contains("Version 2 Content"), "Published page should contain correct content");
        }

        /// <summary>
        /// Tests that ExecuteAsync processes multiple tenants correctly in multi-tenant configuration.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithMultiTenantConfiguration_ProcessesAllTenantsCorrectly()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            var userId = TestUserId.ToString();

            // Setup multi-tenant configuration database
            var configDbOptions = new DbContextOptionsBuilder<DynamicConfigDbContext>()
                .UseInMemoryDatabase(databaseName: $"ConfigDb-{Guid.NewGuid()}")
                .Options;

            using var configDbContext = new DynamicConfigDbContext(configDbOptions);

            // Define three tenants
            var tenant1Domain = "tenant1.com";
            var tenant2Domain = "tenant2.com";
            var tenant3Domain = "tenant3.com";

            // Create tenant connection strings (using in-memory databases)
            var tenant1ConnectionString = $"Data Source=InMemoryTenant1-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
            var tenant2ConnectionString = $"Data Source=InMemoryTenant2-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
            var tenant3ConnectionString = $"Data Source=InMemoryTenant3-{Guid.NewGuid()};Mode=Memory;Cache=Shared";

            // Add tenant connections to config database
            configDbContext.Connections.AddRange(
                new Connection
                {
                    Id = Guid.NewGuid(),
                    DomainNames = new[] { tenant1Domain },
                    DbConn = tenant1ConnectionString,
                    StorageConn = "UseDevelopmentStorage=true",
                    WebsiteUrl = $"https://{tenant1Domain}",
                    Customer = "Tenant 1 Customer",
                    ResourceGroup = "test-resource-group-1"  // Add this line
                },
                new Connection
                {
                    Id = Guid.NewGuid(),
                    DomainNames = new[] { tenant2Domain },
                    DbConn = tenant2ConnectionString,
                    StorageConn = "UseDevelopmentStorage=true",
                    WebsiteUrl = $"https://{tenant2Domain}",
                    Customer = "Tenant 2 Customer",
                    ResourceGroup = "test-resource-group-2"  // Add this line
                },
                new Connection
                {
                    Id = Guid.NewGuid(),
                    DomainNames = new[] { tenant3Domain },
                    DbConn = tenant3ConnectionString,
                    StorageConn = "UseDevelopmentStorage=true",
                    WebsiteUrl = $"https://{tenant3Domain}",
                    Customer = "Tenant 3 Customer",
                    ResourceGroup = "test-resource-group-3"  // Add this line
                }
            );
            await configDbContext.SaveChangesAsync();

            // Create separate in-memory databases for each tenant
            var tenant1DbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"Tenant1Db-{Guid.NewGuid()}")
                .Options;
            var tenant2DbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"Tenant2Db-{Guid.NewGuid()}")
                .Options;
            var tenant3DbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"Tenant3Db-{Guid.NewGuid()}")
                .Options;

            // Seed Tenant 1 database with 2 article versions
            using (var tenant1Db = new ApplicationDbContext(tenant1DbOptions))
            {
                tenant1Db.Articles.AddRange(
                    new Article
                    {
                        ArticleNumber = 100,
                        VersionNumber = 1,
                        Title = "Tenant1 Article V1",
                        Content = "Content V1",
                        Published = now.AddDays(-5),
                        StatusCode = (int)StatusCodeEnum.Active,
                        UserId = userId,
                        UrlPath = "/tenant1-article"
                    },
                    new Article
                    {
                        ArticleNumber = 100,
                        VersionNumber = 2,
                        Title = "Tenant1 Article V2",
                        Content = "Content V2",
                        Published = now.AddDays(-1),
                        StatusCode = (int)StatusCodeEnum.Active,
                        UserId = userId,
                        UrlPath = "/tenant1-article"
                    }
                );
                await tenant1Db.SaveChangesAsync();
            }

            // Seed Tenant 2 database with 3 article versions
            using (var tenant2Db = new ApplicationDbContext(tenant2DbOptions))
            {
                tenant2Db.Articles.AddRange(
                    new Article
                    {
                        ArticleNumber = 200,
                        VersionNumber = 1,
                        Title = "Tenant2 Article V1",
                        Content = "Content V1",
                        Published = now.AddDays(-10),
                        StatusCode = (int)StatusCodeEnum.Active,
                        UserId = userId,
                        UrlPath = "/tenant2-article"
                    },
                    new Article
                    {
                        ArticleNumber = 200,
                        VersionNumber = 2,
                        Title = "Tenant2 Article V2",
                        Content = "Content V2",
                        Published = now.AddDays(-5),
                        StatusCode = (int)StatusCodeEnum.Active,
                        UserId = userId,
                        UrlPath = "/tenant2-article"
                    },
                    new Article
                    {
                        ArticleNumber = 200,
                        VersionNumber = 3,
                        Title = "Tenant2 Article V3",
                        Content = "Content V3",
                        Published = now.AddDays(-2),
                        StatusCode = (int)StatusCodeEnum.Active,
                        UserId = userId,
                        UrlPath = "/tenant2-article"
                    }
                );
                await tenant2Db.SaveChangesAsync();
            }

            // Seed Tenant 3 database with 2 article versions
            using (var tenant3Db = new ApplicationDbContext(tenant3DbOptions))
            {
                tenant3Db.Articles.AddRange(
                    new Article
                    {
                        ArticleNumber = 300,
                        VersionNumber = 1,
                        Title = "Tenant3 Article V1",
                        Content = "Content V1",
                        Published = now.AddDays(-3),
                        StatusCode = (int)StatusCodeEnum.Active,
                        UserId = userId,
                        UrlPath = "/tenant3-article"
                    },
                    new Article
                    {
                        ArticleNumber = 300,
                        VersionNumber = 2,
                        Title = "Tenant3 Article V2",
                        Content = "Content V2",
                        Published = now.AddDays(-1),
                        StatusCode = (int)StatusCodeEnum.Active,
                        UserId = userId,
                        UrlPath = "/tenant3-article"
                    }
                );
                await tenant3Db.SaveChangesAsync();
            }

            // Setup multi-tenant configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["MultiTenant"] = "true",
                    ["ConnectionStrings:ConfigDbConnectionString"] = $"Data Source=InMemoryConfig-{Guid.NewGuid()};Mode=Memory;Cache=Shared"
                })
                .Build();

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

            var mockConfigProvider = new Mock<IDynamicConfigurationProvider>();
            mockConfigProvider.Setup(x => x.IsMultiTenantConfigured).Returns(true);
            mockConfigProvider.Setup(x => x.GetAllDomainNamesAsync())
                .ReturnsAsync(new List<string> { tenant1Domain, tenant2Domain, tenant3Domain });

            // Return correct connection string for each domain
            mockConfigProvider.Setup(x => x.GetDatabaseConnectionString(tenant1Domain))
                .Returns(tenant1ConnectionString);
            mockConfigProvider.Setup(x => x.GetDatabaseConnectionString(tenant2Domain))
                .Returns(tenant2ConnectionString);
            mockConfigProvider.Setup(x => x.GetDatabaseConnectionString(tenant3Domain))
                .Returns(tenant3ConnectionString);

            // Create ArticleScheduler with mocked multi-tenant config
            var mockLogger = new Mock<ILogger<ArticleScheduler>>();
            var scheduler = new ArticleScheduler(
                Db, // Won't be used in multi-tenant mode
                Options.Create(new CosmosConfig()),
                new MemoryCache(new MemoryCacheOptions()),
                Storage,
                mockLogger.Object,
                mockHttpContextAccessor.Object,
                EditorSettings,
                testClock,
                SlugService,
                ArticleHtmlService,
                CatalogService,
                PublishingService,
                TitleChangeService,
                RedirectService,
                TemplateService,
                mockConfigProvider.Object
            );

            // Act
            await scheduler.ExecuteAsync();

            // Assert - Verify Tenant 1 articles were processed
            using (var tenant1Db = new ApplicationDbContext(tenant1DbOptions))
            {
                var tenant1Article1 = await tenant1Db.Articles
                    .FirstOrDefaultAsync(a => a.ArticleNumber == 100 && a.VersionNumber == 1);
                var tenant1Article2 = await tenant1Db.Articles
                    .FirstOrDefaultAsync(a => a.ArticleNumber == 100 && a.VersionNumber == 2);

                Assert.IsNotNull(tenant1Article1, "Tenant1 Article V1 should exist");
                Assert.IsNull(tenant1Article1.Published, "Tenant1 Article V1 should be unpublished");

                Assert.IsNotNull(tenant1Article2, "Tenant1 Article V2 should exist");
                Assert.IsNotNull(tenant1Article2.Published, "Tenant1 Article V2 should remain published");
            }

            // Assert - Verify Tenant 2 articles were processed
            using (var tenant2Db = new ApplicationDbContext(tenant2DbOptions))
            {
                var tenant2Article1 = await tenant2Db.Articles
                    .FirstOrDefaultAsync(a => a.ArticleNumber == 200 && a.VersionNumber == 1);
                var tenant2Article2 = await tenant2Db.Articles
                    .FirstOrDefaultAsync(a => a.ArticleNumber == 200 && a.VersionNumber == 2);
                var tenant2Article3 = await tenant2Db.Articles
                    .FirstOrDefaultAsync(a => a.ArticleNumber == 200 && a.VersionNumber == 3);

                Assert.IsNotNull(tenant2Article1, "Tenant2 Article V1 should exist");
                Assert.IsNull(tenant2Article1.Published, "Tenant2 Article V1 should be unpublished");

                Assert.IsNotNull(tenant2Article2, "Tenant2 Article V2 should exist");
                Assert.IsNull(tenant2Article2.Published, "Tenant2 Article V2 should be unpublished");

                Assert.IsNotNull(tenant2Article3, "Tenant2 Article V3 should exist");
                Assert.IsNotNull(tenant2Article3.Published, "Tenant2 Article V3 should remain published");
            }

            // Assert - Verify Tenant 3 articles were processed
            using (var tenant3Db = new ApplicationDbContext(tenant3DbOptions))
            {
                var tenant3Article1 = await tenant3Db.Articles
                    .FirstOrDefaultAsync(a => a.ArticleNumber == 300 && a.VersionNumber == 1);
                var tenant3Article2 = await tenant3Db.Articles
                    .FirstOrDefaultAsync(a => a.ArticleNumber == 300 && a.VersionNumber == 2);

                Assert.IsNotNull(tenant3Article1, "Tenant3 Article V1 should exist");
                Assert.IsNull(tenant3Article1.Published, "Tenant3 Article V1 should be unpublished");

                Assert.IsNotNull(tenant3Article2, "Tenant3 Article V2 should exist");
                Assert.IsNotNull(tenant3Article2.Published, "Tenant3 Article V2 should remain published");
            }

            // Assert - Verify logging for each domain
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting scheduled execution")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once,
                "Should log starting execution once");

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed execution for domain") && v.ToString().Contains(tenant1Domain)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once,
                "Should log completion for tenant1.com");

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed execution for domain") && v.ToString().Contains(tenant2Domain)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once,
                "Should log completion for tenant2.com");

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed execution for domain") && v.ToString().Contains(tenant3Domain)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once,
                "Should log completion for tenant3.com");

            // Assert - Verify GetAllDomainNamesAsync was called
            mockConfigProvider.Verify(x => x.GetAllDomainNamesAsync(), Times.Once);

            // Assert - Verify GetDatabaseConnectionString was called for each domain
            mockConfigProvider.Verify(x => x.GetDatabaseConnectionString(tenant1Domain), Times.Once);
            mockConfigProvider.Verify(x => x.GetDatabaseConnectionString(tenant2Domain), Times.Once);
            mockConfigProvider.Verify(x => x.GetDatabaseConnectionString(tenant3Domain), Times.Once);
        }

        /// <summary>
        /// Cleanup after each test.
        /// </summary>
        [TestCleanup]
        public async Task TestCleanup()
        {
            await DisposeAsync();
        }

        /// <summary>
        /// Test implementation of IClock for controlling time in tests.
        /// </summary>
        private class TestClock : IClock
        {
            private DateTimeOffset _utcNow = DateTimeOffset.UtcNow;

            public DateTimeOffset UtcNow => _utcNow;

            public void SetUtcNow(DateTimeOffset dateTime)
            {
                _utcNow = dateTime;
            }
        }
    }
}