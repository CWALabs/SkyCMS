// <copyright file="SaveArticleContentTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Features.Articles.Save
{
    using Cosmos.Common.Data;
    using Cosmos.Common.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sky.Editor.Features.Articles.Save;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Tests for content processing, integrity, and edge cases.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class SaveArticleContentTests : SkyCmsTestBase
    {
        [TestInitialize]
        public void Setup() => InitializeTestContext();

        [TestMethod]
        public async Task SaveArticle_WithNullContent_ReturnsValidationError()
        {
            // Arrange
            var article = await Logic.CreateArticle("Test", TestUserId);
            await Logic.SaveArticle(article, TestUserId);

            var command = new SaveArticleCommand
            {
                ArticleNumber = article.ArticleNumber,
                Title = "Test",
                Content = null!, // Null content
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Errors.ContainsKey(nameof(command.Content)));
        }

        [TestMethod]
        public async Task SaveArticle_WithEmptyContent_ReturnsValidationError()
        {
            // Arrange
            var article = await Logic.CreateArticle("Empty Content", TestUserId);
            await Logic.SaveArticle(article, TestUserId);

            var command = new SaveArticleCommand
            {
                ArticleNumber = article.ArticleNumber,
                Title = "Empty Content Test",
                Content = string.Empty, // Empty content
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Errors.ContainsKey(nameof(command.Content)));
        }

        [TestMethod]
        public async Task SaveArticle_WithVeryLargeContent_SavesSuccessfully()
        {
            // Arrange
            var article = await Logic.CreateArticle("Large Content Test", TestUserId);
            await Logic.SaveArticle(article, TestUserId);

            // Generate 500KB of HTML content
            var largeContent = string.Concat(Enumerable.Repeat("<p>Lorem ipsum dolor sit amet, consectetur adipiscing elit. </p>", 10000));

            var command = new SaveArticleCommand
            {
                ArticleNumber = article.ArticleNumber,
                Title = "Large Content Article",
                Content = largeContent,
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Model!.Content.Length > 100000);
            
            // Verify content was persisted correctly
            var savedArticle = await Db.Articles
                .FirstOrDefaultAsync(a => a.ArticleNumber == article.ArticleNumber);
            Assert.IsNotNull(savedArticle);
            Assert.IsTrue(savedArticle.Content.Length > 100000);
        }

        [TestMethod]
        public async Task SaveArticle_WithSpecialCharacters_PreservesContent()
        {
            // Arrange
            var article = await Logic.CreateArticle("Special Chars", TestUserId);
            await Logic.SaveArticle(article, TestUserId);

            var specialContent = "<p>Test with émojis 🎉 and spëcial çhars & symbols: © ® ™ € £ ¥</p>";

            var command = new SaveArticleCommand
            {
                ArticleNumber = article.ArticleNumber,
                Title = "Special Characters Test",
                Content = specialContent,
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var saved = await Db.Articles
                .FirstOrDefaultAsync(a => a.ArticleNumber == article.ArticleNumber);
            Assert.IsNotNull(saved);
            Assert.IsTrue(saved.Content.Contains("émojis"));
            Assert.IsTrue(saved.Content.Contains("🎉"));
            Assert.IsTrue(saved.Content.Contains("spëcial"));
            Assert.IsTrue(saved.Content.Contains("çhars"));
        }

        [TestMethod]
        public async Task SaveArticle_EnsuresEditableMarkers_AddedToContent()
        {
            // Arrange
            var article = await Logic.CreateArticle("Editable Test", TestUserId);
            await Logic.SaveArticle(article, TestUserId);

            var contentWithoutMarkers = "<div><h1>Title</h1><p>Content without editable markers</p></div>";

            var command = new SaveArticleCommand
            {
                ArticleNumber = article.ArticleNumber,
                Title = "Editable Markers Test",
                Content = contentWithoutMarkers,
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Data!.Model!.Content.Contains("contenteditable") || 
                         result.Data.Model.Content.Contains("data-ccms-ceid"));
        }

        [TestMethod]
        public async Task SaveArticle_WithMalformedHtml_HandlesGracefully()
        {
            // Arrange
            var article = await Logic.CreateArticle("Malformed HTML", TestUserId);
            await Logic.SaveArticle(article, TestUserId);

            var malformedHtml = "<div><p>Unclosed paragraph<div>Nested incorrectly</p></div>";

            var command = new SaveArticleCommand
            {
                ArticleNumber = article.ArticleNumber,
                Title = "Malformed HTML Test",
                Content = malformedHtml,
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsTrue(result.IsSuccess); // Should not crash
            Assert.IsNotNull(result.Data!.Model!.Content);
            
            // Verify content was saved
            var saved = await Db.Articles
                .FirstOrDefaultAsync(a => a.ArticleNumber == article.ArticleNumber);
            Assert.IsNotNull(saved);
            Assert.IsFalse(string.IsNullOrWhiteSpace(saved.Content));
        }

        [TestMethod]
        public async Task SaveArticle_WithScriptTags_PreservesContent()
        {
            // Arrange
            var article = await Logic.CreateArticle("Script Tags Test", TestUserId);
            await Logic.SaveArticle(article, TestUserId);

            var contentWithScript = "<div><p>Content</p><script>console.log('test');</script></div>";

            var command = new SaveArticleCommand
            {
                ArticleNumber = article.ArticleNumber,
                Title = "Script Tags Test",
                Content = contentWithScript,
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var saved = await Db.Articles
                .FirstOrDefaultAsync(a => a.ArticleNumber == article.ArticleNumber);
            Assert.IsNotNull(saved);
            // Content should be preserved (sanitization happens elsewhere if needed)
            Assert.IsTrue(saved.Content.Contains("script"));
        }

        [TestMethod]
        public async Task SaveArticle_WithUnicodeContent_PreservesEncoding()
        {
            // Arrange
            var article = await Logic.CreateArticle("Unicode Test", TestUserId);
            await Logic.SaveArticle(article, TestUserId);

            var unicodeContent = "<p>Chinese: 你好世界</p><p>Arabic: مرحبا بالعالم</p><p>Russian: Привет мир</p>";

            var command = new SaveArticleCommand
            {
                ArticleNumber = article.ArticleNumber,
                Title = "Unicode Content Test",
                Content = unicodeContent,
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var saved = await Db.Articles
                .FirstOrDefaultAsync(a => a.ArticleNumber == article.ArticleNumber);
            Assert.IsNotNull(saved);
            Assert.IsTrue(saved.Content.Contains("你好世界"));
            Assert.IsTrue(saved.Content.Contains("مرحبا بالعالم"));
            Assert.IsTrue(saved.Content.Contains("Привет мир"));
        }

        [TestMethod]
        public async Task SaveArticle_ContentWithNestedDivs_PreservesStructure()
        {
            // Arrange
            var article = await Logic.CreateArticle("Nested Structure", TestUserId);
            await Logic.SaveArticle(article, TestUserId);

            var nestedContent = @"
                <div class='container'>
                    <div class='row'>
                        <div class='col-md-6'>
                            <p>Column 1</p>
                        </div>
                        <div class='col-md-6'>
                            <p>Column 2</p>
                        </div>
                    </div>
                </div>";

            var command = new SaveArticleCommand
            {
                ArticleNumber = article.ArticleNumber,
                Title = "Nested Structure Test",
                Content = nestedContent,
                UserId = TestUserId,
                ArticleType = ArticleType.General
            };

            // Act
            var result = await SaveArticleHandler.HandleAsync(command);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            var saved = await Db.Articles
                .FirstOrDefaultAsync(a => a.ArticleNumber == article.ArticleNumber);
            Assert.IsNotNull(saved);
            Assert.IsTrue(saved.Content.Contains("container"));
            Assert.IsTrue(saved.Content.Contains("col-md-6"));
        }
    }
}