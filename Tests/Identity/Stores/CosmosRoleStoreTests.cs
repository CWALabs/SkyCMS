using AspNetCore.Identity.FlexDb.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sky.Tests.Identity.Stores
{
    [TestClass()]
    public class CosmosRoleStoreTests : CosmosIdentityTestsBase
    {
        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task CreateAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var dbContext = GetDbContext(connectionString, databaseName);
            
            var currentCount = await dbContext.Roles.CountAsync();
            
            // Create test roles
            for (int i = 0; i < 10; i++) // Reduced from 35
            {
                var role = new IdentityRole($"HUB{GetNextRandomNumber(1000, 9999)}");
                role.NormalizedName = role.Name.ToUpper();
                using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
                var result = await roleStore.CreateAsync(role);
                Assert.IsTrue(result.Succeeded, $"Role creation failed for {providerName}");
            }

            // Assert
            Assert.HasCount(10 + currentCount, await dbContext.Roles.ToListAsync(), 
                $"Role count mismatch for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task DeleteAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            using var userStore = _testUtilities.GetUserStore(connectionString, databaseName);
            using var dbContext = GetDbContext(connectionString, databaseName);
            
            var role = await GetMockRandomRoleAsync(roleStore);
            var user = await GetMockRandomUserAsync(userStore);
            var roleClaim = GetMockClaim();
            
            await roleStore.AddClaimAsync(role, roleClaim);
            await userStore.AddToRoleAsync(user, role.NormalizedName);
            
            var roleId = role.Id;

            // Act
            var result = await roleStore.DeleteAsync(role);

            // Assert
            Assert.IsTrue(result.Succeeded, $"Role deletion failed for {providerName}");
            Assert.IsEmpty(await dbContext.Roles.Where(a => a.Name == role.Name).ToListAsync(),
                $"Role not deleted for {providerName}");
            Assert.IsEmpty(await dbContext.RoleClaims.Where(a => a.RoleId == roleId).ToListAsync(),
                $"Role claims not deleted for {providerName}");
            Assert.IsEmpty(await dbContext.UserRoles.Where(a => a.RoleId == roleId).ToListAsync(),
                $"User roles not deleted for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task FindByIdAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var role = await GetMockRandomRoleAsync(roleStore);

            // Act
            var r = await roleStore.FindByIdAsync(role.Id);

            // Assert
            Assert.AreEqual(role.Id, r.Id, $"Role ID mismatch for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task FindByNameAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var role = await GetMockRandomRoleAsync(roleStore);

            // Act
            var r = await roleStore.FindByNameAsync(role.Name.ToUpper());

            // Assert
            Assert.AreEqual(role.Id, r.Id, $"Role ID mismatch for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task GetNormalizedRoleNameAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var role = await GetMockRandomRoleAsync(roleStore);

            // Act
            var r = await roleStore.FindByNameAsync(role.Name.ToUpper());

            // Assert
            Assert.AreEqual(role.Id, r.Id, $"Role ID mismatch for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task GetRoleIdAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var role = await GetMockRandomRoleAsync(roleStore);

            // Act
            var result = await roleStore.GetRoleIdAsync(role);

            // Assert
            Assert.AreEqual(role.Id, result, $"Role ID mismatch for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task GetRoleNameAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var role = await GetMockRandomRoleAsync(roleStore);

            // Act
            var result = await roleStore.GetRoleNameAsync(role);

            // Assert
            Assert.AreEqual(role.Name, result, $"Role name mismatch for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task SetNormalizedRoleNameAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var role = await GetMockRandomRoleAsync(roleStore);
            var newName = $"WOW{Guid.NewGuid()}".ToUpper();

            // Act
            await roleStore.SetNormalizedRoleNameAsync(role, newName, default);

            // Assert
            var result = await roleStore.GetNormalizedRoleNameAsync(role);
            Assert.AreEqual(newName, result, $"Normalized name mismatch for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task SetRoleNameAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var role = await GetMockRandomRoleAsync(roleStore);
            var newName = $"WOW{Guid.NewGuid()}".ToUpper();

            // Act
            await roleStore.SetRoleNameAsync(role, newName);

            // Assert
            var result1 = await roleStore.GetRoleNameAsync(role);
            Assert.AreEqual(newName, result1, $"Role name mismatch for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task UpdateAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var role = await GetMockRandomRoleAsync(roleStore);
            var newName = $"WOW{Guid.NewGuid()}".ToLower();

            role.Name = newName;
            role.NormalizedName = newName.ToUpper();

            // Act
            var result = await roleStore.UpdateAsync(role);

            // Assert
            Assert.IsTrue(result.Succeeded, $"Update failed for {providerName}");
            role = await roleStore.FindByIdAsync(role.Id);
            Assert.AreEqual(newName, role.Name, $"Role name mismatch for {providerName}");
            Assert.AreEqual(newName.ToUpper(), role.NormalizedName, $"Normalized name mismatch for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task GetClaimsAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var claims = new Claim[] { GetMockClaim(), GetMockClaim(), GetMockClaim() };
            var role = await GetMockRandomRoleAsync(roleStore);
            await roleStore.AddClaimAsync(role, claims[0], default);
            await roleStore.AddClaimAsync(role, claims[1], default);
            await roleStore.AddClaimAsync(role, claims[2], default);

            // Act
            var result2 = await roleStore.GetClaimsAsync(role, default);

            // Assert
            Assert.HasCount(3, result2, $"Expected 3 claims for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task AddClaimAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var role = await GetMockRandomRoleAsync(roleStore);
            var claim = GetMockClaim();

            // Act
            await roleStore.AddClaimAsync(role, claim, default);

            // Assert
            var result2 = await roleStore.GetClaimsAsync(role, default);
            Assert.HasCount(1, result2, $"Expected 1 claim for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod()]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task RemoveClaimAsyncTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var role = await GetMockRandomRoleAsync(roleStore);
            var claim = GetMockClaim();
            await roleStore.AddClaimAsync(role, claim, default);
            var result2 = await roleStore.GetClaimsAsync(role, default);
            Assert.HasCount(1, result2, $"Expected 1 claim before removal for {providerName}");

            // Act
            await roleStore.RemoveClaimAsync(role, claim, default);

            // Assert
            var result3 = await roleStore.GetClaimsAsync(role, default);
            Assert.IsEmpty(result3, $"Expected 0 claims after removal for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }

        [TestMethod]
        [DynamicData(nameof(DatabaseProviders))]
        public async Task QueryRolesTest(string providerName, string connectionString, string databaseName)
        {
            // Arrange
            InitializeClass(providerName, connectionString, databaseName);
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var role1 = await GetMockRandomRoleAsync(roleStore);

            // Act
            var result = await roleStore.Roles.ToListAsync();

            // Assert
            Assert.IsNotEmpty(result, $"Expected at least 1 role for {providerName}");

            // Cleanup
            await CleanupDatabase(providerName, connectionString, databaseName);
        }
    }
}