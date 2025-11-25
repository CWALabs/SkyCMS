// <copyright file="TitleChangeServiceTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Services.Titles
{
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sky.Editor.Domain.Events;
    using Sky.Editor.Services.Titles;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Unit tests for the <see cref="TitleChangeService"/> class.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class TitleChangeServiceTests : SkyCmsTestBase
    {
        [TestInitialize]
        public void Setup() => InitializeTestContext();

        #region BuildArticleUrl Tests

        /// <summary>
        /// Tests that BuildArticleUrl returns normalized title for standard articles.
        /// </summary>
        [TestMethod]
        public void BuildArticleUrl_StandardArticle_ReturnsNormalizedTitle()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Test Article",
                ArticleType = (int)ArticleType.General
            };

            // Act
            var result = TitleChangeService.BuildArticleUrl(article);

            // Assert
            Assert.AreEqual("test-article", result);
        }

        /// <summary>
        /// Tests that BuildArticleUrl includes blog key for blog posts.
        /// </summary>
        [TestMethod]
        public void BuildArticleUrl_BlogPost_ReturnsNormalizedTitleWithBlogKey()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "My Blog Post",
                ArticleType = (int)ArticleType.BlogPost,
                BlogKey = "my-blog"
            };

            // Act
            var result = TitleChangeService.BuildArticleUrl(article);

            // Assert
            Assert.IsTrue(result.Contains("my-blog"));
            Assert.IsTrue(result.Contains("my-blog-post"));
        }

        /// <summary>
        /// Tests that BuildArticleUrl returns normalized title for blog stream articles.
        /// </summary>
        [TestMethod]
        public void BuildArticleUrl_BlogStream_ReturnsNormalizedTitle()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "My Blog Stream",
                ArticleType = (int)ArticleType.BlogStream
            };

            // Act
            var result = TitleChangeService.BuildArticleUrl(article);

            // Assert
            Assert.AreEqual("my-blog-stream", result);
        }

        #endregion

        #region HandleTitleChangeAsync Tests

        /// <summary>
        /// Tests that HandleTitleChangeAsync does nothing when slug hasn't changed.
        /// </summary>
        [TestMethod]
        public async Task HandleTitleChangeAsync_UnchangedSlug_DoesNothing()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Test Article",
                ArticleType = (int)ArticleType.General,
                UrlPath = "test-article",
                UserId = TestUserId.ToString(),
                StatusCode = (int)StatusCodeEnum.Active
            };

            Db.Articles.Add(article);
            await Db.SaveChangesAsync();
            EventDispatcher.Clear();

            // Act
            await TitleChangeService.HandleTitleChangeAsync(article, "Test Article", "test-article");

            // Assert - no title changed event should have been dispatched
            var titleChangedEvent = EventDispatcher.Last<TitleChangedEvent>();
            Assert.IsNull(titleChangedEvent);
        }

        /// <summary>
        /// Tests that HandleTitleChangeAsync throws when slug conflicts with existing article.
        /// </summary>
        [TestMethod]
        public async Task HandleTitleChangeAsync_SlugConflict_ThrowsInvalidOperationException()
        {
            // Arrange
            var existingArticle = new Article
            {
                ArticleNumber = 2,
                Title = "New Title",
                UrlPath = "new-title",
                StatusCode = (int)StatusCodeEnum.Active,
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString()
            };

            var article = new Article
            {
                ArticleNumber = 1,
                Title = "New Title",
                ArticleType = (int)ArticleType.General,
                UrlPath = "old-title",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(existingArticle);
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act & Assert - should throw
            await Assert.ThrowsAsync<InvalidOperationException>(
                async () => await TitleChangeService.HandleTitleChangeAsync(article, "Old Title", "old-title"));
        }

        /// <summary>
        /// Tests that HandleTitleChangeAsync updates article URL and saves changes.
        /// </summary>
        [TestMethod]
        public async Task HandleTitleChangeAsync_ValidSlugChange_UpdatesArticleAndSaves()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "New Title",
                ArticleType = (int)ArticleType.General,
                UrlPath = "old-title",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(article);
            await Db.SaveChangesAsync();
            EventDispatcher.Clear();

            // Act
            await TitleChangeService.HandleTitleChangeAsync(article, "Old Title", "old-title");

            // Assert
            Assert.AreEqual("new-title", article.UrlPath);

            var titleChangedEvent = EventDispatcher.Last<TitleChangedEvent>();
            Assert.IsNotNull(titleChangedEvent);
        }

        /// <summary>
        /// Tests that HandleTitleChangeAsync publishes article if it's currently published.
        /// </summary>
        [TestMethod]
        public async Task HandleTitleChangeAsync_PublishedArticle_RepublishesArticle()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;

            var article = new Article
            {
                ArticleNumber = 1,
                Title = "New Title",
                ArticleType = (int)ArticleType.General,
                UrlPath = "old-title",
                Published = DateTimeOffset.UtcNow.AddMinutes(-10),
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            await TitleChangeService.HandleTitleChangeAsync(article, "Old Title", "old-title");

            // Assert
            Assert.AreEqual("new-title", article.UrlPath);

            // Verify the article was updated in the database
            var updatedArticle = await Db.Articles.FirstOrDefaultAsync(a => a.ArticleNumber == 1);
            Assert.IsNotNull(updatedArticle);
            Assert.AreEqual("new-title", updatedArticle.UrlPath);
        }

        /// <summary>
        /// Tests that HandleTitleChangeAsync creates redirects for changed URLs.
        /// </summary>
        [TestMethod]
        public async Task HandleTitleChangeAsync_ChangedUrl_CreatesRedirects()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "New Title",
                ArticleType = (int)ArticleType.General,
                UrlPath = "old-title",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddMinutes(-10) // ✅ ADD THIS LINE
            };

            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            await TitleChangeService.HandleTitleChangeAsync(article, "Old Title", "old-title");

            // Assert - CORRECTED
            var redirect = await Db.Articles.FirstOrDefaultAsync(a =>
                a.StatusCode == (int)StatusCodeEnum.Redirect &&
                a.UrlPath == "old-title");

            Assert.IsNotNull(redirect, "Redirect should have been created");
            Assert.Contains("/new-title", redirect.Content);
        }

        /// <summary>
        /// Tests that HandleTitleChangeAsync cascades changes to blog posts when blog stream title changes.
        /// </summary>
        [TestMethod]
        public async Task HandleTitleChangeAsync_BlogStreamTitleChange_CascadesToBlogPosts()
        {
            // Arrange
            var blogStream = new Article
            {
                ArticleNumber = 1,
                Title = "New Blog Title",
                ArticleType = (int)ArticleType.BlogStream,
                UrlPath = "old-blog",
                BlogKey = "old-blog",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            var blogPost1 = new Article
            {
                ArticleNumber = 2,
                Title = "Post Title One",
                ArticleType = (int)ArticleType.BlogPost,
                UrlPath = "old-blog/post-title-one",
                BlogKey = "old-blog",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            var blogPost2 = new Article
            {
                ArticleNumber = 3,
                Title = "Post Title Two",
                ArticleType = (int)ArticleType.BlogPost,
                UrlPath = "old-blog/post-title-two",
                BlogKey = "old-blog",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(blogStream);
            Db.Articles.Add(blogPost1);
            Db.Articles.Add(blogPost2);
            await Db.SaveChangesAsync();
            EventDispatcher.Clear();

            // Act
            await TitleChangeService.HandleTitleChangeAsync(blogStream, "Old Blog", "old-blog");

            // Assert
            Assert.AreEqual("new-blog-title", blogStream.BlogKey);
            Assert.AreEqual("new-blog-title", blogStream.UrlPath);

            var updatedPost1 = await Db.Articles.FirstOrDefaultAsync(a => a.ArticleNumber == 2);
            Assert.IsNotNull(updatedPost1);
            Assert.AreEqual("new-blog-title", updatedPost1.BlogKey);
            Assert.IsTrue(updatedPost1.UrlPath.StartsWith("new-blog-title/"));

            var updatedPost2 = await Db.Articles.FirstOrDefaultAsync(a => a.ArticleNumber == 3);
            Assert.IsNotNull(updatedPost2);
            Assert.AreEqual("new-blog-title", updatedPost2.BlogKey);
            Assert.IsTrue(updatedPost2.UrlPath.StartsWith("new-blog-title/"));

            // Verify event was dispatched
            var titleChangedEvent = EventDispatcher.Last<TitleChangedEvent>();
            Assert.IsNotNull(titleChangedEvent);
        }

        /// <summary>
        /// Tests that HandleTitleChangeAsync synchronizes all article versions.
        /// </summary>
        [TestMethod]
        public async Task HandleTitleChangeAsync_MultipleVersions_SynchronizesAll()
        {
            // Arrange
            var article1 = new Article
            {
                Id = Guid.NewGuid(),
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "New Title",
                ArticleType = (int)ArticleType.General,
                UrlPath = "old-title",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            var article2 = new Article
            {
                Id = Guid.NewGuid(),
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "New Title",
                ArticleType = (int)ArticleType.General,
                UrlPath = "old-title",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            var article3 = new Article
            {
                Id = Guid.NewGuid(),
                ArticleNumber = 1,
                VersionNumber = 3,
                Title = "New Title",
                ArticleType = (int)ArticleType.General,
                UrlPath = "old-title",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(article1);
            Db.Articles.Add(article2);
            Db.Articles.Add(article3);
            await Db.SaveChangesAsync();

            // Act
            await TitleChangeService.HandleTitleChangeAsync(article3, "Old Title", "old-title");

            // Assert
            var version1 = await Db.Articles.FirstOrDefaultAsync(a => a.ArticleNumber == 1 && a.VersionNumber == 1);
            var version2 = await Db.Articles.FirstOrDefaultAsync(a => a.ArticleNumber == 1 && a.VersionNumber == 2);
            var version3 = await Db.Articles.FirstOrDefaultAsync(a => a.ArticleNumber == 1 && a.VersionNumber == 3);

            Assert.AreEqual("new-title", version1.UrlPath);
            Assert.AreEqual("new-title", version2.UrlPath);
            Assert.AreEqual("new-title", version3.UrlPath);
        }

        /// <summary>
        /// Tests that HandleTitleChangeAsync updates blog key for blog stream articles.
        /// </summary>
        [TestMethod]
        public async Task HandleTitleChangeAsync_BlogStreamSlugChange_UpdatesBlogKey()
        {
            // Arrange
            var blogStream = new Article
            {
                ArticleNumber = 1,
                Title = "New Blog Stream",
                ArticleType = (int)ArticleType.BlogStream,
                UrlPath = "old-blog-stream",
                BlogKey = "old-blog-stream",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(blogStream);
            await Db.SaveChangesAsync();

            // Act
            await TitleChangeService.HandleTitleChangeAsync(blogStream, "Old Blog Stream", "old-blog-stream");

            // Assert
            Assert.AreEqual("new-blog-stream", blogStream.UrlPath);
            Assert.AreEqual("new-blog-stream", blogStream.BlogKey);
        }

        /// <summary>
        /// Tests that HandleTitleChangeAsync updates blog key for blog post articles.
        /// </summary>
        [TestMethod]
        public async Task HandleTitleChangeAsync_BlogPostSlugChange_UpdatesBlogKey()
        {
            // Arrange
            var blogPost = new Article
            {
                ArticleNumber = 1,
                Title = "New Post Title",
                ArticleType = (int)ArticleType.BlogPost,
                UrlPath = "my-blog/old-post-title",
                BlogKey = "my-blog",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(blogPost);
            await Db.SaveChangesAsync();

            // Act
            await TitleChangeService.HandleTitleChangeAsync(blogPost, "Old Post Title", "old-post-title");

            // Assert
            Assert.AreEqual("my-blog/new-post-title", blogPost.UrlPath);
            Assert.AreEqual("my-blog", blogPost.BlogKey);
        }

        #endregion

        #region ValidateTitle Tests

        /// <summary>
        /// Tests that ValidateTitle returns false for null or whitespace titles.
        /// </summary>
        [TestMethod]
        public async Task ValidateTitle_NullOrWhitespace_ReturnsFalse()
        {
            // Act & Assert
            Assert.IsFalse(await TitleChangeService.ValidateTitle(null, null));
            Assert.IsFalse(await TitleChangeService.ValidateTitle(string.Empty, null));
            Assert.IsFalse(await TitleChangeService.ValidateTitle("   ", null));
        }

        /// <summary>
        /// Tests that ValidateTitle returns false when title conflicts with existing article.
        /// </summary>
        [TestMethod]
        public async Task ValidateTitle_ConflictingTitle_ReturnsFalse()
        {
            // Arrange
            var existingArticle = new Article
            {
                ArticleNumber = 2,
                Title = "Existing Title",
                UrlPath = "existing-title",
                StatusCode = (int)StatusCodeEnum.Active,
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(existingArticle);
            await Db.SaveChangesAsync();

            // Act
            var result = await TitleChangeService.ValidateTitle("Existing Title", 1);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that ValidateTitle returns true for valid, unique title.
        /// </summary>
        [TestMethod]
        public async Task ValidateTitle_ValidUniqueTitle_ReturnsTrue()
        {
            // Act
            var result = await TitleChangeService.ValidateTitle("Valid New Title", null);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that ValidateTitle allows same title for the same article (edit scenario).
        /// </summary>
        [TestMethod]
        public async Task ValidateTitle_SameTitleForSameArticle_ReturnsTrue()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "My Title",
                UrlPath = "my-title",
                StatusCode = (int)StatusCodeEnum.Active,
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            var result = await TitleChangeService.ValidateTitle("My Title", 1);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that ValidateTitle returns false for titles matching reserved paths with wildcard.
        /// </summary>
        [TestMethod]
        public async Task ValidateTitle_ReservedPathWithWildcard_ReturnsFalse()
        {
            // Act - ReservedPaths service should include paths like "pub/*" in test setup
            var result = await TitleChangeService.ValidateTitle("pub/test", null);

            // Assert
            Assert.IsFalse(result, "Title starting with 'pub' should be rejected as reserved path");
        }

        /// <summary>
        /// Tests that ValidateTitle returns false for exact match with reserved path.
        /// </summary>
        [TestMethod]
        public async Task ValidateTitle_ExactReservedPath_ReturnsFalse()
        {
            // Act - ReservedPaths service should include exact paths like "editor"
            var result = await TitleChangeService.ValidateTitle("editor", null);

            // Assert
            Assert.IsFalse(result, "Title 'editor' should be rejected as reserved path");
        }

        /// <summary>
        /// Tests that ValidateTitle is case-insensitive for title conflicts.
        /// </summary>
        [TestMethod]
        public async Task ValidateTitle_CaseInsensitiveConflict_ReturnsFalse()
        {
            // Arrange
            var existingArticle = new Article
            {
                ArticleNumber = 2,
                Title = "My Article",
                UrlPath = "my-article",
                StatusCode = (int)StatusCodeEnum.Active,
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(existingArticle);
            await Db.SaveChangesAsync();

            // Act
            var result = await TitleChangeService.ValidateTitle("MY ARTICLE", 1);

            // Assert
            Assert.IsFalse(result, "Title validation should be case-insensitive");
        }

        /// <summary>
        /// Tests that ValidateTitle ignores deleted articles when checking conflicts.
        /// </summary>
        [TestMethod]
        public async Task ValidateTitle_DeletedArticleWithSameTitle_ReturnsTrue()
        {
            // Arrange
            var deletedArticle = new Article
            {
                ArticleNumber = 2,
                Title = "Deleted Article",
                UrlPath = "deleted-article",
                StatusCode = (int)StatusCodeEnum.Deleted,
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(deletedArticle);
            await Db.SaveChangesAsync();

            // Act
            var result = await TitleChangeService.ValidateTitle("Deleted Article", 1);

            // Assert
            Assert.IsTrue(result, "Deleted articles should not cause title conflicts");
        }

        #endregion
    }
}