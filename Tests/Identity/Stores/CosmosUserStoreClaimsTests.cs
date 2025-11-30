using Sky.Tests.Identity;
using System.Security.Claims;

namespace Sky.Tests.Identity.Stores
{
    [TestClass()]
    public class CosmosUserStoreClaimsTests : CosmosIdentityTestsBase
    {
        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task Consolidated_ClaimsAsync_CRUD_Tests(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var userStore = _testUtilities.GetUserStore(connectionString, databaseName);
            var user1 = await GetMockRandomUserAsync(userStore);

            // Clean up claims before starting
            var claims = await userStore.GetClaimsAsync(user1, default);
            if (claims.Any())
            {
                await userStore.RemoveClaimsAsync(user1, claims, default);
            }

            var claim = new Claim[] { GetMockClaim("a"), GetMockClaim("b"), GetMockClaim("c") };
            var newClaim = GetMockClaim("d");

            await userStore.AddClaimsAsync(user1, claim, default);

            // Act - Create
            var result2 = await userStore.GetClaimsAsync(user1, default);

            // Assert - Create
            Assert.AreEqual(3, result2.Count, $"Expected 3 claims for {providerName}");

            // Act - Replace
            await userStore.ReplaceClaimAsync(user1, claim.FirstOrDefault(), newClaim, default);

            // Test - Replace
            var result3 = await userStore.GetClaimsAsync(user1, default);
            Assert.IsFalse(result3.Any(a => a.Type == claim.FirstOrDefault()?.Type), 
                $"Old claim should be removed for {providerName}");

            var testAny = result3.Any(a => a.Type == newClaim.Type);
            if (!testAny)
            {
                throw new Exception($"Replace failed for {providerName} with {result3.Count} claims with types {string.Join(",", result3.Select(s => s.Type).ToArray())}");
            }

            Assert.IsTrue(testAny, $"New claim should exist for {providerName}");

            // Act - Delete
            await userStore.RemoveClaimsAsync(user1, result3, default);
            var result4 = await userStore.GetClaimsAsync(user1, default);
            Assert.IsFalse(result4.Any(), $"All claims should be removed for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task GetUsersForClaimAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            
            var val = Guid.NewGuid().ToString();
            var claims = new Claim[] { new Claim(val, val) };
            
            using (var userStore = _testUtilities.GetUserStore(connectionString, databaseName))
            {
                var user1 = await GetMockRandomUserAsync(userStore);
                await userStore.AddClaimsAsync(user1, claims, default);
            }

            using (var userStore = _testUtilities.GetUserStore(connectionString, databaseName))
            {
                var user2 = await GetMockRandomUserAsync(userStore);
                await userStore.AddClaimsAsync(user2, claims, default);
            }

            using (var userStore = _testUtilities.GetUserStore(connectionString, databaseName))
            {
                // Act
                var result1 = await userStore.GetUsersForClaimAsync(claims.FirstOrDefault()!, default);
                
                // Assert
                Assert.AreEqual(2, result1.Count, $"Expected 2 users with claim for {providerName}");
            }

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }
    }
}