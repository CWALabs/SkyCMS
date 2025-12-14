using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AspNetCore.Identity.CosmosDb.Tests.Net9.Stores
{
    [TestClass()]
    [DoNotParallelize]
    public class CosmosRoleStoreTests : CosmosIdentityTestsBase
    {
        /// <summary>
        /// Provides test data for all available database providers
        /// </summary>
        public static IEnumerable<object[]> GetTestProviders()
        {
            var providers = TestUtilities.GetAvailableProviders();

            foreach (var provider in providers)
            {
                yield return new object[] { provider };
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // Initialize utilities if not already done
            if (_testUtilities == null)
            {
                _testUtilities = new TestUtilities();
            }
            if (_random == null)
            {
                _random = new Random();
            }
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityRole"/> for unit testing purposes
        /// </summary>
        /// <returns></returns>
        private async Task<IdentityRole> GetMockRandomRoleAsync(string connectionString, string databaseName)
        {
            // Use GUID to ensure uniqueness instead of random numbers
            var role = new IdentityRole($"HUB{Guid.NewGuid():N}");
            role.NormalizedName = role.Name.ToUpper();
            using var roleStore = _testUtilities.GetRoleStore(connectionString, databaseName);
            var result = await roleStore.CreateAsync(role);
            Assert.IsTrue(result.Succeeded);
            return role;
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task CreateAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Act
            // Create a bunch of roles in rapid succession
            using var dbContext = _testUtilities.GetDbContext(provider.ConnectionString, provider.DatabaseName);
            var currentCount = await dbContext.Roles.CountAsync();
            for (int i = 0; i < 35; i++)
            {
                var r = await GetMockRandomRoleAsync(provider.ConnectionString, provider.DatabaseName);
            }

            // Assert
            Assert.AreEqual(35 + currentCount, await dbContext.Roles.CountAsync(), $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task DeleteAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            using var dbContext = _testUtilities.GetDbContext(provider.ConnectionString, provider.DatabaseName);
            var role = await GetMockRandomRoleAsync(roleStore);
            var user = await GetMockRandomUserAsync(userStore);
            var roleClaim = GetMockClaim();
            await roleStore.AddClaimAsync(role, roleClaim);
            await userStore.AddToRoleAsync(user, role.NormalizedName);

            var roleId = role.Id;

            // Act
            var result = await roleStore.DeleteAsync(role);

            // Assert
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(await dbContext.Roles.Where(a => a.Name == role.Name).CountAsync() == 0, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(await dbContext.RoleClaims.Where(a => a.RoleId == roleId).CountAsync() == 0, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(await dbContext.UserRoles.Where(a => a.RoleId == roleId).CountAsync() == 0, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task FindByIdAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var role = await GetMockRandomRoleAsync(provider.ConnectionString, provider.DatabaseName);

            // Act
            var r = await roleStore.FindByIdAsync(role.Id);

            // Assert
            Assert.AreEqual(role.Id, r.Id, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task FindByNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var role = await GetMockRandomRoleAsync(provider.ConnectionString, provider.DatabaseName);

            // Act
            var r = await roleStore.FindByNameAsync(role.Name.ToUpper());

            // Assert
            Assert.AreEqual(role.Id, r.Id, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetNormalizedRoleNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var role = await GetMockRandomRoleAsync(provider.ConnectionString, provider.DatabaseName);

            // Act
            var r = await roleStore.FindByNameAsync(role.Name.ToUpper());

            // Assert
            Assert.AreEqual(role.Id, r.Id, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetRoleIdAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var role = await GetMockRandomRoleAsync(provider.ConnectionString, provider.DatabaseName);

            // Act
            var result = await roleStore.GetRoleIdAsync(role);

            // Assert
            Assert.AreEqual(role.Id, result, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetRoleNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var role = await GetMockRandomRoleAsync(provider.ConnectionString, provider.DatabaseName);

            // Act
            var result = await roleStore.GetRoleNameAsync(role);

            // Assert
            Assert.AreEqual(role.Name, result, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetNormalizedRoleNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var role = await GetMockRandomRoleAsync(provider.ConnectionString, provider.DatabaseName);
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            // Act
            await roleStore.SetNormalizedRoleNameAsync(role, newName.ToUpper(), default);

            // Assert
            var result = await roleStore.GetNormalizedRoleNameAsync(role);
            Assert.AreEqual(newName.ToUpper(), result, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetRoleNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var role = await GetMockRandomRoleAsync(provider.ConnectionString, provider.DatabaseName);
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            // Act
            await roleStore.SetRoleNameAsync(role, newName);

            // Assert
            var result1 = await roleStore.GetRoleNameAsync(role);
            Assert.AreEqual(newName, result1, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task UpdateAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var role = await GetMockRandomRoleAsync(roleStore);
            var newName = $"WOW{Guid.NewGuid().ToString()}";

            role.Name = newName;
            role.NormalizedName = newName.ToLower();

            // Act
            var result = await roleStore.UpdateAsync(role);

            // Assert
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
            role = await roleStore.FindByIdAsync(role.Id);
            Assert.AreEqual(newName, role.Name, $"Failed for provider: {provider.DisplayName}");
            Assert.AreEqual(newName.ToLower(), role.NormalizedName, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetClaimsAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var claims = new Claim[] { GetMockClaim(), GetMockClaim(), GetMockClaim() };
            var role = await GetMockRandomRoleAsync(provider.ConnectionString, provider.DatabaseName);
            await roleStore.AddClaimAsync(role, claims[0], default);
            await roleStore.AddClaimAsync(role, claims[1], default);
            await roleStore.AddClaimAsync(role, claims[2], default);

            // Act
            var result2 = await roleStore.GetClaimsAsync(role, default);

            // Assert
            Assert.AreEqual(3, result2.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task AddClaimAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Assert
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var role = await GetMockRandomRoleAsync(provider.ConnectionString, provider.DatabaseName);
            var claim = GetMockClaim();

            // Act
            await roleStore.AddClaimAsync(role, claim, default);

            // Assert
            var result2 = await roleStore.GetClaimsAsync(role, default);
            Assert.AreEqual(1, result2.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task RemoveClaimAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Assert
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var role = await GetMockRandomRoleAsync(provider.ConnectionString, provider.DatabaseName);
            var claim = GetMockClaim();
            await roleStore.AddClaimAsync(role, claim, default);
            var result2 = await roleStore.GetClaimsAsync(role, default);
            Assert.AreEqual(1, result2.Count, $"Failed for provider: {provider.DisplayName}");

            // Act
            await roleStore.RemoveClaimAsync(role, claim, default);

            // Assert
            var result3 = await roleStore.GetClaimsAsync(role, default);
            Assert.AreEqual(0, result3.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task QueryRolesTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roletore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var user1 = await GetMockRandomRoleAsync(roletore);

            // Act
            var result = await roletore.Roles.ToListAsync();

            // Assert
            Assert.IsTrue(result.Count > 0, $"Failed for provider: {provider.DisplayName}");
        }
    }
}
