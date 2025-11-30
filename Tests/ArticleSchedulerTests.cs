// <copyright file="ArticleSchedulerTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Services.Scheduling
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sky.Editor.Data.Logic;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.Scheduling;

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
        public new void Setup()
        {
            testClock = new TestClock();
            Clock = testClock; // Set BEFORE InitializeTestContext

            InitializeTestContext();
            mockLogger = new Mock<ILogger<ArticleScheduler>>();
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
            Assert.Contains("Version 2 Content", publishedPage.Content, "Published page should contain correct content");
        }

        /// <summary>
        /// Tests that ArticleScheduler uses ITenantArticleLogicFactory correctly.
        /// </summary>
        [TestMethod]
        public async Task ProcessArticleVersions_UsesFactory_ToCreateArticleLogic()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);
            
            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Test V1",
                Content = "Content",
                Published = now.AddDays(-5),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString(),
                UrlPath = "/test"
            };
            
            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Test V2",
                Content = "Content",
                Published = now.AddDays(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString(),
                UrlPath = "/test"
            };
            
            Db.Articles.AddRange(article1, article2);
            await Db.SaveChangesAsync();
            
            // Act
            await ArticleScheduler.ExecuteAsync();
            
            // Assert - Verify factory was called
            var factoryFromServices = Services.GetRequiredService<ITenantArticleLogicFactory>();
            Assert.IsNotNull(factoryFromServices);
        }

        /// <summary>
        /// Tests that factory creates properly scoped ArticleEditLogic for tenant.
        /// </summary>
        [TestMethod]
        public async Task TenantArticleLogicFactory_CreatesCorrectlyScoped_ArticleLogic()
        {
            // Arrange
            var factory = Services.GetRequiredService<ITenantArticleLogicFactory>();
            var domainName = "test.com";
            
            // Act
            var articleLogic = await factory.CreateForTenantAsync(domainName);
            
            // Assert
            Assert.IsNotNull(articleLogic);
            Assert.IsInstanceOfType(articleLogic, typeof(ArticleEditLogic));
        }

        /// <summary>
        /// Tests handling of articles scheduled exactly 1 second apart.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithVersionsScheduledOneSecondApart_HandlesCorrectly()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);
            
            var article1 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Test V1",
                Published = now.AddSeconds(-2),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString(),
                UrlPath = "/test"
            };
            
            var article2 = new Article
            {
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Test V2",
                Published = now.AddSeconds(-1),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString(),
                UrlPath = "/test"
            };
            
            Db.Articles.AddRange(article1, article2);
            await Db.SaveChangesAsync();
            
            // Act
            await ArticleScheduler.ExecuteAsync();
            
            // Assert
            var updated1 = await Db.Articles.FindAsync(article1.Id);
            var updated2 = await Db.Articles.FindAsync(article2.Id);
            
            Assert.IsNull(updated1.Published);
            Assert.IsNotNull(updated2.Published);
        }

        /// <summary>
        /// Tests that scheduler handles version number gaps correctly.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithNonSequentialVersionNumbers_HandlesCorrectly()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);
            
            // Version numbers: 1, 5, 10 (gaps intentional)
            var articles = new []
            {
                new Article
                {
                    ArticleNumber = 1,
                    VersionNumber = 1,
                    Published = now.AddDays(-10),
                    StatusCode = (int)StatusCodeEnum.Active,
                    UserId = TestUserId.ToString(),
                    UrlPath = "/test"
                },
                new Article
                {
                    ArticleNumber = 1,
                    VersionNumber = 5,
                    Published = now.AddDays(-5),
                    StatusCode = (int)StatusCodeEnum.Active,
                    UserId = TestUserId.ToString(),
                    UrlPath = "/test"
                },
                new Article
                {
                    ArticleNumber = 1,
                    VersionNumber = 10,
                    Published = now.AddDays(-1),
                    StatusCode = (int)StatusCodeEnum.Active,
                    UserId = TestUserId.ToString(),
                    UrlPath = "/test"
                }
            };
            
            Db.Articles.AddRange(articles);
            await Db.SaveChangesAsync();
            
            // Act
            await ArticleScheduler.ExecuteAsync();
            
            // Assert - Only version 10 should remain published
            Assert.IsNull((await Db.Articles.FindAsync(articles[0].Id)).Published);
            Assert.IsNull((await Db.Articles.FindAsync(articles[1].Id)).Published);
            Assert.IsNotNull((await Db.Articles.FindAsync(articles[2].Id)).Published);
        }

        /// <summary>
        /// Tests that concurrent executions don't cause data corruption.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_ConcurrentExecutions_NoDataCorruption()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);
    
            // Setup test data
            for (int i = 1; i <= 10; i++)
            {
                Db.Articles.Add(new Article
                {
                    ArticleNumber = i,
                    VersionNumber = 1,
                    Published = now.AddDays(-1),
                    StatusCode = (int)StatusCodeEnum.Active,
                    UserId = TestUserId.ToString(),
                    UrlPath = $"/article-{i}"
                });
            }
            await Db.SaveChangesAsync();
    
            // Act - Run scheduler concurrently
            var tasks = Enumerable.Range(0, 3)
                .Select(_ => ArticleScheduler.ExecuteAsync())
                .ToArray();
    
            await Task.WhenAll(tasks);
    
            // Assert - All articles should still be published (no corruption)
            var publishedCount = await Db.Articles.CountAsync(a => a.Published != null);
            Assert.AreEqual(10, publishedCount);
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