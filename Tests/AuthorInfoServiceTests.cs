// <copyright file="AuthorInfoServiceTests.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Tests.Services.Authors
{
    using Cosmos.Common.Data;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Sky.Editor.Services.Authors;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Unit tests for the <see cref="AuthorInfoService"/> class.
    /// </summary>
    [DoNotParallelize]
    [TestClass]
    public class AuthorInfoServiceTests : SkyCmsTestBase
    {

        [TestInitialize]
        public void Setup() => InitializeTestContext(seedLayout: true);

        [TestCleanup]
        public void Cleanup() => Db.Dispose();

        /// <summary>
        /// Verifies that GetOrCreateAsync returns null when user does not exist.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreateAsync_UserDoesNotExist_ReturnsNull()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var result = await AuthorInfoService.GetOrCreateAsync(nonExistentUserId);

            // Assert
            Assert.IsNull(result, "Should return null when user does not exist.");
        }

        /// <summary>
        /// Verifies that GetOrCreateAsync returns existing AuthorInfo from database.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreateAsync_ExistingAuthorInfo_ReturnsFromDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedAuthorInfo = new AuthorInfo
            {
                Id = userId.ToString(),
                AuthorName = "Test Author",
                AuthorDescription = "Test Description"
            };

            Db.AuthorInfos.Add(expectedAuthorInfo);
            await Db.SaveChangesAsync();

            // Act
            var result = await AuthorInfoService.GetOrCreateAsync(userId);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual(expectedAuthorInfo.Id, result.Id, "ID should match.");
            Assert.AreEqual(expectedAuthorInfo.AuthorName, result.AuthorName, "AuthorName should match.");
            Assert.AreEqual(expectedAuthorInfo.AuthorDescription, result.AuthorDescription, "AuthorDescription should match.");
        }

        /// <summary>
        /// Verifies that GetOrCreateAsync creates new AuthorInfo when none exists.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreateAsync_NoExistingAuthorInfo_CreatesNewRecord()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userName = "testuser@example.com";

            var identityUser = new IdentityUser
            {
                Id = userId.ToString(),
                UserName = userName,
                Email = userName
            };

            Db.Users.Add(identityUser);
            await Db.SaveChangesAsync();

            // Act
            var result = await AuthorInfoService.GetOrCreateAsync(userId);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual(userId.ToString(), result.Id, "ID should match.");
            Assert.AreEqual(userName, result.AuthorName, "AuthorName should be set from UserName.");
            Assert.AreEqual(string.Empty, result.AuthorDescription, "AuthorDescription should be empty string.");

            // Verify it was saved to database
            var savedAuthor = await Db.AuthorInfos.FirstOrDefaultAsync(a => a.Id == userId.ToString());
            Assert.IsNotNull(savedAuthor, "AuthorInfo should be saved to database.");
        }

        /// <summary>
        /// Verifies that GetOrCreateAsync uses email when username is null.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreateAsync_UserNameIsNull_UsesEmail()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var email = "test@example.com";

            var identityUser = new IdentityUser
            {
                Id = userId.ToString(),
                UserName = null,
                Email = email
            };

            Db.Users.Add(identityUser);
            await Db.SaveChangesAsync();

            // Act
            var result = await AuthorInfoService.GetOrCreateAsync(userId);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual(email, result.AuthorName, "AuthorName should be set from Email.");
        }

        /// <summary>
        /// Verifies that GetOrCreateAsync uses userId string when both username and email are null.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreateAsync_UserNameAndEmailAreNull_UsesUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var identityUser = new IdentityUser
            {
                Id = userId.ToString(),
                UserName = null,
                Email = null
            };

            Db.Users.Add(identityUser);
            await Db.SaveChangesAsync();

            // Act
            var result = await AuthorInfoService.GetOrCreateAsync(userId);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual(userId.ToString(), result.AuthorName, "AuthorName should be userId string.");
        }

        /// <summary>
        /// Verifies that GetOrCreateAsync caches the result after first retrieval.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreateAsync_CachesResult_ReturnsFromCacheOnSecondCall()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var authorInfo = new AuthorInfo
            {
                Id = userId.ToString(),
                AuthorName = "Cached Author",
                AuthorDescription = "Cached Description"
            };

            Db.AuthorInfos.Add(authorInfo);
            await Db.SaveChangesAsync();

            // Act - First call should hit database
            var firstResult = await AuthorInfoService.GetOrCreateAsync(userId);

            // Clear EF tracking to ensure DB would return updated value
            Db.ChangeTracker.Clear();

            // Modify the database record using a fresh query
            var dbRecord = await Db.AuthorInfos.FirstOrDefaultAsync(a => a.Id == userId.ToString());
            dbRecord.AuthorName = "Modified Author";
            await Db.SaveChangesAsync();

            // Second call should return cached value
            var secondResult = await AuthorInfoService.GetOrCreateAsync(userId);

            // Assert
            Assert.AreEqual("Cached Author", secondResult.AuthorName,
                "Should return cached value, not modified database value.");
            Assert.AreSame(firstResult, secondResult,
                "Should return same cached instance.");
        }

        /// <summary>
        /// Verifies that GetOrCreateAsync handles cache expiration correctly.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreateAsync_CacheExpires_RetrievesFromDatabaseAgain()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userName = "testuser@example.com";

            var identityUser = new IdentityUser
            {
                Id = userId.ToString(),
                UserName = userName,
                Email = userName
            };

            Db.Users.Add(identityUser);

            var authorInfo = new AuthorInfo
            {
                Id = userId.ToString(),
                AuthorName = "Original Name",
                AuthorDescription = string.Empty
            };

            Db.AuthorInfos.Add(authorInfo);
            await Db.SaveChangesAsync();

            // Act - First call
            var firstResult = await AuthorInfoService.GetOrCreateAsync(userId);

            // Clear cache to simulate expiration
            Cache.Remove(userId.ToString());

            // Modify database
            authorInfo.AuthorName = "Updated Name";
            await Db.SaveChangesAsync();

            // Second call after cache clear
            var secondResult = await AuthorInfoService.GetOrCreateAsync(userId);

            // Assert
            Assert.AreEqual("Updated Name", secondResult.AuthorName, "Should retrieve updated value from database after cache expiration.");
        }

        /// <summary>
        /// Verifies that multiple calls with different user IDs work correctly.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreateAsync_MultipleUsers_HandlesCorrectly()
        {
            // Arrange
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();

            var user1 = new IdentityUser
            {
                Id = userId1.ToString(),
                UserName = "user1@example.com",
                Email = "user1@example.com"
            };

            var user2 = new IdentityUser
            {
                Id = userId2.ToString(),
                UserName = "user2@example.com",
                Email = "user2@example.com"
            };

            Db.Users.AddRange(user1, user2);
            await Db.SaveChangesAsync();

            // Act
            var result1 = await AuthorInfoService.GetOrCreateAsync(userId1);
            var result2 = await AuthorInfoService.GetOrCreateAsync(userId2);

            // Assert
            Assert.IsNotNull(result1, "First result should not be null.");
            Assert.IsNotNull(result2, "Second result should not be null.");
            Assert.AreNotEqual(result1.Id, result2.Id, "Results should have different IDs.");
            Assert.AreEqual("user1@example.com", result1.AuthorName, "First author name should match.");
            Assert.AreEqual("user2@example.com", result2.AuthorName, "Second author name should match.");
        }

        /// <summary>
        /// Verifies that GetOrCreateAsync handles concurrent calls correctly.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreateAsync_ConcurrentCalls_HandlesCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userName = "concurrent@example.com";

            var identityUser = new IdentityUser
            {
                Id = userId.ToString(),
                UserName = userName,
                Email = userName
            };

            Db.Users.Add(identityUser);
            await Db.SaveChangesAsync();

            // Act - Make multiple concurrent calls
            var tasks = new Task<AuthorInfo>[10];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = AuthorInfoService.GetOrCreateAsync(userId);
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            var count = await Db.AuthorInfos.CountAsync(a => a.Id == userId.ToString());
            Assert.AreEqual(1, count,
                "Exactly one AuthorInfo record should exist despite concurrent calls.");

            // All results should reference the same or equal data
            foreach (var result in results)
            {
                Assert.AreEqual(userId.ToString(), result.Id);
                Assert.AreEqual(userName, result.AuthorName);
            }
        }

        /// <summary>
        /// Verifies constructor initializes dependencies correctly.
        /// </summary>
        [TestMethod]
        public void Constructor_InitializesDependencies_Successfully()
        {
            // Arrange & Act
            var testService = new AuthorInfoService(Db, Cache);

            // Assert
            Assert.IsNotNull(testService, "Service should be instantiated successfully.");
        }

        /// <summary>
        /// Verifies that AuthorDescription is always set to empty string for new records.
        /// </summary>
        [TestMethod]
        public async Task GetOrCreateAsync_NewRecord_AuthorDescriptionIsEmptyString()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var identityUser = new IdentityUser
            {
                Id = userId.ToString(),
                UserName = "testuser",
                Email = "test@example.com"
            };

            Db.Users.Add(identityUser);
            await Db.SaveChangesAsync();

            // Act
            var result = await AuthorInfoService.GetOrCreateAsync(userId);

            // Assert
            Assert.IsNotNull(result, "Result should not be null.");
            Assert.AreEqual(string.Empty, result.AuthorDescription, "AuthorDescription should be empty string.");
            Assert.IsNotNull(result.AuthorDescription, "AuthorDescription should not be null.");
        }
    }
}