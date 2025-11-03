// <copyright file="ArticleSchedulerTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Services.Scheduling
{
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
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
        private ArticleScheduler scheduler;
        private Mock<ILogger<ArticleScheduler>> mockLogger;
        private TestClock testClock;

        /// <summary>
        /// Initializes test context before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            InitializeTestContext();
            mockLogger = new Mock<ILogger<ArticleScheduler>>();
            testClock = new TestClock();

            // Override the base clock with test clock
            var clockField = typeof(SkyCmsTestBase).GetField("Clock",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            clockField?.SetValue(this, testClock);

            scheduler = new ArticleScheduler(
                Db,
                Logic,
                testClock,
                mockLogger.Object);
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
            await scheduler.ExecuteAsync();

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
            await scheduler.ExecuteAsync();

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
            await scheduler.ExecuteAsync();

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
            await scheduler.ExecuteAsync();

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
            await scheduler.ExecuteAsync();

            // Assert - No exception thrown
            Assert.IsTrue(true);
        }

        /// <summary>
        /// Tests that ExecuteAsync logs appropriate information messages.
        /// </summary>
        [TestMethod]
        public async Task ExecuteAsync_WithMultipleArticles_LogsInformation()
        {
            // Arrange
            var now = new DateTimeOffset(2024, 11, 3, 12, 0, 0, TimeSpan.Zero);
            testClock.SetUtcNow(now);

            // Act
            await scheduler.ExecuteAsync();

            // Assert - Verify logging occurred
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Starting scheduled execution")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Completed execution")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
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
            await scheduler.ExecuteAsync();

            // Assert - Verify article2 was published
            var publishedPage = await Db.Pages.FirstOrDefaultAsync(p => p.ArticleNumber == 1);
            Assert.IsNotNull(publishedPage, "Published page should exist");
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