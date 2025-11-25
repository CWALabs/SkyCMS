// <copyright file="SaveArticleRootPageTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Features.Articles.Save
{
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Cosmos.Common.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sky.Editor.Features.Articles.Save;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Tests for root/home page edge cases during save operations.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class SaveArticleRootPageTests : SkyCmsTestBase
    {
        [TestInitialize]
        public void Setup() => InitializeTestContext();

        [TestMethod]
        public async Task SaveArticle_RootPage_MaintainsRootUrlPath()
        {
            // Arrange - First article becomes root
            var rootArticle = await Logic.CreateArticle("Home Page", TestUserId);
            Assert.AreEqual("root", rootArticle.UrlPath);

            var command = new SaveArticleCommand
            {
                ArticleNumber = rootArticle.ArticleNumber,
                Title = "Updated Home Page",
                Content = "<p>Updated home content</p>",
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var savedArticle = await Db.Articles
                .FirstOrDefaultAsync(a => a.ArticleNumber == rootArticle.ArticleNumber);
            Assert.AreEqual("root", savedArticle!.UrlPath);
        }

        [TestMethod]
        public async Task SaveArticle_RootPage_TitleChange_DoesNotCreateRedirect()
        {
            // Arrange
            var rootArticle = await Logic.CreateArticle("Home", TestUserId);
            Assert.AreEqual("root", rootArticle.UrlPath);

            var command = new SaveArticleCommand
            {
                ArticleNumber = rootArticle.ArticleNumber,
                Title = "Welcome Page", // Title changed
                Content = "<p>Content</p>",
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            
            // Root page should not create redirects
            var redirectCount = await Db.Articles
                .CountAsync(a => a.StatusCode == (int)StatusCodeEnum.Redirect);
            Assert.AreEqual(0, redirectCount);
        }

        [TestMethod]
        public async Task SaveArticle_RootPage_CanBePublished()
        {
            // Arrange
            var rootArticle = await Logic.CreateArticle("Home", TestUserId);

            var command = new SaveArticleCommand
            {
                ArticleNumber = rootArticle.ArticleNumber,
                Title = "Home",
                Content = "<p>Home content</p>",
                Published = Clock.UtcNow,
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var savedArticle = await Db.Articles
                .FirstOrDefaultAsync(a => a.ArticleNumber == rootArticle.ArticleNumber);
            Assert.IsNotNull(savedArticle!.Published);
        }

        [TestMethod]
        public async Task SaveArticle_RootPage_MultipleSaves_StaysRoot()
        {
            // Arrange
            var rootArticle = await Logic.CreateArticle("Home", TestUserId);

            // Perform multiple saves
            for (int i = 1; i <= 3; i++)
            {
                var command = new SaveArticleCommand
                {
                    ArticleNumber = rootArticle.ArticleNumber,
                    Title = $"Home Version {i}",
                    Content = $"<p>Content {i}</p>",
                    UserId = TestUserId,
                    ArticleType = ArticleType.General
                };

                await SaveArticleHandler.HandleAsync(command);
            }

            // Assert
            var articles = await Db.Articles
                .Where(a => a.ArticleNumber == rootArticle.ArticleNumber)
                .ToListAsync();
            
            Assert.IsTrue(articles.All(a => a.UrlPath == "root"));
        }

        [TestMethod]
        public async Task SaveArticle_FirstArticle_AutomaticallyBecomesRoot()
        {
            // This test verifies the first article creation behavior
            // which is handled in CreateArticle, but good to document
            
            // Arrange
            var firstArticle = await Logic.CreateArticle("First Article", TestUserId);

            // Assert
            Assert.AreEqual("root", firstArticle.UrlPath);
            Assert.IsNotNull(firstArticle.Published); // Auto-published
            Assert.AreEqual(1, firstArticle.ArticleNumber);
        }
    }
}