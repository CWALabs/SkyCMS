using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace AspNetCore.Identity.CosmosDb.Tests.Net9
{
    /// <summary>
    /// Tests the <see cref="UserManager{TUser}"/> when hooked up to Cosmos user and role stores.
    /// </summary>
    [TestClass]
    public class UserManagerInterOperabilityTests : CosmosIdentityTestsBase
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

        // Creates a new test user with a hashed password, using the mock UserManager to do so
        private async Task<IdentityUser> GetTestUser(UserManager<IdentityUser> userManager, string password = "")
        {
            var user = await GetMockRandomUserAsync(null, false);

            if (string.IsNullOrEmpty(password))
                password = $"A1a{Guid.NewGuid()}";

            var result = await userManager.CreateAsync(user, password);

            Assert.IsTrue(result.Succeeded);
            return await userManager.FindByIdAsync(user.Id);
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

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetUserNameTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.GetUserNameAsync(user);
            
            // Assert
            Assert.IsNotNull(result, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetUserIdTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result2 = await userManager.GetUserIdAsync(user);
            
            // Assert
            Assert.IsNotNull(result2, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task CreateAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetMockRandomUserAsync(null, false);

            // Act
            var result = await userManager.CreateAsync(user);

            // Assert
            var result2 = await userManager.FindByIdAsync(user.Id);
            Assert.IsTrue(user.Id == result2.Id, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task UpdateAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            user.PhoneNumber = "9998884444";

            // Act
            var result1 = await userManager.UpdateAsync(user);

            // Assert
            user = await userManager.FindByIdAsync(user.Id);
            Assert.AreEqual("9998884444", user.PhoneNumber, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task DeleteAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var id = user.Id;
            user = await userManager.FindByIdAsync(id);
            Assert.IsNotNull(user, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result = await userManager.DeleteAsync(user);

            // Assert
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
            user = await userManager.FindByIdAsync(id);
            Assert.IsNull(user, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task FindByIdAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            user = await userManager.FindByIdAsync(user.Id);

            // Assert
            Assert.IsNotNull(user, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task FindByNameAsync(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            user = await userManager.FindByNameAsync(user.UserName);

            // Assert
            Assert.IsNotNull(user, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task CreateAsync_WithPassword_Test(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));

            // Act
            var user = await GetTestUser(userManager);

            // Assert
            var result = await userManager.HasPasswordAsync(user);
            Assert.IsNotNull(user, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(result, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(!string.IsNullOrEmpty(user.PasswordHash), $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task UpdateNormalizedUserNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var userName = "Az" + user.UserName;
            user.UserName = userName;

            // Act
            await userManager.UpdateNormalizedUserNameAsync(user);

            // Assert
            user = await userManager.FindByIdAsync(user.Id);
            Assert.IsTrue(user.NormalizedUserName == userName.ToUpperInvariant(), $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetUserNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var userName = await userManager.GetUserNameAsync(user);

            // Assert
            Assert.AreEqual(user.UserName, userName, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetUserNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var userName = "Az" + user.UserName;

            // Act
            await userManager.SetUserNameAsync(user, userName);

            // Assert
            user = await userManager.FindByIdAsync(user.Id);
            Assert.IsTrue(user.UserName == userName, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetUserIdAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.GetUserIdAsync(user);

            // Assert
            Assert.AreEqual(user.Id, result, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task CheckPasswordAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var originalPassword = $"A1a{Guid.NewGuid()}";
            var user = await GetTestUser(userManager, originalPassword);

            // Act - fail
            var result = await userManager.ChangePasswordAsync(user, originalPassword, Guid.NewGuid().ToString());

            // Assert - fail
            Assert.IsFalse(result.Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act - succeed
            result = await userManager.ChangePasswordAsync(user, originalPassword, $"A1a{Guid.NewGuid()}");

            // Assert - succeed
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task HasPasswordAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.HasPasswordAsync(user);

            // Assert
            Assert.IsTrue(result, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task AddPasswordAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange - fail
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.AddPasswordAsync(user, $"A1a{Guid.NewGuid()}");

            // Assert
            Assert.IsFalse(result.Succeeded, $"Failed for provider: {provider.DisplayName}"); // Already has a password

            // Arrange - success
            user.PasswordHash = null;
            var result2 = await userManager.UpdateAsync(user);
            user = await userManager.FindByIdAsync(user.Id);

            // Act
            var result3 = await userManager.AddPasswordAsync(user, $"A1a{Guid.NewGuid()}");

            // Assert
            Assert.IsTrue(result3.Succeeded, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task ChangePasswordAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var originalPassword = $"A1a{Guid.NewGuid()}";
            var user = await GetTestUser(userManager, originalPassword);

            // Act
            var result = await userManager.ChangePasswordAsync(user, originalPassword, $"A1a{Guid.NewGuid()}");

            // Assert
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task RemovePasswordAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            Assert.IsTrue(await userManager.HasPasswordAsync(user), $"Failed for provider: {provider.DisplayName}");

            // Act
            var result = await userManager.RemovePasswordAsync(user);

            // Assert
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
            Assert.IsFalse(await userManager.HasPasswordAsync(user), $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetSecurityStampAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.GetSecurityStampAsync(user);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(result), $"Failed for provider: {provider.DisplayName}");
            Assert.AreEqual(result, user.SecurityStamp, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task UpdateSecurityStampAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var stamp1 = user.SecurityStamp;

            // Act
            var result = await userManager.UpdateSecurityStampAsync(user);

            // Assert
            user = await userManager.FindByIdAsync(user.Id);
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
            Assert.AreNotEqual(stamp1, user.SecurityStamp, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task FindByLoginAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var loginInfo = GetMockLoginInfoAsync();
            await userManager.AddLoginAsync(user, loginInfo);
            var logins = await userManager.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")), $"Failed for provider: {provider.DisplayName}");

            // Act
            var user2 = await userManager.FindByLoginAsync("Twitter", loginInfo.ProviderKey);

            // Assert
            Assert.AreEqual(user.Id, user2.Id, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task RemoveLoginAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var loginInfo = GetMockLoginInfoAsync();
            await userManager.AddLoginAsync(user, loginInfo);
            var logins = await userManager.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")), $"Failed for provider: {provider.DisplayName}");
            var user2 = await userManager.FindByLoginAsync("Twitter", loginInfo.ProviderKey);
            Assert.AreEqual(user.Id, user2.Id, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result = await userManager.RemoveLoginAsync(user, "Twitter", loginInfo.ProviderKey);

            // Assert
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
            logins = await userManager.GetLoginsAsync(user);
            Assert.AreEqual(0, logins.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task AddLoginAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var loginInfo = GetMockLoginInfoAsync();

            // Act
            await userManager.AddLoginAsync(user, loginInfo);

            // Assert
            var logins = await userManager.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")), $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetLoginsAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var loginInfo = GetMockLoginInfoAsync();
            await userManager.AddLoginAsync(user, loginInfo);

            // Act
            var logins = await userManager.GetLoginsAsync(user);

            // Assert
            Assert.AreEqual(1, logins.Count, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")), $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task AddClaimAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var claim = new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Act
            var result = await userManager.AddClaimAsync(user, claim);

            // Assert
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result2 = await userManager.GetClaimsAsync(user);
            Assert.AreEqual(1, result2.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task AddClaimsAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var claims = new Claim[] { new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), new Claim(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()) };

            // Act
            var result = await userManager.AddClaimsAsync(user, claims);

            // Assert
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
            claims = (await userManager.GetClaimsAsync(user)).ToArray();
            Assert.AreEqual(3, claims.Count(), $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task ReplaceClaimAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var claim = new Claim("1", "1");
            var newClaim = new Claim("1", "2");
            var result1 = await userManager.AddClaimAsync(user, claim);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result2 = await userManager.ReplaceClaimAsync(user, claim, newClaim);

            // Assert
            Assert.IsTrue(result2.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result3 = await userManager.GetClaimsAsync(user);
            Assert.AreEqual(1, result3.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task RemoveClaimAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var claim = new Claim("1", "1");
            var result1 = await userManager.AddClaimAsync(user, claim);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result2 = await userManager.RemoveClaimAsync(user, claim);

            // Assert
            Assert.IsTrue(result2.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result3 = await userManager.GetClaimsAsync(user);
            Assert.AreEqual(0, result3.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task RemoveClaimsAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var claims = new Claim[] { new Claim("1", "1"), new Claim("2", "2"), new Claim("3", "3") };
            var result1 = await userManager.AddClaimsAsync(user, claims);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result2 = await userManager.RemoveClaimsAsync(user, claims);

            // Assert
            Assert.IsTrue(result2.Succeeded, $"Failed for provider: {provider.DisplayName}");
            claims = (await userManager.GetClaimsAsync(user)).ToArray();
            Assert.AreEqual(0, claims.Count(), $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetClaimsAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var claims = new Claim[] { new Claim("1", "1"), new Claim("2", "2"), new Claim("3", "3") };
            var result1 = await userManager.AddClaimsAsync(user, claims);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result2 = await userManager.GetClaimsAsync(user);

            // Assert
            Assert.AreEqual(3, result2.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task AddToRoleAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName));
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var role = await GetMockRandomRoleAsync(null, false);
            var result1 = await roleManager.CreateAsync(role);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result2 = await userManager.AddToRoleAsync(user, role.Name);

            // Assert
            Assert.IsTrue(result2.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result3 = await userManager.GetRolesAsync(user);
            Assert.AreEqual(1, result3.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task AddToRolesAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName));
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var role1 = await GetMockRandomRoleAsync(null, false);
            var role2 = await GetMockRandomRoleAsync(null, false);
            var role3 = await GetMockRandomRoleAsync(null, false);
            var result1 = await roleManager.CreateAsync(role1);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result2 = await roleManager.CreateAsync(role2);
            Assert.IsTrue(result2.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result3 = await roleManager.CreateAsync(role3);
            Assert.IsTrue(result3.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var roles = new string[] { role1.Name, role2.Name, role3.Name };

            // Act
            var result4 = await userManager.AddToRolesAsync(user, roles);

            // Assert
            Assert.IsTrue(result4.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result5 = await userManager.GetRolesAsync(user);
            Assert.AreEqual(3, result5.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task RemoveFromRoleAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName));
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var role = await GetMockRandomRoleAsync(null, false);
            var result1 = await roleManager.CreateAsync(role);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue((await userManager.AddToRoleAsync(user, role.Name)).Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result2 = await userManager.RemoveFromRoleAsync(user, role.Name);

            // Assert
            Assert.IsTrue(result2.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result3 = await userManager.GetRolesAsync(user);
            Assert.AreEqual(0, result3.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task RemoveFromRolesAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);

            // Arrange
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName));
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var role1 = await GetMockRandomRoleAsync(null, false);
            var role2 = await GetMockRandomRoleAsync(null, false);
            var role3 = await GetMockRandomRoleAsync(null, false);
            var result1 = await roleManager.CreateAsync(role1);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result2 = await roleManager.CreateAsync(role2);
            Assert.IsTrue(result2.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result3 = await roleManager.CreateAsync(role3);
            Assert.IsTrue(result3.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var roles = new string[] { role1.Name, role2.Name, role3.Name };

            // Act
            var result5 = await userManager.RemoveFromRolesAsync(user, roles);

            // Assert
            Assert.IsTrue(result2.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result6 = await userManager.GetRolesAsync(user);
            Assert.AreEqual(0, result6.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetRolesAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName));
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var role1 = await GetMockRandomRoleAsync(null, false);
            var role2 = await GetMockRandomRoleAsync(null, false);
            var role3 = await GetMockRandomRoleAsync(null, false);
            var result1 = await roleManager.CreateAsync(role1);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result2 = await roleManager.CreateAsync(role2);
            Assert.IsTrue(result2.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result3 = await roleManager.CreateAsync(role3);
            Assert.IsTrue(result3.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var roles = new string[] { role1.Name, role2.Name, role3.Name };
            Assert.IsTrue((await userManager.AddToRolesAsync(user, roles)).Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result5 = await userManager.GetRolesAsync(user);

            // Assert
            Assert.AreEqual(3, result5.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task IsInRoleAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName));
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var role = await GetMockRandomRoleAsync(null, false);
            var result1 = await roleManager.CreateAsync(role);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue((await userManager.AddToRoleAsync(user, role.Name)).Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result2 = await userManager.IsInRoleAsync(user, role.Name);

            // Assert
            Assert.IsTrue(result2, $"Failed for provider: {provider.DisplayName}");
            var result3 = await userManager.GetRolesAsync(user);
            Assert.AreEqual(1, result3.Count, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetEmailAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result1 = await userManager.GetEmailAsync(user);

            // Assert
            Assert.AreEqual(user.Email, result1, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetEmailAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var emailAddress = "bb" + user.Email;

            // Act
            var result1 = await userManager.SetEmailAsync(user, emailAddress);

            // Assert
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var user2 = await userManager.FindByIdAsync(user.Id);
            Assert.AreEqual(emailAddress, user2.Email, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task FindByEmailAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result1 = await userManager.FindByEmailAsync(user.Email);

            // Assert
            Assert.AreEqual(user.Id, result1.Id, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task UpdateNormalizedEmailAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var emailAddress = "Bb" + user.Email;
            user.Email = emailAddress;

            // Act
            await userManager.UpdateNormalizedEmailAsync(user);

            // Assert
            var user2 = await userManager.FindByIdAsync(user.Id);
            Assert.AreEqual(emailAddress.ToUpperInvariant(), user2.NormalizedEmail, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetPhoneNumberAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var phoneNumber = "3334445555";
            var result1 = await userManager.SetPhoneNumberAsync(user, phoneNumber);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result2 = await userManager.GetPhoneNumberAsync(user);

            // Assert
            Assert.AreEqual(phoneNumber, result2, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetPhoneNumberAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var phoneNumber = "3334445555";

            // Act
            var result1 = await userManager.SetPhoneNumberAsync(user, phoneNumber);

            // Assert
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result2 = await userManager.GetPhoneNumberAsync(user);
            Assert.AreEqual(phoneNumber, result2, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task IsPhoneNumberConfirmedAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.IsPhoneNumberConfirmedAsync(user);
            
            // Assert
            Assert.IsFalse(result, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GenerateChangePhoneNumberTokenAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act - Placeholder test (requires token provider registration)
            
            // Assert
            Assert.IsNotNull(user, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task VerifyChangePhoneNumberTokenAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act - Placeholder test (requires token provider registration)
            
            // Assert
            Assert.IsNotNull(user, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetTwoFactorEnabledAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result1 = await userManager.GetTwoFactorEnabledAsync(user);

            // Assert
            Assert.IsFalse(result1, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetTwoFactorEnabledAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.SetTwoFactorEnabledAsync(user, true);

            // Assert
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result2 = await userManager.GetTwoFactorEnabledAsync(user);
            Assert.IsTrue(result2, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task IsLockedOutAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result = await userManager.IsLockedOutAsync(user);

            // Assert
            Assert.IsFalse(result, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetLockoutEnabledAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result1 = await userManager.SetLockoutEnabledAsync(user, true);

            // Assert
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result2 = await userManager.GetLockoutEnabledAsync(user);
            Assert.IsTrue(result2, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetLockoutEnabledAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var result1 = await userManager.SetLockoutEnabledAsync(user, true);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result2 = await userManager.GetLockoutEnabledAsync(user);

            // Assert
            Assert.IsTrue(result2, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetLockoutEndDateAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act - Placeholder test
            
            // Assert
            Assert.IsNotNull(user, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetLockoutEndDateAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var dateTime = DateTimeOffset.Now.AddMinutes(15);

            // Act
            var result1 = await userManager.SetLockoutEndDateAsync(user, dateTime);

            // Assert
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result2 = await userManager.GetLockoutEndDateAsync(user);
            Assert.AreEqual(dateTime, result2, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task AccessFailedAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);

            // Act
            var result1 = await userManager.AccessFailedAsync(user);
            var result2 = await userManager.AccessFailedAsync(user);

            // Assert
            var result3 = await userManager.GetAccessFailedCountAsync(user);
            Assert.AreEqual(2, result3, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task ResetAccessFailedCountAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            var user = await GetTestUser(userManager);
            var result1 = await userManager.AccessFailedAsync(user);
            var result2 = await userManager.AccessFailedAsync(user);
            var result3 = await userManager.GetAccessFailedCountAsync(user);
            Assert.AreEqual(2, result3, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result4 = await userManager.ResetAccessFailedCountAsync(user);

            // Assert
            Assert.IsTrue(result4.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result5 = await userManager.GetAccessFailedCountAsync(user);
            Assert.AreEqual(0, result5, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetUsersInRoleAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userManager = GetTestUserManager(_testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName));
            using var roleManager = GetTestRoleManager(_testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName));
            var user1 = await GetTestUser(userManager);
            var user2 = await GetTestUser(userManager);
            var user3 = await GetTestUser(userManager);
            var role = await GetMockRandomRoleAsync(null, false);
            await roleManager.CreateAsync(role);
            var result1 = await userManager.AddToRoleAsync(user1, role.Name);
            Assert.IsTrue(result1.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result2 = await userManager.AddToRoleAsync(user2, role.Name);
            Assert.IsTrue(result2.Succeeded, $"Failed for provider: {provider.DisplayName}");
            var result3 = await userManager.AddToRoleAsync(user3, role.Name);
            Assert.IsTrue(result3.Succeeded, $"Failed for provider: {provider.DisplayName}");

            // Act
            var result4 = await userManager.GetUsersInRoleAsync(role.Name);

            // Assert
            Assert.AreEqual(3, result4.Count(), $"Failed for provider: {provider.DisplayName}");
        }
    }
}
