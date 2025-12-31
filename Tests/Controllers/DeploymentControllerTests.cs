// <copyright file="DeploymentControllerTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Controllers
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using BCrypt.Net;
    using Cosmos.Cms.Common;
    using Cosmos.Cms.Common.Models;
    using Cosmos.Cms.Editor.Controllers;
    using Cosmos.Common.Data;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sky.Cms.Controllers;

    /// <summary>
    /// Comprehensive unit tests for the DeploymentController.
    /// </summary>
    [TestClass]
    public class DeploymentControllerTests : SkyCmsTestBase
    {
        private DeploymentController controller;
        private Guid testSpaArticleId;
        private string testDeploymentKey;
        private string testWebhookSecret;

        [TestInitialize]
        public new void Setup()
        {
            InitializeTestContext();

            // Generate test credentials
            testDeploymentKey = "TestDeployKey123!";
            testWebhookSecret = "TestWebhookSecret456!";

            // Create a test SPA article
            testSpaArticleId = Guid.NewGuid();
            var metadata = new SpaMetadata
            {
                DeploymentKeyHash = BCrypt.HashPassword(testDeploymentKey),
                WebhookSecretHash = BCrypt.HashPassword(testWebhookSecret),
                DeploymentCount = 0
            };

            var spaArticle = new PublishedPage
            {
                Id = testSpaArticleId,
                ArticleNumber = 1,
                Title = "Test SPA Application",
                UrlPath = "/test-spa",
                ArticleType = (int)ArticleType.SpaApp,
                Content = System.Text.Json.JsonSerializer.Serialize(metadata),
                Published = DateTimeOffset.UtcNow,
                StatusCode = 0,
                Updated = DateTimeOffset.UtcNow,
                VersionNumber = 1
            };

            Db.Pages.Add(spaArticle);
            Db.SaveChanges();

            // Create controller
            controller = new DeploymentController(
                Db,
                Storage,
                new NullLogger<DeploymentController>());

            // Setup HttpContext with headers
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Hub-Signature-256"] = "sha256=test";
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region Deploy Method Tests

        [TestMethod]
        public async Task Deploy_ValidRequest_ReturnsOkWithMetadata()
        {
            // Arrange
            var zipFile = CreateTestZipFile();

            // Act
            var result = await controller.Deploy(testSpaArticleId, testDeploymentKey, zipFile);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            dynamic value = okResult.Value;
            Assert.IsTrue(value.success);
            Assert.AreEqual(1, (int)value.deploymentCount);
        }

        [TestMethod]
        public async Task Deploy_InvalidArticleId_ReturnsNotFound()
        {
            // Arrange
            var invalidId = Guid.NewGuid();
            var zipFile = CreateTestZipFile();

            // Act
            var result = await controller.Deploy(invalidId, testDeploymentKey, zipFile);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
            var notFoundResult = result as NotFoundObjectResult;
            dynamic value = notFoundResult.Value;
            Assert.IsFalse(value.success);
            Assert.AreEqual("SPA article not found", (string)value.error);
        }

        [TestMethod]
        public async Task Deploy_InvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var zipFile = CreateTestZipFile();
            var wrongPassword = "WrongPassword123!";

            // Act
            var result = await controller.Deploy(testSpaArticleId, wrongPassword, zipFile);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
            var unauthorizedResult = result as UnauthorizedObjectResult;
            dynamic value = unauthorizedResult.Value;
            Assert.IsFalse(value.success);
            Assert.AreEqual("Invalid deployment key", (string)value.error);
        }

        [TestMethod]
        public async Task Deploy_NonSpaArticle_ReturnsNotFound()
        {
            // Arrange
            var regularArticle = new PublishedPage
            {
                Id = Guid.NewGuid(),
                ArticleNumber = 2,
                Title = "Regular Article",
                UrlPath = "/regular",
                ArticleType = (int)ArticleType.General,
                Published = DateTimeOffset.UtcNow,
                StatusCode = 0
            };
            Db.Pages.Add(regularArticle);
            Db.SaveChanges();

            var zipFile = CreateTestZipFile();

            // Act
            var result = await controller.Deploy(regularArticle.Id, testDeploymentKey, zipFile);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task Deploy_NullZipFile_ReturnsBadRequest()
        {
            // Act
            var result = await controller.Deploy(testSpaArticleId, testDeploymentKey, null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            dynamic value = badRequestResult.Value;
            Assert.IsFalse(value.success);
            Assert.AreEqual("No file uploaded", (string)value.error);
        }

        [TestMethod]
        public async Task Deploy_EmptyZipFile_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);
            mockFile.Setup(f => f.FileName).Returns("test.zip");

            // Act
            var result = await controller.Deploy(testSpaArticleId, testDeploymentKey, mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Deploy_OversizedZipFile_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(101_000_000); // 101 MB (over 100 MB limit)
            mockFile.Setup(f => f.FileName).Returns("test.zip");

            // Act
            var result = await controller.Deploy(testSpaArticleId, testDeploymentKey, mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            dynamic value = badRequestResult.Value;
            Assert.IsTrue(((string)value.error).Contains("exceeds maximum"));
        }

        [TestMethod]
        public async Task Deploy_NonZipFile_ReturnsBadRequest()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1000);
            mockFile.Setup(f => f.FileName).Returns("test.txt");

            // Act
            var result = await controller.Deploy(testSpaArticleId, testDeploymentKey, mockFile.Object);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            dynamic value = badRequestResult.Value;
            Assert.AreEqual("File must be a .zip archive", (string)value.error);
        }

        [TestMethod]
        public async Task Deploy_UpdatesDeploymentCount()
        {
            // Arrange
            var zipFile = CreateTestZipFile();

            // Act - First deployment
            await controller.Deploy(testSpaArticleId, testDeploymentKey, zipFile);

            // Create new zip for second deployment
            var zipFile2 = CreateTestZipFile();

            // Act - Second deployment
            var result = await controller.Deploy(testSpaArticleId, testDeploymentKey, zipFile2);

            // Assert
            var okResult = result as OkObjectResult;
            dynamic value = okResult.Value;
            Assert.AreEqual(2, (int)value.deploymentCount);
        }

        [TestMethod]
        public async Task Deploy_ExtractsGitHubHeaders()
        {
            // Arrange
            var zipFile = CreateTestZipFile();
            controller.ControllerContext.HttpContext.Request.Headers["X-GitHub-SHA"] = "abc123commit";
            controller.ControllerContext.HttpContext.Request.Headers["X-GitHub-Repository"] = "owner/repo";

            // Act
            var result = await controller.Deploy(testSpaArticleId, testDeploymentKey, zipFile);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            // Verify metadata was updated
            var article = await Db.Pages.FindAsync(testSpaArticleId);
            var metadata = System.Text.Json.JsonSerializer.Deserialize<SpaMetadata>(article.Content);
            Assert.AreEqual("abc123commit", metadata.LastCommitSha);
            Assert.AreEqual("owner/repo", metadata.LastDeployedFrom);
        }

        [TestMethod]
        public async Task Deploy_WithoutWebhookSignature_StillSucceeds()
        {
            // Arrange
            var zipFile = CreateTestZipFile();
            controller.ControllerContext.HttpContext.Request.Headers.Remove("X-Hub-Signature-256");

            // Act
            var result = await controller.Deploy(testSpaArticleId, testDeploymentKey, zipFile);

            // Assert - Should succeed (development mode allows missing signature)
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        #endregion

        #region Password Rotation Tests

        [TestMethod]
        public async Task Deploy_WithRotatedPassword_AcceptsPreviousKeyInGracePeriod()
        {
            // Arrange
            var newPassword = "NewPassword789!";
            var article = await Db.Pages.FindAsync(testSpaArticleId);
            var metadata = System.Text.Json.JsonSerializer.Deserialize<SpaMetadata>(article.Content);

            // Simulate password rotation
            metadata.DeploymentKeyHashPrevious = metadata.DeploymentKeyHash;
            metadata.DeploymentKeyHash = BCrypt.HashPassword(newPassword);
            metadata.DeploymentKeyRotatedAt = DateTimeOffset.UtcNow.AddHours(-1); // 1 hour ago

            article.Content = System.Text.Json.JsonSerializer.Serialize(metadata);
            await Db.SaveChangesAsync();

            var zipFile = CreateTestZipFile();

            // Act - Use OLD password (should work within grace period)
            var result = await controller.Deploy(testSpaArticleId, testDeploymentKey, zipFile);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Deploy_WithRotatedPassword_RejectsOldKeyAfterGracePeriod()
        {
            // Arrange
            var newPassword = "NewPassword789!";
            var article = await Db.Pages.FindAsync(testSpaArticleId);
            var metadata = System.Text.Json.JsonSerializer.Deserialize<SpaMetadata>(article.Content);

            // Simulate password rotation 25 hours ago (beyond 24-hour grace period)
            metadata.DeploymentKeyHashPrevious = metadata.DeploymentKeyHash;
            metadata.DeploymentKeyHash = BCrypt.HashPassword(newPassword);
            metadata.DeploymentKeyRotatedAt = DateTimeOffset.UtcNow.AddHours(-25);

            article.Content = System.Text.Json.JsonSerializer.Serialize(metadata);
            await Db.SaveChangesAsync();

            var zipFile = CreateTestZipFile();

            // Act - Use OLD password (should fail after grace period)
            var result = await controller.Deploy(testSpaArticleId, testDeploymentKey, zipFile);

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        #endregion

        #region File Validation Tests

        [TestMethod]
        public async Task Deploy_WithPathTraversalAttempt_ThrowsException()
        {
            // Arrange
            var zipFile = CreateMaliciousZipFile("../../../etc/passwd");

            // Act & Assert
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await controller.Deploy(testSpaArticleId, testDeploymentKey, zipFile);
            });
        }

        [TestMethod]
        public async Task Deploy_WithValidHtmlCssJs_Succeeds()
        {
            // Arrange
            var zipFile = CreateTestZipFile(new[]
            {
                ("index.html", "<html><body>Test</body></html>"),
                ("styles.css", "body { color: red; }"),
                ("app.js", "console.log('test');")
            });

            // Act
            var result = await controller.Deploy(testSpaArticleId, testDeploymentKey, zipFile);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Deploy_UpdatesArticleTimestamp()
        {
            // Arrange
            var zipFile = CreateTestZipFile();
            var originalUpdated = (await Db.Pages.FindAsync(testSpaArticleId)).Updated;

            // Wait a moment to ensure timestamp changes
            await Task.Delay(100);

            // Act
            await controller.Deploy(testSpaArticleId, testDeploymentKey, zipFile);

            // Assert
            var article = await Db.Pages.FindAsync(testSpaArticleId);
            Assert.IsTrue(article.Updated > originalUpdated);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates a test zip file with sample SPA content.
        /// </summary>
        private IFormFile CreateTestZipFile(params (string filename, string content)[] files)
        {
            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                if (files == null || files.Length == 0)
                {
                    // Default files
                    files = new[]
                    {
                        ("index.html", "<html><body>Test SPA</body></html>"),
                        ("static/js/main.js", "console.log('test');"),
                        ("static/css/style.css", "body { margin: 0; }")
                    };
                }

                foreach (var (filename, content) in files)
                {
                    var entry = archive.CreateEntry(filename);
                    using var entryStream = entry.Open();
                    using var writer = new StreamWriter(entryStream);
                    writer.Write(content);
                }
            }

            memoryStream.Position = 0;

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            mockFile.Setup(f => f.FileName).Returns("spa-deployment.zip");
            mockFile.Setup(f => f.Length).Returns(memoryStream.Length);

            return mockFile.Object;
        }

        /// <summary>
        /// Creates a malicious zip file with path traversal attempt.
        /// </summary>
        private IFormFile CreateMaliciousZipFile(string maliciousPath)
        {
            var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var entry = archive.CreateEntry(maliciousPath);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream);
                writer.Write("malicious content");
            }

            memoryStream.Position = 0;

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            mockFile.Setup(f => f.FileName).Returns("malicious.zip");
            mockFile.Setup(f => f.Length).Returns(memoryStream.Length);

            return mockFile.Object;
        }

        #endregion
    }
}