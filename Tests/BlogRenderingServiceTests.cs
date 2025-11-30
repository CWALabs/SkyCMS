// <copyright file="BlogRenderingServiceTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Services.BlogPublishing
{
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sky.Editor.Services.BlogPublishing;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Unit tests for the <see cref="BlogRenderingService"/> class.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class BlogRenderingServiceTests : SkyCmsTestBase
    {
        private BlogRenderingService service = null!;

        [TestInitialize]
        public new void Setup()
        {
            InitializeTestContext(seedLayout: true);
            service = new BlogRenderingService(Db);
        }

        [TestCleanup]
        public void Cleanup() => Db.Dispose();

        #region GenerateBlogStreamHtml Tests

        /// <summary>
        /// Verifies that GenerateBlogStreamHtml returns HTML when template exists.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogStreamHtml_TemplateExists_ReturnsHtml()
        {
            // Arrange
            await SeedBlogStreamTemplate();
            var article = CreateBlogStreamArticle();
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            var result = await service.GenerateBlogStreamHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains(article.Title, result, "Result should contain the article title.");
            Assert.Contains(article.Introduction, result, "Result should contain the article introduction.");
        }

        /// <summary>
        /// Verifies that GenerateBlogStreamHtml handles banner image correctly.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogStreamHtml_WithBannerImage_SetsSrcAttribute()
        {
            // Arrange
            await SeedBlogStreamTemplate();
            var article = CreateBlogStreamArticle();
            article.BannerImage = "https://example.com/banner.jpg";
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            var result = await service.GenerateBlogStreamHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains(article.BannerImage, result, "Result should contain banner image src.");
            Assert.DoesNotContain("display:none", result, "Banner should not be hidden when image exists.");
        }

        /// <summary>
        /// Verifies that GenerateBlogStreamHtml hides banner when image is empty.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogStreamHtml_NoBannerImage_HidesBanner()
        {
            // Arrange
            await SeedBlogStreamTemplate();
            var article = CreateBlogStreamArticle();
            article.BannerImage = string.Empty;
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            var result = await service.GenerateBlogStreamHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains("display:none", result, "Banner should be hidden when no image.");
        }

        /// <summary>
        /// Verifies that GenerateBlogStreamHtml retrieves published blog entries.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogStreamHtml_RetrievesPublishedEntries_CorrectlyRendersEntries()
        {
            // Arrange
            await SeedBlogStreamTemplate();
            var blogKey = "test-blog";
            var article = CreateBlogStreamArticle(blogKey);
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            var entry1 = await CreateBlogEntry(blogKey, "Entry 1", "<p>Content 1</p>", DateTimeOffset.UtcNow.AddDays(-2));
            var entry2 = await CreateBlogEntry(blogKey, "Entry 2", "<p>Content 2</p>", DateTimeOffset.UtcNow.AddDays(-1));

            // Act
            var result = await service.GenerateBlogStreamHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains("Entry 1", result, "Result should contain first entry title.");
            Assert.Contains("Entry 2", result, "Result should contain second entry title.");
        }

        /// <summary>
        /// Verifies that GenerateBlogStreamHtml limits results to 10 entries.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogStreamHtml_MoreThan10Entries_ReturnsTop10()
        {
            // Arrange
            await SeedBlogStreamTemplate();
            var blogKey = "test-blog";
            var article = CreateBlogStreamArticle(blogKey);
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Create 15 blog entries
            for (int i = 1; i <= 15; i++)
            {
                await CreateBlogEntry(blogKey, $"Entry {i}", $"<p>Content {i}</p>", DateTimeOffset.UtcNow.AddDays(-i));
            }

            // Act
            var result = await service.GenerateBlogStreamHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");

            // Most recent 10 should be present (Entry 1 through Entry 10)
            for (int i = 1; i <= 10; i++)
            {
                Assert.Contains($"Entry {i}", result, $"Result should contain Entry {i}.");
            }

            // Older entries should not be present
            Assert.DoesNotContain("Entry 11", result, "Result should not contain Entry 11.");
            Assert.DoesNotContain("Entry 15", result, "Result should not contain Entry 15.");
        }

        /// <summary>
        /// Verifies that GenerateBlogStreamHtml excludes deleted entries.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogStreamHtml_ExcludesDeletedEntries_OnlyReturnsActive()
        {
            // Arrange
            await SeedBlogStreamTemplate();
            var blogKey = "test-blog";
            var article = CreateBlogStreamArticle(blogKey);
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            var activeEntry = await CreateBlogEntry(blogKey, "Active Entry", "<p>Active Content</p>", DateTimeOffset.UtcNow.AddDays(-1));
            var deletedEntry = await CreateBlogEntry(blogKey, "Deleted Entry", "<p>Deleted Content</p>", DateTimeOffset.UtcNow.AddDays(-2), isDeleted: true);

            // Act
            var result = await service.GenerateBlogStreamHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains("Active Entry", result, "Result should contain active entry.");
            Assert.DoesNotContain("Deleted Entry", result, "Result should not contain deleted entry.");
        }

        /// <summary>
        /// Verifies that GenerateBlogStreamHtml excludes redirect entries.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogStreamHtml_ExcludesRedirectEntries_OnlyReturnsRegularContent()
        {
            // Arrange
            await SeedBlogStreamTemplate();
            var blogKey = "test-blog";
            var article = CreateBlogStreamArticle(blogKey);
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            var regularEntry = await CreateBlogEntry(blogKey, "Regular Entry", "<p>Regular Content</p>", DateTimeOffset.UtcNow.AddDays(-1));
            var redirectEntry = await CreateBlogEntry(blogKey, "Redirect Entry", "<p>Redirect Content</p>", DateTimeOffset.UtcNow.AddDays(-2), isRedirect: true);

            // Act
            var result = await service.GenerateBlogStreamHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains("Regular Entry", result, "Result should contain regular entry.");
            Assert.DoesNotContain("Redirect Entry", result, "Result should not contain redirect entry.");
        }

        /// <summary>
        /// Verifies that GenerateBlogStreamHtml excludes future-published entries.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogStreamHtml_ExcludesFuturePublishedEntries_OnlyReturnsCurrentAndPast()
        {
            // Arrange
            await SeedBlogStreamTemplate();
            var blogKey = "test-blog";
            var article = CreateBlogStreamArticle(blogKey);
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            var pastEntry = await CreateBlogEntry(blogKey, "Past Entry", "<p>Past Content</p>", DateTimeOffset.UtcNow.AddDays(-1));
            var futureEntry = await CreateBlogEntry(blogKey, "Future Entry", "<p>Future Content</p>", DateTimeOffset.UtcNow.AddDays(1));

            // Act
            var result = await service.GenerateBlogStreamHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains("Past Entry", result, "Result should contain past entry.");
            Assert.DoesNotContain("Future Entry", result, "Result should not contain future entry.");
        }

        /// <summary>
        /// Verifies that GenerateBlogStreamHtml extracts introduction from first paragraph when introduction is empty.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogStreamHtml_EmptyIntroduction_ExtractsFromFirstParagraph()
        {
            // Arrange
            await SeedBlogStreamTemplate();
            var blogKey = "test-blog";
            var article = CreateBlogStreamArticle(blogKey);
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            var entry = await CreateBlogEntry(
                blogKey,
                "Test Entry",
                "<p>This is the first paragraph.</p><p>This is the second paragraph.</p>",
                DateTimeOffset.UtcNow.AddDays(-1),
                introduction: string.Empty);

            // Act
            var result = await service.GenerateBlogStreamHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");

            // Note: The service modifies the entry in-memory but does NOT save to database
            // So we verify the returned HTML contains the extracted text, not the database record
            Assert.Contains("This is the first paragraph.",
result, "Result should contain the extracted introduction text.");
        }

        /// <summary>
        /// Verifies that GenerateBlogStreamHtml handles entries with no blog key match.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogStreamHtml_DifferentBlogKey_ExcludesEntries()
        {
            // Arrange
            await SeedBlogStreamTemplate();
            var article = CreateBlogStreamArticle("blog-a");
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            var blogAEntry = await CreateBlogEntry("blog-a", "Blog A Entry", "<p>Content A</p>", DateTimeOffset.UtcNow.AddDays(-1));
            var blogBEntry = await CreateBlogEntry("blog-b", "Blog B Entry", "<p>Content B</p>", DateTimeOffset.UtcNow.AddDays(-1));

            // Act
            var result = await service.GenerateBlogStreamHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains("Blog A Entry", result, "Result should contain blog A entry.");
            Assert.DoesNotContain("Blog B Entry", result, "Result should not contain blog B entry.");
        }

        /// <summary>
        /// Verifies that GenerateBlogStreamHtml handles empty blog with no entries gracefully.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogStreamHtml_NoEntries_ReturnsEmptyContainer()
        {
            // Arrange
            await SeedBlogStreamTemplate();
            var article = CreateBlogStreamArticle("empty-blog");
            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            // Act
            var result = await service.GenerateBlogStreamHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains(article.Title, result, "Result should contain the article title.");
            Assert.Contains("blog-items", result, "Result should contain the blog-items container.");
        }

        #endregion

        #region GenerateBlogEntryHtml Tests

        /// <summary>
        /// Verifies that GenerateBlogEntryHtml returns HTML when template exists.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogEntryHtml_TemplateExists_ReturnsHtml()
        {
            // Arrange
            await SeedBlogPostTemplate();
            var article = CreateBlogEntryArticle();

            // Act
            var result = await service.GenerateBlogEntryHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains(article.Title, result, "Result should contain the article title.");
            Assert.Contains(article.Content, result, "Result should contain the article content.");
        }

        /// <summary>
        /// Verifies that GenerateBlogEntryHtml handles banner image correctly.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogEntryHtml_WithBannerImage_AddsImageElement()
        {
            // Arrange
            await SeedBlogPostTemplate();
            var article = CreateBlogEntryArticle();
            article.BannerImage = "https://example.com/post-banner.jpg";

            // Act
            var result = await service.GenerateBlogEntryHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains(article.BannerImage, result, "Result should contain banner image src.");
            Assert.Contains("<img", result, "Result should contain img tag.");
            Assert.Contains("ccms-img-widget-img", result, "Image should have correct class.");
        }

        /// <summary>
        /// Verifies that GenerateBlogEntryHtml clears image div when no banner image.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogEntryHtml_NoBannerImage_ClearsImageDiv()
        {
            // Arrange
            await SeedBlogPostTemplate();
            var article = CreateBlogEntryArticle();
            article.BannerImage = string.Empty;

            // Act
            var result = await service.GenerateBlogEntryHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains("ccms-blog-title-image", result, "Result should contain image div.");
            Assert.DoesNotContain("<img", result, "Result should not contain img tag when no banner.");
        }

        /// <summary>
        /// Verifies that GenerateBlogEntryHtml sets title correctly.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogEntryHtml_SetsTitle_InCorrectElement()
        {
            // Arrange
            await SeedBlogPostTemplate();
            var article = CreateBlogEntryArticle();
            article.Title = "My Amazing Blog Post";

            // Act
            var result = await service.GenerateBlogEntryHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains("My Amazing Blog Post", result, "Result should contain the title.");
            Assert.Contains("ccms-blog-item-title", result, "Result should contain title element.");
        }

        /// <summary>
        /// Verifies that GenerateBlogEntryHtml sets content correctly.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogEntryHtml_SetsContent_InCorrectElement()
        {
            // Arrange
            await SeedBlogPostTemplate();
            var article = CreateBlogEntryArticle();
            article.Content = "<p>This is my blog content with <strong>formatting</strong>.</p>";

            // Act
            var result = await service.GenerateBlogEntryHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains(article.Content, result, "Result should contain the article content.");
            Assert.Contains("ccms-blog-item-content", result, "Result should contain content element.");
        }

        /// <summary>
        /// Verifies that GenerateBlogEntryHtml handles HTML special characters correctly.
        /// </summary>
        [TestMethod]
        public async Task GenerateBlogEntryHtml_WithSpecialCharacters_PreservesContent()
        {
            // Arrange
            await SeedBlogPostTemplate();
            var article = CreateBlogEntryArticle();
            article.Title = "Test & Demo";
            article.Content = "<p>Testing &lt;special&gt; characters &amp; entities.</p>";

            // Act
            var result = await service.GenerateBlogEntryHtml(article);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.Contains(article.Title, result, "Result should preserve title with special chars.");
            Assert.Contains(article.Content, result, "Result should preserve content with special chars.");
        }

        #endregion

        #region Constructor Tests

        /// <summary>
        /// Verifies constructor initializes dependencies correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_InitializesDependencies_Successfully()
        {
            // Arrange & Act
            var testService = new BlogRenderingService(Db);

            // Assert
            Assert.IsNotNull(testService, "Service should be instantiated successfully.");
        }

        /// <summary>
        /// Verifies constructor throws when database context is null.
        /// </summary>
        [TestMethod]
        public void Constructor_NullDbContext_ThrowsNullReferenceException()
        {
            // Arrange & Act - This will throw when service tries to use the null context
            Assert.Throws<ArgumentNullException>(() => new BlogRenderingService(null!));

            // To actually trigger the exception, we need to call a method
            // But since the constructor doesn't validate, we document the expected behavior
            // Assert - Exception expected when service is used
        }

        #endregion

        #region Helper Methods

        private async Task SeedBlogStreamTemplate()
        {
            var template = new Template
            {
                PageType = "blog-stream",
                Title = "Blog Stream Template",
                Content = @"
                    <img class='stream-banner' src='' />
                    <h1 class='stream-title'></h1>
                    <p class='stream-description'></p>
                    <div class='blog-items'></div>"
            };

            Db.Templates.Add(template);
            await Db.SaveChangesAsync();
        }

        private async Task SeedBlogPostTemplate()
        {
            var template = new Template
            {
                PageType = "blog-post",
                Title = "Blog Post Template",
                Content = @"
                    <div class='ccms-blog-title-image'></div>
                    <h1 class='ccms-blog-item-title'></h1>
                    <div class='ccms-blog-item-content'></div>"
            };

            Db.Templates.Add(template);
            await Db.SaveChangesAsync();
        }

        private Article CreateBlogStreamArticle(string blogKey = "default")
        {
            return new Article
            {
                Id = Guid.NewGuid(),
                ArticleType = (int)ArticleType.BlogStream, // Assuming 2 indicates blog stream
                BlogKey = blogKey,
                Title = "My Blog Stream",
                Introduction = "Welcome to my blog!",
                BannerImage = string.Empty,
                Content = "<p>Stream content</p>",
                StatusCode = (int)StatusCodeEnum.Active,
                Published = DateTimeOffset.UtcNow,
                ArticleNumber = 1,
                VersionNumber = 1,
                UrlPath = "blog"
            };
        }

        private Article CreateBlogEntryArticle()
        {
            return new Article
            {
                Id = Guid.NewGuid(),
                BlogKey = "default",
                ArticleType = (int)ArticleType.BlogPost, // Assuming 1 indicates blog entry
                Title = "Test Blog Entry",
                Introduction = "This is a test blog entry.",
                BannerImage = string.Empty,
                Content = "<p>Blog entry content goes here.</p>",
                StatusCode = (int)StatusCodeEnum.Active,
                Published = DateTimeOffset.UtcNow,
                ArticleNumber = 2,
                VersionNumber = 1,
                UrlPath = "test-blog-entry"
            };
        }

        private async Task<Article> CreateBlogEntry(
            string blogKey,
            string title,
            string content,
            DateTimeOffset published,
            bool isDeleted = false,
            bool isRedirect = false,
            string introduction = "Default introduction")
        {
            var statusCode = isDeleted
                ? (int)StatusCodeEnum.Deleted
                : (isRedirect ? (int)StatusCodeEnum.Redirect : (int)StatusCodeEnum.Active);

            var article = new Article
            {
                Id = Guid.NewGuid(),
                ArticleType = (int)ArticleType.BlogPost, // Assuming 1 indicates blog entry
                BlogKey = blogKey,
                Title = title,
                Introduction = introduction,
                BannerImage = "https://example.com/banner.jpg",
                Content = content,
                StatusCode = statusCode,
                Published = published,
                ArticleNumber = await Db.Articles.CountAsync() + 1,
                VersionNumber = 1,
                UrlPath = title.ToLower().Replace(" ", "-")
            };

            Db.Articles.Add(article);
            await Db.SaveChangesAsync();

            return article;
        }

        #endregion
    }
}