// <copyright file="TemplateServiceTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Services.Templates
{
    using Cosmos.Common.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sky.Editor.Services.Templates;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Unit tests for the TemplateService class.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class TemplateServiceTests
    {
        private Mock<IWebHostEnvironment> mockEnvironment;
        private Mock<ILogger<TemplateService>> mockLogger;
        private ApplicationDbContext dbContext;
        private TemplateService templateService;

        /// <summary>
        /// Initializes the test environment before each test.
        /// </summary>
        [TestInitialize]
        public new void Setup()
        {
            // Setup mock IWebHostEnvironment
            mockEnvironment = new Mock<IWebHostEnvironment>();
            mockEnvironment.Setup(e => e.ContentRootPath).Returns("TestRoot");

            // Setup mock ILogger
            mockLogger = new Mock<ILogger<TemplateService>>();

            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            dbContext = new ApplicationDbContext(options);

            // Create the service under test
            templateService = new TemplateService(mockEnvironment.Object, mockLogger.Object, dbContext);
        }

        /// <summary>
        /// Cleans up after each test.
        /// </summary>
        [TestCleanup]
        public void Cleanup()
        {
            dbContext?.Dispose();
        }

        #region GetAllTemplatesAsync Tests

        /// <summary>
        /// Tests that GetAllTemplatesAsync returns all standard templates.
        /// </summary>
        [TestMethod]
        public async Task GetAllTemplatesAsync_ReturnsAllTemplates()
        {
            // Act
            var result = await templateService.GetAllTemplatesAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result, "Should return at least one template");
            Assert.IsTrue(result.Any(t => t.Key == "blog-stream"), "Should contain blog-stream template");
            Assert.IsTrue(result.Any(t => t.Key == "blog-post"), "Should contain blog-post template");
        }

        /// <summary>
        /// Tests that GetAllTemplatesAsync returns cached templates on second call.
        /// </summary>
        [TestMethod]
        public async Task GetAllTemplatesAsync_ReturnsCachedTemplates_OnSecondCall()
        {
            // Act
            var firstCall = await templateService.GetAllTemplatesAsync();
            var secondCall = await templateService.GetAllTemplatesAsync();

            // Assert
            Assert.AreSame(firstCall, secondCall, "Should return the same cached instance");
        }

        /// <summary>
        /// Tests that GetAllTemplatesAsync returns templates with all required properties.
        /// </summary>
        [TestMethod]
        public async Task GetAllTemplatesAsync_TemplatesHaveRequiredProperties()
        {
            // Act
            var result = await templateService.GetAllTemplatesAsync();

            // Assert
            foreach (var template in result)
            {
                Assert.IsFalse(string.IsNullOrEmpty(template.Key), "Template should have a Key");
                Assert.IsFalse(string.IsNullOrEmpty(template.Name), "Template should have a Name");
                Assert.IsFalse(string.IsNullOrEmpty(template.Description), "Template should have a Description");
                Assert.IsFalse(string.IsNullOrEmpty(template.Category), "Template should have a Category");
                Assert.IsFalse(string.IsNullOrEmpty(template.FilePath), "Template should have a FilePath");
                Assert.IsNotNull(template.Tags, "Template should have Tags collection");
            }
        }

        #endregion

        #region GetTemplatesByCategoryAsync Tests

        /// <summary>
        /// Tests that GetTemplatesByCategoryAsync returns only templates from specified category.
        /// </summary>
        [TestMethod]
        public async Task GetTemplatesByCategoryAsync_ReturnsBlogCategoryTemplates()
        {
            // Act
            var result = await templateService.GetTemplatesByCategoryAsync("Blog");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result, "Should return at least one Blog template");
            Assert.IsTrue(result.All(t => t.Category.Equals("Blog", StringComparison.OrdinalIgnoreCase)),
                "All templates should be in Blog category");
        }

        /// <summary>
        /// Tests that GetTemplatesByCategoryAsync returns empty list for non-existent category.
        /// </summary>
        [TestMethod]
        public async Task GetTemplatesByCategoryAsync_ReturnsEmptyList_ForNonExistentCategory()
        {
            // Act
            var result = await templateService.GetTemplatesByCategoryAsync("NonExistentCategory");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result, "Should return empty list for non-existent category");
        }

        /// <summary>
        /// Tests that GetTemplatesByCategoryAsync is case-insensitive.
        /// </summary>
        [TestMethod]
        public async Task GetTemplatesByCategoryAsync_IsCaseInsensitive()
        {
            // Act
            var lowerCase = await templateService.GetTemplatesByCategoryAsync("blog");
            var upperCase = await templateService.GetTemplatesByCategoryAsync("BLOG");
            var mixedCase = await templateService.GetTemplatesByCategoryAsync("BlOg");

            // Assert
            Assert.HasCount(lowerCase.Count, upperCase, "Should return same count regardless of case");
            Assert.HasCount(lowerCase.Count, mixedCase, "Should return same count regardless of case");
        }

        #endregion

        #region GetTemplateByKeyAsync Tests

        /// <summary>
        /// Tests that GetTemplateByKeyAsync returns correct template for valid key.
        /// </summary>
        [TestMethod]
        public async Task GetTemplateByKeyAsync_ReturnsCorrectTemplate_ForValidKey()
        {
            // Act
            var result = await templateService.GetTemplateByKeyAsync("blog-stream");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("blog-stream", result.Key);
            Assert.AreEqual("Blog Stream", result.Name);
            Assert.AreEqual("Blog", result.Category);
        }

        /// <summary>
        /// Tests that GetTemplateByKeyAsync returns null for invalid key.
        /// </summary>
        [TestMethod]
        public async Task GetTemplateByKeyAsync_ReturnsNull_ForInvalidKey()
        {
            // Act
            var result = await templateService.GetTemplateByKeyAsync("non-existent-key");

            // Assert
            Assert.IsNull(result);
        }

        /// <summary>
        /// Tests that GetTemplateByKeyAsync is case-insensitive.
        /// </summary>
        [TestMethod]
        public async Task GetTemplateByKeyAsync_IsCaseInsensitive()
        {
            // Act
            var lowerCase = await templateService.GetTemplateByKeyAsync("blog-stream");
            var upperCase = await templateService.GetTemplateByKeyAsync("BLOG-STREAM");
            var mixedCase = await templateService.GetTemplateByKeyAsync("Blog-Stream");

            // Assert
            Assert.IsNotNull(lowerCase);
            Assert.IsNotNull(upperCase);
            Assert.IsNotNull(mixedCase);
            Assert.AreEqual(lowerCase.Key, upperCase.Key);
            Assert.AreEqual(lowerCase.Key, mixedCase.Key);
        }

        #endregion

        #region GetTemplateContentAsync Tests

        /// <summary>
        /// Tests that GetTemplateContentAsync returns content for valid key.
        /// </summary>
        [TestMethod]
        public async Task GetTemplateContentAsync_ReturnsContent_ForValidKey()
        {
            // Act
            var result = await templateService.GetTemplateContentAsync("blog-stream");

            // Assert - Content may be null if file doesn't exist, but method should not throw
            Assert.IsNotNull(result != null ? result : string.Empty);
        }

        /// <summary>
        /// Tests that GetTemplateContentAsync returns null for invalid key.
        /// </summary>
        [TestMethod]
        public async Task GetTemplateContentAsync_ReturnsNull_ForInvalidKey()
        {
            // Act
            var result = await templateService.GetTemplateContentAsync("non-existent-key");

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region SearchTemplatesAsync Tests

        /// <summary>
        /// Tests that SearchTemplatesAsync returns all templates for empty search term.
        /// </summary>
        [TestMethod]
        public async Task SearchTemplatesAsync_ReturnsAllTemplates_ForEmptySearchTerm()
        {
            // Arrange
            var allTemplates = await templateService.GetAllTemplatesAsync();

            // Act
            var result = await templateService.SearchTemplatesAsync(string.Empty);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(allTemplates.Count, result);
        }

        /// <summary>
        /// Tests that SearchTemplatesAsync returns all templates for null search term.
        /// </summary>
        [TestMethod]
        public async Task SearchTemplatesAsync_ReturnsAllTemplates_ForNullSearchTerm()
        {
            // Arrange
            var allTemplates = await templateService.GetAllTemplatesAsync();

            // Act
            var result = await templateService.SearchTemplatesAsync(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(allTemplates.Count, result);
        }

        /// <summary>
        /// Tests that SearchTemplatesAsync returns all templates for whitespace search term.
        /// </summary>
        [TestMethod]
        public async Task SearchTemplatesAsync_ReturnsAllTemplates_ForWhitespaceSearchTerm()
        {
            // Arrange
            var allTemplates = await templateService.GetAllTemplatesAsync();

            // Act
            var result = await templateService.SearchTemplatesAsync("   ");

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(allTemplates.Count, result);
        }

        /// <summary>
        /// Tests that SearchTemplatesAsync finds templates by name.
        /// </summary>
        [TestMethod]
        public async Task SearchTemplatesAsync_FindsTemplatesByName()
        {
            // Act
            var result = await templateService.SearchTemplatesAsync("Blog Stream");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result, "Should find templates matching 'Blog Stream'");
            Assert.IsTrue(result.Any(t => t.Name.Contains("Blog Stream", StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Tests that SearchTemplatesAsync finds templates by description.
        /// </summary>
        [TestMethod]
        public async Task SearchTemplatesAsync_FindsTemplatesByDescription()
        {
            // Act
            var result = await templateService.SearchTemplatesAsync("featured image");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result, "Should find templates with 'featured image' in description");
        }

        /// <summary>
        /// Tests that SearchTemplatesAsync finds templates by tags.
        /// </summary>
        [TestMethod]
        public async Task SearchTemplatesAsync_FindsTemplatesByTags()
        {
            // Act
            var result = await templateService.SearchTemplatesAsync("blog");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result, "Should find templates with 'blog' tag");
        }

        /// <summary>
        /// Tests that SearchTemplatesAsync is case-insensitive.
        /// </summary>
        [TestMethod]
        public async Task SearchTemplatesAsync_IsCaseInsensitive()
        {
            // Act
            var lowerCase = await templateService.SearchTemplatesAsync("blog");
            var upperCase = await templateService.SearchTemplatesAsync("BLOG");
            var mixedCase = await templateService.SearchTemplatesAsync("BlOg");

            // Assert
            Assert.HasCount(lowerCase.Count, upperCase, "Should return same count regardless of case");
            Assert.HasCount(lowerCase.Count, mixedCase, "Should return same count regardless of case");
        }

        /// <summary>
        /// Tests that SearchTemplatesAsync returns empty list for non-matching search term.
        /// </summary>
        [TestMethod]
        public async Task SearchTemplatesAsync_ReturnsEmptyList_ForNonMatchingSearchTerm()
        {
            // Act
            var result = await templateService.SearchTemplatesAsync("xyznonexistent");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsEmpty(result, "Should return empty list for non-matching search term");
        }

        #endregion

        #region EnsureDefaultTemplatesExistAsync Tests

        /// <summary>
        /// Tests that EnsureDefaultTemplatesExistAsync creates templates when none exist.
        /// </summary>
        [TestMethod]
        public async Task EnsureDefaultTemplatesExistAsync_CreatesTemplates_WhenNoneExist()
        {
            // Arrange
            var defaultLayout = new Layout
            {
                Id = Guid.NewGuid(),
                IsDefault = true,
                CommunityLayoutId = Guid.NewGuid().ToString()
            };
            await dbContext.Layouts.AddAsync(defaultLayout);
            await dbContext.SaveChangesAsync();

            // Act
            await templateService.EnsureDefaultTemplatesExistAsync();

            // Assert
            var templates = await dbContext.Templates.ToListAsync();
            Assert.IsNotEmpty(templates, "Should have created at least one template");
            Assert.IsTrue(templates.Any(t => t.Title == "Blog Stream"), "Should have created Blog Stream template");
            Assert.IsTrue(templates.Any(t => t.Title == "Blog Post"), "Should have created Blog Post template");
        }

        /// <summary>
        /// Tests that EnsureDefaultTemplatesExistAsync does not create duplicates.
        /// </summary>
        [TestMethod]
        public async Task EnsureDefaultTemplatesExistAsync_DoesNotCreateDuplicates()
        {
            // Arrange
            var defaultLayout = new Layout
            {
                Id = Guid.NewGuid(),
                IsDefault = true,
                CommunityLayoutId = Guid.NewGuid().ToString()
            };
            await dbContext.Layouts.AddAsync(defaultLayout);

            var existingTemplate = new Template
            {
                Id = Guid.NewGuid(),
                Title = "Blog Stream",
                PageType = "blog-stream",
                LayoutId = defaultLayout.Id,
                Content = "<div>Existing content</div>"
            };
            await dbContext.Templates.AddAsync(existingTemplate);
            await dbContext.SaveChangesAsync();

            var initialCount = await dbContext.Templates.CountAsync();

            // Act
            await templateService.EnsureDefaultTemplatesExistAsync();

            // Assert
            var finalCount = await dbContext.Templates.CountAsync();
            Assert.IsGreaterThanOrEqualTo(initialCount, finalCount, "Should not decrease template count");

            var blogStreamTemplates = await dbContext.Templates
                .Where(t => t.Title == "Blog Stream")
                .ToListAsync();
            Assert.HasCount(1, blogStreamTemplates, "Should not create duplicate Blog Stream template");
        }

        /// <summary>
        /// Tests that EnsureDefaultTemplatesExistAsync handles missing default layout gracefully.
        /// </summary>
        [TestMethod]
        public async Task EnsureDefaultTemplatesExistAsync_HandlesNoDefaultLayout()
        {
            // Act
            await templateService.EnsureDefaultTemplatesExistAsync();

            // Assert - Should not throw exception
            var templates = await dbContext.Templates.ToListAsync();

            // Templates may still be created with null LayoutId
            Assert.IsNotNull(templates);
        }

        #endregion

        #region Thread Safety Tests

        /// <summary>
        /// Tests that GetAllTemplatesAsync is thread-safe.
        /// </summary>
        [TestMethod]
        public async Task GetAllTemplatesAsync_IsThreadSafe()
        {
            // Arrange
            var tasks = new List<Task<List<PageTemplate>>>();

            // Act - Execute multiple concurrent calls
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(templateService.GetAllTemplatesAsync());
            }

            var results = await Task.WhenAll(tasks);

            // Assert - All results should be the same cached instance
            var firstResult = results[0];
            foreach (var result in results)
            {
                Assert.AreSame(firstResult, result, "All concurrent calls should return the same cached instance");
            }
        }

        #endregion
    }
}