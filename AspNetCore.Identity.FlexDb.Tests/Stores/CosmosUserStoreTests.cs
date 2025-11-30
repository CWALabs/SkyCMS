using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Identity.CosmosDb.Tests.Net9.Stores
{
    [TestClass()]
    public class CosmosUserStoreTests : CosmosIdentityTestsBase
    {
        private static string phoneNumber = "0000000000";

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
        /// Create an IdentityUser test
        /// </summary>
        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task CreateAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            
            // Create a bunch of users in rapid succession
            for (int i = 0; i < 35; i++)
            {
                var r = await GetMockRandomUserAsync(userStore);
            }

            // Arrange - setup the new user
            var user = new IdentityUser(TestUtilities.IDENUSER1EMAIL) { Email = TestUtilities.IDENUSER1EMAIL };
            user.NormalizedUserName = user.UserName.ToUpper();
            user.NormalizedEmail = user.Email.ToUpper();
            user.Id = Guid.NewGuid().ToString(); // Use unique ID per test run

            // Act - create the user
            var result = await userStore.CreateAsync(user);

            // Assert - User should have been created
            Assert.IsNotNull(result, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");

            var user2 = await userStore.FindByIdAsync(user.Id);

            Assert.IsNotNull(user2, $"Failed for provider: {provider.DisplayName}");
            Assert.AreEqual(user2.UserName, TestUtilities.IDENUSER1EMAIL, $"Failed for provider: {provider.DisplayName}");
            Assert.AreEqual(user2.Email, TestUtilities.IDENUSER1EMAIL, $"Failed for provider: {provider.DisplayName}");
            Assert.AreEqual(user2.NormalizedUserName, TestUtilities.IDENUSER1EMAIL.ToUpper(), $"Failed for provider: {provider.DisplayName}");
            Assert.AreEqual(user2.NormalizedEmail, TestUtilities.IDENUSER1EMAIL.ToUpper(), $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task DeleteAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange - setup the new user
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            using var dbContext = _testUtilities.GetDbContext(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var userId = user.Id;
            var role = await GetMockRandomRoleAsync(roleStore);
            var claim = GetMockClaim();
            var login = GetMockLoginInfoAsync();
            await userStore.AddClaimsAsync(user, new[] { claim });
            await userStore.AddLoginAsync(user, login);
            await userStore.AddToRoleAsync(user, role.NormalizedName);

            // Act
            var result = await userStore.DeleteAsync(user);

            // Assert
            Assert.IsTrue(result.Succeeded, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(await dbContext.Users.Where(a => a.Id == userId).CountAsync() == 0, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(await dbContext.UserClaims.Where(a => a.UserId == userId).CountAsync() == 0, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(await dbContext.UserLogins.Where(a => a.UserId == userId).CountAsync() == 0, $"Failed for provider: {provider.DisplayName}");
            Assert.IsTrue(await dbContext.UserRoles.Where(a => a.UserId == userId).CountAsync() == 0, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task FindByEmailAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var user1 = await userStore.FindByEmailAsync(user.Email.ToUpper());

            // Assert
            Assert.IsNotNull(user1, $"Failed for provider: {provider.DisplayName}");
            Assert.AreEqual(user.Email, user1.Email, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task FindByIdAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var user1 = await userStore.FindByIdAsync(user.Id);

            // Assert
            Assert.IsNotNull(user1, $"Failed for provider: {provider.DisplayName}");
            Assert.AreEqual(user.Id, user1.Id, $"Failed for provider: {provider.DisplayName}");
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task FindByNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var user1 = await userStore.FindByNameAsync(user.UserName.ToUpper());

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(user.UserName, user1.UserName);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task FindByNameEmailAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var user1 = await userStore.FindByNameAsync(user.Email.ToUpper());

            // Assert
            Assert.IsNotNull(user);
            Assert.AreEqual(user.UserName, user1.UserName);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetEmailAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var result = await userStore.GetEmailAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Email, result);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetEmailConfirmedAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var result = await userStore.GetEmailConfirmedAsync(user);
            Assert.IsNotNull(result);
            Assert.IsFalse(result);

            // Arrange - user name and email are the same with this test
            await userStore.SetEmailConfirmedAsync(user, true);

            // Act
            result = await userStore.GetEmailConfirmedAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetEmailConfirmedAsyncTestFail(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var result = await userStore.GetEmailConfirmedAsync(user);
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
            await userStore.SetEmailConfirmedAsync(user, true);

            // Act
            result = await userStore.GetEmailConfirmedAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetNormalizedEmailAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var result = await userStore.GetNormalizedEmailAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.NormalizedEmail, result);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetNormalizedUserNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var result = await userStore.GetNormalizedUserNameAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.NormalizedUserName, result);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetPasswordHashAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var hash = await userStore.GetPasswordHashAsync(user); // Should be no hash now
            Assert.IsTrue(string.IsNullOrEmpty(hash));
            var password = Guid.NewGuid().ToString(); // Now add hash
            await userStore.SetPasswordHashAsync(user, password);

            // Act
            hash = await userStore.GetPasswordHashAsync(user);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(hash));
            Assert.AreSame(password, hash); // The hash should be different than original
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetPhoneNumberAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var phoneNumber = "1234567899";
            await userStore.SetPhoneNumberAsync(user, phoneNumber);
            //user = await userStore.FindByIdAsync(user.Id);

            // Act
            user = await userStore.FindByIdAsync(user.Id);
            var result2 = await userStore.GetPhoneNumberAsync(user);

            // Assert
            Assert.AreSame(phoneNumber, result2);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetPhoneNumberConfirmedAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            await userStore.SetPhoneNumberAsync(user, phoneNumber);
            //user = await userStore.FindByIdAsync(user.Id);
            await userStore.SetPhoneNumberConfirmedAsync(user, true);
            //user = await userStore.FindByIdAsync(user.Id);

            // Act
            var result = await userStore.GetPhoneNumberConfirmedAsync(user);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetUserIdAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var result = await userStore.GetUserIdAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Id, result);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetUserNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var result = await userStore.GetUserNameAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.UserName, result);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task HasPasswordAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var hash = await userStore.GetPasswordHashAsync(user); // Should be no hash now
            Assert.IsTrue(string.IsNullOrEmpty(hash));
            var password = Guid.NewGuid().ToString(); // Now add hash

            await userStore.SetPasswordHashAsync(user, password);

            // Act
            var result1 = await userStore.HasPasswordAsync(user);

            // Assert
            Assert.IsTrue(result1);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetEmailAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            await userStore.SetEmailAsync(user, TestUtilities.IDENUSER2EMAIL);

            // Assert
            var user2 = await userStore.FindByIdAsync(user.Id);

            Assert.IsNotNull(user2);
            Assert.AreEqual(TestUtilities.IDENUSER2EMAIL, user2.Email);

            Assert.AreEqual(user.UserName, user2.UserName);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetEmailConfirmedAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            Assert.IsFalse(user.EmailConfirmed);

            // Act
            await userStore.SetEmailConfirmedAsync(user, true);

            // Assert
            var result = await userStore.GetEmailConfirmedAsync(user);
            user = await userStore.FindByIdAsync(user.Id);
            Assert.IsTrue(user.EmailConfirmed);
            Assert.IsTrue(result);
        }

        // This function is tested with SetEmailAsync().
        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetNormalizedEmailAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var newEmail = $"A{GetNextRandomNumber(111, 9999).ToString()}@foo.com";

            // Act
            await userStore.SetNormalizedEmailAsync(user, newEmail.ToUpper());

            // Assert
            user = await userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(newEmail.ToUpper(), user.NormalizedEmail);
        }

        // This method is tested with SetUserNameAsync().
        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetNormalizedUserNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var newEmail = $"A{GetNextRandomNumber(111, 9999).ToString()}@foo.com";

            // Act
            await userStore.SetNormalizedUserNameAsync(user, newEmail.ToUpper());

            // Assert
            var user2 = await userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(newEmail.ToUpper(), user2.NormalizedUserName);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetPasswordHashAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            Assert.IsTrue(string.IsNullOrEmpty(user.PasswordHash));

            // Act
            await userStore.SetPasswordHashAsync(user, Guid.NewGuid().ToString());

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(user.PasswordHash));


        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetPhoneNumberAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            Assert.IsTrue(string.IsNullOrEmpty(user.PhoneNumber));

            // Act
            await userStore.SetPhoneNumberAsync(user, phoneNumber);

            // Assert
            var user2 = await userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(phoneNumber, user2.PhoneNumber);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetPhoneNumberConfirmedAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            Assert.IsFalse(user.PhoneNumberConfirmed);

            // Act
            await userStore.SetPhoneNumberConfirmedAsync(user, true);

            // Assert
            var result = await userStore.GetPhoneNumberConfirmedAsync(user);
            user = await userStore.FindByIdAsync(user.Id);
            Assert.IsTrue(user.PhoneNumberConfirmed);
            Assert.IsTrue(result);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetUserNameAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var newUserName = "A" + user.UserName;

            // Act
            await userStore.SetUserNameAsync(user, newUserName);

            // Assert
            user = await userStore.FindByIdAsync(user.Id);
            Assert.AreEqual(newUserName, user.UserName);

        }

        // This method tested with SetPasswordHashAsyncTest() | UserManager.AddPasswordAsync()
        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task UpdateAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var phoneNumber = "1234567890";

            // Act
            user.Email = TestUtilities.IDENUSER1EMAIL;
            user.NormalizedEmail = TestUtilities.IDENUSER1EMAIL.ToUpper();
            user.PhoneNumber = phoneNumber;

            var result = await userStore.UpdateAsync(user);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Succeeded);

            var user1 = await userStore.FindByIdAsync(user.Id);

            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL, user1.Email);
            Assert.AreEqual(TestUtilities.IDENUSER1EMAIL.ToUpper(), user1.NormalizedEmail);
            Assert.AreEqual(phoneNumber, user1.PhoneNumber);

        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task AddLoginAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var loginInfo = GetMockLoginInfoAsync();
            await userStore.AddLoginAsync(user, loginInfo);

            // Assert
            var logins = await userStore.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));

        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task RemoveLoginAsyncTest(TestDatabaseProvider provider)
        {

            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var loginInfo = GetMockLoginInfoAsync();
            await userStore.AddLoginAsync(user, loginInfo);
            var logins = await userStore.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));

            // Act
            await userStore.RemoveLoginAsync(user, "Twitter", loginInfo.ProviderKey);

            // Assert
            logins = await userStore.GetLoginsAsync(user);
            Assert.AreEqual(0, logins.Count);

        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetLoginsAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var loginInfo = GetMockLoginInfoAsync();
            await userStore.AddLoginAsync(user, loginInfo);

            // Act
            var logins = await userStore.GetLoginsAsync(user);

            // Assert
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task FindByLoginAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var loginInfo = GetMockLoginInfoAsync();
            await userStore.AddLoginAsync(user, loginInfo);
            var logins = await userStore.GetLoginsAsync(user);
            Assert.AreEqual(1, logins.Count);
            Assert.IsTrue(logins.Any(a => a.LoginProvider.Equals("Twitter")));

            // Arrange
            var user2 = await userStore.FindByLoginAsync("Twitter", loginInfo.ProviderKey);

            // Assert
            Assert.AreEqual(user.Id, user2.Id);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task AddToRoleAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var role = await GetMockRandomRoleAsync(roleStore);
            var users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users

            // Act
            await userStore.AddToRoleAsync(user, role.Name);

            // Assert
            Assert.IsTrue(await userStore.IsInRoleAsync(user, role.Name));
            users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(1, users.Count); // Should be one user
            Assert.IsTrue(users.Any(u => u.Id == user.Id));
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task RemoveFromRoleAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var role = await GetMockRandomRoleAsync(roleStore);
            var users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users
            await userStore.AddToRoleAsync(user, role.Name);
            Assert.IsTrue(await userStore.IsInRoleAsync(user, role.Name));
            users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(1, users.Count); // Should be one user
            Assert.IsTrue(users.Any(u => u.Id == user.Id));

            // Act
            await userStore.RemoveFromRoleAsync(user, role.Name);

            // Assert
            users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users

        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetRolesAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var role1 = await GetMockRandomRoleAsync(roleStore);
            var role2 = await GetMockRandomRoleAsync(roleStore);
            var users1 = await userStore.GetUsersInRoleAsync(role1.Name);
            Assert.AreEqual(0, users1.Count); // Should be no users
            var users2 = await userStore.GetUsersInRoleAsync(role1.Name);
            Assert.AreEqual(0, users2.Count); // Should be no users

            await userStore.AddToRoleAsync(user, role1.Name);
            await userStore.AddToRoleAsync(user, role2.Name);

            Assert.IsTrue(await userStore.IsInRoleAsync(user, role1.Name));
            Assert.IsTrue(await userStore.IsInRoleAsync(user, role2.Name));

            // Act
            var roles = await userStore.GetRolesAsync(user);

            // Assert
            Assert.AreEqual(2, roles.Count); // Should be two
            Assert.IsTrue(roles.Contains(role1.Name));
            Assert.IsTrue(roles.Contains(role2.Name));

        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task IsInRoleAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);
            var role = await GetMockRandomRoleAsync(roleStore);
            var users = await userStore.GetUsersInRoleAsync(role.Name);
            Assert.AreEqual(0, users.Count); // Should be no users
            await userStore.AddToRoleAsync(user, role.Name);

            // Act
            var result = await userStore.IsInRoleAsync(user, role.Name);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod()]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task GetUsersInRoleAsyncTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);
            
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            using var roleStore = _testUtilities.GetRoleStore(provider.ConnectionString, provider.DatabaseName);
            var user1 = await GetMockRandomUserAsync(userStore);
            var user2 = await GetMockRandomUserAsync(userStore);
            var role = await GetMockRandomRoleAsync(roleStore);
            await userStore.AddToRoleAsync(user1, role.Name);
            await userStore.AddToRoleAsync(user2, role.Name);

            // Act
            var result = await userStore.GetUsersInRoleAsync(role.Name);

            // Assert
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result.Any(r => r.Id == user1.Id));
            Assert.IsTrue(result.Any(r => r.Id == user2.Id));

        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task QueryUsersTest(TestDatabaseProvider provider)
        {
            InitializeForProvider(provider);

            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user1 = await GetMockRandomUserAsync(userStore);

            // Act
            var result = await userStore.Users.ToListAsync();

            // Assert
            Assert.IsInstanceOfType(userStore.Users, typeof(IQueryable<IdentityUser>));
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        [DynamicData(nameof(GetTestProviders), DynamicDataSourceType.Method)]
        public async Task SetAndGetAuthenticatorKeyAsyncTest(TestDatabaseProvider provider)
        {
            // Arrange
            using var userStore = _testUtilities.GetUserStore(provider.ConnectionString, provider.DatabaseName);
            var user = await GetMockRandomUserAsync(userStore);

            // Act
            var loginInfo = GetMockLoginInfoAsync();
            await userStore.AddLoginAsync(user, loginInfo);
            await userStore.SetAuthenticatorKeyAsync(user, "AuthenticatorKey", default);
            var code = await userStore.GetAuthenticatorKeyAsync(user, default);

            // Assert
            Assert.IsNotNull(code);
        }
    }
}