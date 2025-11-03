// <copyright file="CatalogServiceTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Services.Catalog
{
    using System;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sky.Editor.Services.Html;

    /// <summary>
    /// Unit tests for the <see cref="CatalogService"/> class.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class CatalogServiceTests : SkyCmsTestBase
    {
        /// <summary>
        /// Initializes test dependencies before each test.
        /// </summary>
        [TestInitialize]
        public void Setup() => InitializeTestContext();

        [TestCleanup]
        public async Task Cleanup()
        {
            await Db.DisposeAsync();
        }

        #region UpsertAsync Tests

        /// <summary>
        /// Tests that UpsertAsync creates a new catalog entry when none exists.
        /// </summary>
        [TestMethod]
        public async Task UpsertAsync_WhenNoExistingEntry_CreatesNewCatalogEntry()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Test Article",
                Introduction = "Test introduction",
                StatusCode = 1,
                BannerImage = "/images/banner.jpg",
                BlogKey = "tech",
                Published = DateTimeOffset.UtcNow,
                Updated = DateTimeOffset.UtcNow,
                UrlPath = "/test-article",
                TemplateId = Guid.NewGuid()
            };

            // Act
            var result = await CatalogService.UpsertAsync(article);

            // Assert
            Assert.IsNotNull(result);
            
            var catalogCount = await Db.ArticleCatalog.CountAsync();
            Assert.AreEqual(1, catalogCount, "Database should contain exactly one catalog entry");
            
            var savedEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 1);
            Assert.IsNotNull(savedEntry, "Catalog entry should exist in database");
            Assert.AreEqual(article.ArticleNumber, savedEntry.ArticleNumber);
            Assert.AreEqual(article.Title, savedEntry.Title);
            Assert.AreEqual(article.Introduction, savedEntry.Introduction);
            Assert.AreEqual("Active", savedEntry.Status);
            Assert.AreEqual(article.BannerImage, savedEntry.BannerImage);
            Assert.AreEqual(article.BlogKey, savedEntry.BlogKey);
        }

        /// <summary>
        /// Tests that UpsertAsync updates existing catalog entry.
        /// </summary>
        [TestMethod]
        public async Task UpsertAsync_WhenExistingEntry_UpdatesCatalogEntry()
        {
            // Arrange
            var existingEntry = new CatalogEntry
            {
                ArticleNumber = 1,
                Title = "Old Title",
                Status = "Active",
                Introduction = "Old introduction",
                UrlPath = "/old-article",
                Updated = DateTimeOffset.UtcNow.AddDays(-1)
            };
            Db.ArticleCatalog.Add(existingEntry);
            await Db.SaveChangesAsync();

            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Updated Title",
                Introduction = "Updated introduction",
                StatusCode = 1,
                BannerImage = "/images/new-banner.jpg",
                BlogKey = "tech",
                Published = DateTimeOffset.UtcNow,
                Updated = DateTimeOffset.UtcNow,
                UrlPath = "/updated-article",
                TemplateId = Guid.NewGuid()
            };

            // Act
            var result = await CatalogService.UpsertAsync(article);

            // Assert
            Assert.IsNotNull(result);
            
            var catalogCount = await Db.ArticleCatalog.CountAsync();
            Assert.AreEqual(1, catalogCount, "Should still have only one entry after update");
            
            var updatedEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 1);
            Assert.AreEqual("Updated Title", updatedEntry.Title);
            Assert.AreEqual("Updated introduction", updatedEntry.Introduction);
        }

        /// <summary>
        /// Tests that UpsertAsync sets status to Inactive when StatusCode is 0.
        /// </summary>
        [TestMethod]
        public async Task UpsertAsync_WhenStatusCodeIsZero_SetsStatusToInactive()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Inactive Article",
                StatusCode = 0,
                UrlPath = "/inactive-article",
                Updated = DateTimeOffset.UtcNow
            };

            // Act
            var result = await CatalogService.UpsertAsync(article);

            // Assert
            Assert.AreEqual("Inactive", result.Status);
            
            var savedEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 1);
            Assert.AreEqual("Inactive", savedEntry.Status);
        }

        /// <summary>
        /// Tests that UpsertAsync sets status to Active when StatusCode is non-zero.
        /// </summary>
        [TestMethod]
        public async Task UpsertAsync_WhenStatusCodeIsNonZero_SetsStatusToActive()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Active Article",
                StatusCode = 1,
                UrlPath = "/active-article",
                Updated = DateTimeOffset.UtcNow
            };

            // Act
            var result = await CatalogService.UpsertAsync(article);

            // Assert
            Assert.AreEqual("Active", result.Status);
            
            var savedEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 1);
            Assert.AreEqual("Active", savedEntry.Status);
        }

        /// <summary>
        /// Tests that UpsertAsync extracts introduction from content when introduction is empty.
        /// </summary>
        [TestMethod]
        public async Task UpsertAsync_WhenIntroductionIsEmpty_ExtractsFromContent()
        {
            // Arrange
            const string htmlContent = "<p>This is extracted introduction from content</p>";
            
            // Create a custom mock for IArticleHtmlService
            var mockHtmlService = new Mock<IArticleHtmlService>();
            mockHtmlService
                .Setup(x => x.ExtractIntroduction(It.IsAny<string>()))
                .Returns("Extracted introduction from content");
            
            // Create a custom CatalogService instance with the mocked HTML service
            var customCatalogService = new Sky.Editor.Services.Catalog.CatalogService(
                Db, 
                mockHtmlService.Object, 
                Clock, 
                new Microsoft.Extensions.Logging.Abstractions.NullLogger<Sky.Editor.Services.Catalog.CatalogService>());

            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Test Article",
                Introduction = string.Empty,
                Content = htmlContent,
                StatusCode = 1,
                UrlPath = "/test-article",
                VersionNumber = 1,
                Updated = DateTimeOffset.UtcNow
            };

            // Add the article to DB so the query can find it
            Db.Articles.Add(new Article
            {
                ArticleNumber = 1,
                VersionNumber = 1,
                Content = htmlContent
            });
            await Db.SaveChangesAsync();

            // Act
            var result = await customCatalogService.UpsertAsync(article);

            // Assert
            Assert.AreEqual("Extracted introduction from content", result.Introduction);
            mockHtmlService.Verify(x => x.ExtractIntroduction(It.IsAny<string>()), Times.Once);
            
            var savedEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 1);
            Assert.AreEqual("Extracted introduction from content", savedEntry.Introduction);
        }

        /// <summary>
        /// Tests that UpsertAsync does not extract introduction when already provided.
        /// </summary>
        [TestMethod]
        public async Task UpsertAsync_WhenIntroductionIsProvided_DoesNotExtractFromContent()
        {
            // Arrange
            const string providedIntro = "Provided introduction";
            
            // Create a custom mock for IArticleHtmlService to verify it's NOT called
            var mockHtmlService = new Mock<IArticleHtmlService>();
            
            var customCatalogService = new Sky.Editor.Services.Catalog.CatalogService(
                Db, 
                mockHtmlService.Object, 
                Clock, 
                new Microsoft.Extensions.Logging.Abstractions.NullLogger<Sky.Editor.Services.Catalog.CatalogService>());

            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Test Article",
                Introduction = providedIntro,
                StatusCode = 1,
                UrlPath = "/test-article",
                Updated = DateTimeOffset.UtcNow
            };

            // Act
            var result = await customCatalogService.UpsertAsync(article);

            // Assert
            Assert.AreEqual(providedIntro, result.Introduction);
            mockHtmlService.Verify(x => x.ExtractIntroduction(It.IsAny<string>()), Times.Never);
            
            var savedEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 1);
            Assert.AreEqual(providedIntro, savedEntry.Introduction);
        }

        /// <summary>
        /// Tests that UpsertAsync sets AuthorInfo to empty string.
        /// </summary>
        [TestMethod]
        public async Task UpsertAsync_SetsAuthorInfoToEmptyString()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Test Article",
                StatusCode = 1,
                UrlPath = "/test-article",
                Updated = DateTimeOffset.UtcNow
            };

            // Act
            var result = await CatalogService.UpsertAsync(article);

            // Assert
            Assert.AreEqual(string.Empty, result.AuthorInfo);
            
            var savedEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 1);
            Assert.AreEqual(string.Empty, savedEntry.AuthorInfo);
        }

        #endregion

        #region DeleteAsync Tests

        /// <summary>
        /// Tests that DeleteAsync removes existing catalog entry.
        /// </summary>
        [TestMethod]
        public async Task DeleteAsync_WhenEntryExists_RemovesEntry()
        {
            // Arrange
            var entry = new CatalogEntry
            {
                ArticleNumber = 1,
                Title = "Test Article",
                UrlPath = "/test-article",
                Status = "Active",
                Updated = DateTimeOffset.UtcNow
            };
            Db.ArticleCatalog.Add(entry);
            await Db.SaveChangesAsync();

            var countBefore = await Db.ArticleCatalog.CountAsync();
            Assert.AreEqual(1, countBefore, "Should have one entry before delete");

            // Act
            await CatalogService.DeleteAsync(1);

            // Assert
            var countAfter = await Db.ArticleCatalog.CountAsync();
            Assert.AreEqual(0, countAfter, "Database should be empty after delete");
            
            var deletedEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 1);
            Assert.IsNull(deletedEntry, "Entry should no longer exist in database");
        }

        /// <summary>
        /// Tests that DeleteAsync handles non-existent entry gracefully.
        /// </summary>
        [TestMethod]
        public async Task DeleteAsync_WhenEntryDoesNotExist_DoesNotThrowException()
        {
            // Arrange - no entries in catalog
            var countBefore = await Db.ArticleCatalog.CountAsync();
            Assert.AreEqual(0, countBefore, "Database should be empty initially");

            // Act - should not throw
            await CatalogService.DeleteAsync(999);

            // Assert
            var countAfter = await Db.ArticleCatalog.CountAsync();
            Assert.AreEqual(0, countAfter, "Database should still be empty");
        }

        /// <summary>
        /// Tests that DeleteAsync is idempotent.
        /// </summary>
        [TestMethod]
        public async Task DeleteAsync_IsIdempotent()
        {
            // Arrange
            var entry = new CatalogEntry
            {
                ArticleNumber = 1,
                Title = "Test Article",
                UrlPath = "/test-article",
                Status = "Active",
                Updated = DateTimeOffset.UtcNow
            };
            Db.ArticleCatalog.Add(entry);
            await Db.SaveChangesAsync();

            // Act
            await CatalogService.DeleteAsync(1);
            await CatalogService.DeleteAsync(1); // Call again

            // Assert
            var count = await Db.ArticleCatalog.CountAsync();
            Assert.AreEqual(0, count, "Database should be empty after both delete operations");
        }

        #endregion

        #region Edge Case Tests

        /// <summary>
        /// Tests that UpsertAsync handles null template ID.
        /// </summary>
        [TestMethod]
        public async Task UpsertAsync_WhenTemplateIdIsNull_HandlesGracefully()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Test Article",
                StatusCode = 1,
                UrlPath = "/test-article",
                TemplateId = null,
                Updated = DateTimeOffset.UtcNow
            };

            // Act
            var result = await CatalogService.UpsertAsync(article);

            // Assert
            Assert.IsNull(result.TemplateId);
            
            var savedEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 1);
            Assert.IsNull(savedEntry.TemplateId);
        }

        /// <summary>
        /// Tests that UpsertAsync handles null published date.
        /// </summary>
        [TestMethod]
        public async Task UpsertAsync_WhenPublishedIsNull_HandlesGracefully()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Draft Article",
                StatusCode = 0,
                UrlPath = "/draft-article",
                Published = null,
                Updated = DateTimeOffset.UtcNow
            };

            // Act
            var result = await CatalogService.UpsertAsync(article);

            // Assert
            Assert.IsNull(result.Published);
            
            var savedEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 1);
            Assert.IsNull(savedEntry.Published);
        }

        /// <summary>
        /// Tests that UpsertAsync preserves all article properties correctly.
        /// </summary>
        [TestMethod]
        public async Task UpsertAsync_PreservesAllArticleProperties()
        {
            // Arrange
            var templateId = Guid.NewGuid();
            var publishedDate = DateTimeOffset.UtcNow.AddDays(-1);
            var updatedDate = DateTimeOffset.UtcNow;

            var article = new Article
            {
                ArticleNumber = 42,
                Title = "Complete Article",
                Introduction = "Full introduction",
                BannerImage = "/images/banner.jpg",
                BlogKey = "technology",
                Published = publishedDate,
                Updated = updatedDate,
                UrlPath = "/complete-article",
                TemplateId = templateId,
                StatusCode = 1
            };

            // Act
            var result = await CatalogService.UpsertAsync(article);

            // Assert - verify returned object
            Assert.AreEqual(42, result.ArticleNumber);
            Assert.AreEqual("Complete Article", result.Title);
            Assert.AreEqual("Full introduction", result.Introduction);
            Assert.AreEqual("/images/banner.jpg", result.BannerImage);
            Assert.AreEqual("technology", result.BlogKey);
            Assert.AreEqual(publishedDate, result.Published);
            Assert.AreEqual(updatedDate, result.Updated);
            Assert.AreEqual("/complete-article", result.UrlPath);
            Assert.AreEqual(templateId, result.TemplateId);
            Assert.AreEqual("Active", result.Status);

            // Assert - verify database persistence
            var savedEntry = await Db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == 42);
            Assert.IsNotNull(savedEntry, "Entry should be saved in database");
            Assert.AreEqual(42, savedEntry.ArticleNumber);
            Assert.AreEqual("Complete Article", savedEntry.Title);
            Assert.AreEqual("Full introduction", savedEntry.Introduction);
            Assert.AreEqual("/images/banner.jpg", savedEntry.BannerImage);
            Assert.AreEqual("technology", savedEntry.BlogKey);
            Assert.AreEqual(publishedDate, savedEntry.Published);
            Assert.AreEqual(updatedDate, savedEntry.Updated);
            Assert.AreEqual("/complete-article", savedEntry.UrlPath);
            Assert.AreEqual(templateId, savedEntry.TemplateId);
            Assert.AreEqual("Active", savedEntry.Status);
        }

        #endregion
    }
}