// <copyright file="TitleChangeServiceTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Services.Titles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sky.Cms.Models;
    using Sky.Editor.Domain.Events;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.BlogPublishing;
    using Sky.Editor.Services.CDN;
    using Sky.Editor.Services.Publishing;
    using Sky.Editor.Services.Redirects;
    using Sky.Editor.Services.ReservedPaths;
    using Sky.Editor.Services.Slugs;
    using Sky.Editor.Services.Titles;

    /// <summary>
    /// Unit tests for the TitleChangeService class.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class TitleChangeServiceTests : SkyCmsTestBase
    {
        private TitleChangeService titleChangeService;
        private Mock<ISlugService> mockSlugService;
        private Mock<IRedirectService> mockRedirectService;
        private Mock<IClock> mockClock;
        private Mock<IDomainEventDispatcher> mockDispatcher;
        private Mock<IPublishingService> mockPublishingService;
        private Mock<IReservedPaths> mockReservedPaths;
        private Mock<IBlogRenderingService> mockBlogRenderingService;
        private Mock<ILogger<TitleChangeService>> mockLogger;

        [TestInitialize]
        public new void Setup()
        {
            InitializeTestContext();

            // Initialize mocks
            mockSlugService = new Mock<ISlugService>();
            mockRedirectService = new Mock<IRedirectService>();
            mockClock = new Mock<IClock>();
            mockDispatcher = new Mock<IDomainEventDispatcher>();
            mockPublishingService = new Mock<IPublishingService>();
            mockReservedPaths = new Mock<IReservedPaths>();
            mockBlogRenderingService = new Mock<IBlogRenderingService>();
            mockLogger = new Mock<ILogger<TitleChangeService>>();

            // Setup default mock behavior
            mockSlugService.Setup(s => s.Normalize(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string title, string blogKey) =>
                {
                    var slug = title.ToLowerInvariant().Replace(" ", "-");
                    return string.IsNullOrEmpty(blogKey) ? slug : $"{blogKey}/{slug}";
                });

            mockClock.Setup(c => c.UtcNow).Returns(DateTimeOffset.UtcNow);

            mockPublishingService.Setup(p => p.PublishAsync(It.IsAny<Article>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CdnResult>());

            mockReservedPaths.Setup(r => r.GetReservedPaths())
                .ReturnsAsync(new List<ReservedPath>());

            mockBlogRenderingService.Setup(b => b.GenerateBlogStreamHtml(It.IsAny<Article>()))
                .ReturnsAsync("<div>Blog content</div>");

            mockRedirectService.Setup(r => r.CreateOrUpdateRedirectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Article());

            mockDispatcher.Setup(d => d.DispatchAsync(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Create service instance
            titleChangeService = new TitleChangeService(
                Db,
                mockSlugService.Object,
                mockRedirectService.Object,
                mockClock.Object,
                mockDispatcher.Object,
                mockPublishingService.Object,
                mockReservedPaths.Object,
                mockBlogRenderingService.Object,
                mockLogger.Object);
        }

        #region BuildArticleUrl Tests

        [TestMethod]
        public void BuildArticleUrl_GeneralArticle_ReturnsNormalizedTitle()
        {
            // Arrange
            var article = new Article
            {
                Title = "Test Article",
                ArticleType = (int)ArticleType.General
            };

            // Act
            var result = titleChangeService.BuildArticleUrl(article);

            // Assert
            Assert.AreEqual("test-article", result);
        }

        [TestMethod]
        public void BuildArticleUrl_BlogPost_IncludesBlogKey()
        {
            // Arrange
            var article = new Article
            {
                Title = "Blog Post Title",
                ArticleType = (int)ArticleType.BlogPost,
                BlogKey = "my-blog"
            };

            // Act
            var result = titleChangeService.BuildArticleUrl(article);

            // Assert
            Assert.AreEqual("my-blog/blog-post-title", result);
            mockSlugService.Verify(s => s.Normalize("Blog Post Title", "my-blog"), Times.Once);
        }

        #endregion

        #region HandleTitleChangeAsync - General Tests

        [TestMethod]
        public async Task HandleTitleChangeAsync_NoSlugChange_DoesNothing()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "Test Title",
                UrlPath = "test-title",
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString()
            };

            mockSlugService.Setup(s => s.Normalize("Test Title", It.IsAny<string>()))
                .Returns("test-title");

            // Act
            await titleChangeService.HandleTitleChangeAsync(article, "Test Title", "test-title");

            // Assert - No redirects should be created
            mockRedirectService.Verify(
                r => r.CreateOrUpdateRedirectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()),
                Times.Never);
        }

        [TestMethod]
        public async Task HandleTitleChangeAsync_RootPage_PreservesRootUrlPath()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "New Home Title",
                UrlPath = "root",
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddDays(-1)
            };

            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            await titleChangeService.HandleTitleChangeAsync(article, "Old Home Title", "root");

            // Assert
            Assert.AreEqual("root", article.UrlPath, "Root page URL should remain 'root'");
            
            // Verify the overload WITHOUT CancellationToken is called
            mockDispatcher.Verify(
                d => d.DispatchAsync(It.Is<TitleChangedEvent>(e =>
                    e.ArticleNumber == article.ArticleNumber &&
                    e.OldTitle == "Old Home Title" &&
                    e.NewTitle == "New Home Title")),
                Times.Once);
        }

        [TestMethod]
        public async Task HandleTitleChangeAsync_PublishedArticle_CreatesRedirect()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "New Title",
                UrlPath = "old-title",
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddDays(-1)
            };

            mockSlugService.Setup(s => s.Normalize("New Title", It.IsAny<string>()))
                .Returns("new-title");

            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            await titleChangeService.HandleTitleChangeAsync(article, "Old Title", "old-title");

            // Assert
            Assert.AreEqual("new-title", article.UrlPath);
            mockRedirectService.Verify(
                r => r.CreateOrUpdateRedirectAsync("old-title", "new-title", It.IsAny<Guid>()),
                Times.Once);
        }

        [TestMethod]
        public async Task HandleTitleChangeAsync_UnpublishedArticle_DoesNotCreateRedirect()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "New Title",
                UrlPath = "old-title",
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString(),
                Published = null // Unpublished
            };

            mockSlugService.Setup(s => s.Normalize("New Title", It.IsAny<string>()))
                .Returns("new-title");

            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            await titleChangeService.HandleTitleChangeAsync(article, "Old Title", "old-title");

            // Assert
            Assert.AreEqual("new-title", article.UrlPath);
            mockRedirectService.Verify(
                r => r.CreateOrUpdateRedirectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()),
                Times.Never);
        }

        [TestMethod]
        public async Task HandleTitleChangeAsync_SlugConflict_ThrowsException()
        {
            // Arrange
            var existingArticle = new Article
            {
                ArticleNumber = 1,
                Title = "Existing Article",
                UrlPath = "new-title",
                ArticleType = (int)ArticleType.General,
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            var article = new Article
            {
                ArticleNumber = 2,
                Title = "New Title",
                UrlPath = "old-title",
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString()
            };

            mockSlugService.Setup(s => s.Normalize("New Title", It.IsAny<string>()))
                .Returns("new-title");

            Db.Articles.AddRange(existingArticle, article);
            await Db.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(async () =>
                await titleChangeService.HandleTitleChangeAsync(article, "Old Title", "old-title"));
        }

        [TestMethod]
        public async Task HandleTitleChangeAsync_UpdatesAllVersions()
        {
            // Arrange
            var publishedVersion = new Article
            {
                Id = Guid.NewGuid(),
                ArticleNumber = 1,
                VersionNumber = 1,
                Title = "Old Title",
                UrlPath = "old-title",
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddDays(-1)
            };

            var draftVersion = new Article
            {
                Id = Guid.NewGuid(),
                ArticleNumber = 1,
                VersionNumber = 2,
                Title = "Old Title",
                UrlPath = "old-title",
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString(),
                Published = null
            };

            mockSlugService.Setup(s => s.Normalize(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("new-title");

            Db.Articles.AddRange(publishedVersion, draftVersion);
            await Db.SaveChangesAsync();

            // Act
            publishedVersion.Title = "New Title";
            await titleChangeService.HandleTitleChangeAsync(publishedVersion, "Old Title", "old-title");

            // Assert
            var versions = await Db.Articles.Where(a => a.ArticleNumber == 1).ToListAsync();
            Assert.IsTrue(versions.All(v => v.UrlPath == "new-title"), "All versions should have new URL path");
        }

        #endregion

        #region HandleTitleChangeAsync - Blog Stream Tests

        [TestMethod]
        public async Task HandleTitleChangeAsync_BlogStream_UpdatesBlogKey()
        {
            // Arrange
            var blogStream = new Article
            {
                ArticleNumber = 1,
                Title = "New Blog Title",
                UrlPath = "old-blog",
                BlogKey = "old-blog",
                ArticleType = (int)ArticleType.BlogStream,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddDays(-1)
            };

            mockSlugService.Setup(s => s.Normalize("New Blog Title", It.IsAny<string>()))
                .Returns("new-blog");

            Db.Articles.Add(blogStream);
            await Db.SaveChangesAsync();

            // Act
            await titleChangeService.HandleTitleChangeAsync(blogStream, "Old Blog Title", "old-blog");

            // Assert
            Assert.AreEqual("new-blog", blogStream.UrlPath);
            Assert.AreEqual("new-blog", blogStream.BlogKey);
        }

        [TestMethod]
        public async Task HandleTitleChangeAsync_BlogStream_UpdatesAllBlogPosts()
        {
            // Arrange
            var blogStream = new Article
            {
                ArticleNumber = 1,
                Title = "New Blog Title",
                UrlPath = "old-blog",
                BlogKey = "old-blog",
                ArticleType = (int)ArticleType.BlogStream,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddDays(-1)
            };

            var blogPost1 = new Article
            {
                ArticleNumber = 2,
                Title = "Post 1",
                UrlPath = "old-blog/post-1",
                BlogKey = "old-blog",
                ArticleType = (int)ArticleType.BlogPost,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddDays(-1)
            };

            var blogPost2 = new Article
            {
                ArticleNumber = 3,
                Title = "Post 2",
                UrlPath = "old-blog/post-2",
                BlogKey = "old-blog",
                ArticleType = (int)ArticleType.BlogPost,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddDays(-1)
            };

            mockSlugService.Setup(s => s.Normalize("New Blog Title", It.IsAny<string>()))
                .Returns("new-blog");
            mockSlugService.Setup(s => s.Normalize("Post 1", "new-blog"))
                .Returns("new-blog/post-1");
            mockSlugService.Setup(s => s.Normalize("Post 2", "new-blog"))
                .Returns("new-blog/post-2");

            Db.Articles.AddRange(blogStream, blogPost1, blogPost2);
            await Db.SaveChangesAsync();

            // Act
            await titleChangeService.HandleTitleChangeAsync(blogStream, "Old Blog Title", "old-blog");

            // Assert
            var posts = await Db.Articles
                .Where(a => a.ArticleType == (int)ArticleType.BlogPost)
                .ToListAsync();

            Assert.AreEqual(2, posts.Count);
            Assert.IsTrue(posts.All(p => p.BlogKey == "new-blog"), "All posts should have new blog key");
            Assert.IsTrue(posts.All(p => p.UrlPath.StartsWith("new-blog/")), "All posts should have updated URL paths");
        }

        [TestMethod]
        public async Task HandleTitleChangeAsync_BlogStream_RegeneratesHtml()
        {
            // Arrange
            var blogStream = new Article
            {
                ArticleNumber = 1,
                Title = "New Blog Title",
                UrlPath = "old-blog",
                BlogKey = "old-blog",
                ArticleType = (int)ArticleType.BlogStream,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddDays(-1)
            };

            mockSlugService.Setup(s => s.Normalize("New Blog Title", It.IsAny<string>()))
                .Returns("new-blog");

            mockBlogRenderingService.Setup(b => b.GenerateBlogStreamHtml(It.IsAny<Article>()))
                .ReturnsAsync("<div>New Blog Content</div>");

            Db.Articles.Add(blogStream);
            await Db.SaveChangesAsync();

            // Act
            await titleChangeService.HandleTitleChangeAsync(blogStream, "Old Blog Title", "old-blog");

            // Assert
            mockBlogRenderingService.Verify(
                b => b.GenerateBlogStreamHtml(It.Is<Article>(a => a.ArticleNumber == blogStream.ArticleNumber)),
                Times.Once);
            Assert.AreEqual("<div>New Blog Content</div>", blogStream.Content);
        }

        #endregion

        #region HandleTitleChangeAsync - Child Articles Tests

        [TestMethod]
        public async Task HandleTitleChangeAsync_WithChildren_UpdatesChildUrls()
        {
            // Arrange
            var parentArticle = new Article
            {
                ArticleNumber = 1,
                Title = "New Parent Title",
                UrlPath = "old-parent",
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddDays(-1)
            };

            var childArticle = new Article
            {
                ArticleNumber = 2,
                Title = "Child Article",
                UrlPath = "old-parent/child",
                ArticleType = (int)ArticleType.General,
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddDays(-1)
            };

            mockSlugService.Setup(s => s.Normalize("New Parent Title", It.IsAny<string>()))
                .Returns("new-parent");

            Db.Articles.AddRange(parentArticle, childArticle);
            await Db.SaveChangesAsync();

            // Act
            await titleChangeService.HandleTitleChangeAsync(parentArticle, "Old Parent Title", "old-parent");

            // Assert
            var updatedChild = await Db.Articles.FirstAsync(a => a.ArticleNumber == 2);
            Assert.AreEqual("new-parent/child", updatedChild.UrlPath, "Child URL should be updated with new parent slug");
        }

        #endregion

        #region ValidateTitle Tests

        [TestMethod]
        public async Task ValidateTitle_NullTitle_ReturnsFalse()
        {
            // Act
            var result = await titleChangeService.ValidateTitle(null, null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ValidateTitle_EmptyTitle_ReturnsFalse()
        {
            // Act
            var result = await titleChangeService.ValidateTitle(string.Empty, null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ValidateTitle_WhitespaceTitle_ReturnsFalse()
        {
            // Act
            var result = await titleChangeService.ValidateTitle("   ", null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task ValidateTitle_ReservedPath_ReturnsFalse()
        {
            // Arrange
            mockReservedPaths.Setup(r => r.GetReservedPaths())
                .ReturnsAsync(new List<ReservedPath>
                {
                    new ReservedPath { Path = "admin" },
                    new ReservedPath { Path = "api/*" }
                });

            // Act
            var result = await titleChangeService.ValidateTitle("admin", null);

            // Assert
            Assert.IsFalse(result, "Reserved path should not be valid");
        }

        [TestMethod]
        public async Task ValidateTitle_WildcardReservedPath_ReturnsFalse()
        {
            // Arrange
            mockReservedPaths.Setup(r => r.GetReservedPaths())
                .ReturnsAsync(new List<ReservedPath>
                {
                    new ReservedPath { Path = "api/*" }
                });

            // Act
            var result = await titleChangeService.ValidateTitle("api/users", null);

            // Assert
            Assert.IsFalse(result, "Path starting with wildcard reserved prefix should not be valid");
        }

        [TestMethod]
        public async Task ValidateTitle_ExistingTitleDifferentArticle_ReturnsFalse()
        {
            // Arrange
            var existingArticle = new Article
            {
                ArticleNumber = 1,
                Title = "Existing Title",
                UrlPath = "existing-title",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(existingArticle);
            await Db.SaveChangesAsync();

            // Act
            var result = await titleChangeService.ValidateTitle("Existing Title", 2);

            // Assert
            Assert.IsFalse(result, "Title already used by another article should not be valid");
        }

        [TestMethod]
        public async Task ValidateTitle_ExistingTitleSameArticle_ReturnsTrue()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "My Title",
                UrlPath = "my-title",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            var result = await titleChangeService.ValidateTitle("My Title", 1);

            // Assert
            Assert.IsTrue(result, "Same article should be able to keep its own title");
        }

        [TestMethod]
        public async Task ValidateTitle_DeletedArticleTitle_ReturnsTrue()
        {
            // Arrange
            var deletedArticle = new Article
            {
                ArticleNumber = 1,
                Title = "Deleted Title",
                UrlPath = "deleted-title",
                StatusCode = (int)StatusCodeEnum.Deleted,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(deletedArticle);
            await Db.SaveChangesAsync();

            // Act
            var result = await titleChangeService.ValidateTitle("Deleted Title", 2);

            // Assert
            Assert.IsTrue(result, "Title from deleted article should be available for reuse");
        }

        [TestMethod]
        public async Task ValidateTitle_UniqueTitle_ReturnsTrue()
        {
            // Arrange
            mockReservedPaths.Setup(r => r.GetReservedPaths())
                .ReturnsAsync(new List<ReservedPath>());

            // Act
            var result = await titleChangeService.ValidateTitle("Unique New Title", null);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task ValidateTitle_CaseInsensitive_ReturnsFalse()
        {
            // Arrange
            var existingArticle = new Article
            {
                ArticleNumber = 1,
                Title = "My Title",
                UrlPath = "my-title",
                StatusCode = (int)StatusCodeEnum.Active,
                UserId = TestUserId.ToString()
            };

            Db.Articles.Add(existingArticle);
            await Db.SaveChangesAsync();

            // Act
            var result = await titleChangeService.ValidateTitle("MY TITLE", 2);

            // Assert
            Assert.IsFalse(result, "Title validation should be case-insensitive");
        }

        #endregion

        #region Event Dispatching Tests

        [TestMethod]
        public async Task HandleTitleChangeAsync_DispatchesTitleChangedEvent()
        {
            // Arrange
            var article = new Article
            {
                ArticleNumber = 1,
                Title = "New Title",
                UrlPath = "old-title",
                ArticleType = (int)ArticleType.General,
                UserId = TestUserId.ToString(),
                Published = DateTimeOffset.UtcNow.AddDays(-1)
            };

            mockSlugService.Setup(s => s.Normalize("New Title", It.IsAny<string>()))
                .Returns("new-title");

            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            await titleChangeService.HandleTitleChangeAsync(article, "Old Title", "old-title");

            // Assert - Verify the overload WITHOUT CancellationToken is called
            mockDispatcher.Verify(
                d => d.DispatchAsync(It.Is<TitleChangedEvent>(e =>
                    e.ArticleNumber == 1 &&
                    e.OldTitle == "Old Title" &&
                    e.NewTitle == "New Title")),
                Times.Once);
        }

        #endregion
    }
}