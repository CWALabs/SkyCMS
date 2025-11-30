using AspNetCore.Identity.FlexDb;
using AspNetCore.Identity.FlexDb.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sky.Tests.Identity
{
    /// <summary>
    /// Base class for Identity tests supporting multiple database providers via CosmosDbOptionsBuilder.
    /// </summary>
    public abstract class CosmosIdentityTestsBase
    {
        public static TestUtilities _testUtilities;
        public static Random _random;
        public static IConfiguration _configuration;
        
        // CRITICAL: Keep the SQLite connection open for in-memory databases
        private static SqliteConnection _sqliteConnection;
        
        // Store the current provider name to determine if we need special handling
        private static string _currentProviderName;
        private static string _currentConnectionString;

        /// <summary>
        /// Database provider test data for data-driven tests.
        /// </summary>
        public static IEnumerable<object[]> DatabaseProviders
        {
            get
            {
                var config = GetConfiguration();
                var useCloudDatabases = config.GetValue<bool>("TestSettings:UseCloudDatabases", false);

                // Always test SQLite (in-memory, no external dependencies)
                yield return new object[] { "SQLite", config.GetConnectionString("SQLite"), "IdentityTest" };

                // Only test cloud databases if explicitly enabled
                if (useCloudDatabases)
                {
                    yield return new object[] { "CosmosDB", config.GetConnectionString("CosmosDB"), "IdentityTest" };
                    yield return new object[] { "SqlServer", config.GetConnectionString("SqlServer"), "IdentityTest" };
                    yield return new object[] { "MySQL", config.GetConnectionString("MySQL"), "skycmstest" };
                }
            }
        }

        /// <summary>
        /// Gets configuration from user secrets and environment variables.
        /// </summary>
        public static IConfiguration GetConfiguration()
        {
            if (_configuration == null)
            {
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddUserSecrets<CosmosIdentityTestsBase>(optional: true)
                    .AddEnvironmentVariables()
                    .Build();
            }
            return _configuration;
        }

        /// <summary>
        /// Initialize test context for a specific database provider.
        /// </summary>
        /// <param name="providerName">Name of the database provider (CosmosDB, SqlServer, MySQL, SQLite)</param>
        /// <param name="connectionString">Connection string for the provider</param>
        /// <param name="databaseName">Database name (used for some providers)</param>
        /// <param name="backwardCompatibility">Enable backward compatibility mode</param>
        public static void InitializeClass(string providerName, string connectionString, string databaseName, bool backwardCompatibility = false)
        {
            _testUtilities = new TestUtilities();
            _random = new Random();
            
            // Store current provider and connection info
            _currentProviderName = providerName;
            _currentConnectionString = connectionString;

            // For SQLite in-memory, we need to keep a connection open
            if (providerName == "SQLite" && connectionString.Contains(":memory:"))
            {
                try
                {
                    // Create and open a persistent connection
                    _sqliteConnection = new SqliteConnection(connectionString);
                    _sqliteConnection.Open();
                    
                    // Create the database schema using a temporary context
                    var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
                    optionsBuilder.UseSqlite(_sqliteConnection); // Use the open connection
                    
                    using var tempContext = new IdentityDbContext(optionsBuilder.Options);
                    tempContext.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing {providerName}: {ex.Message}");
                    throw;
                }
            }
            else
            {
                // For non-SQLite databases, use the original pattern
                using var dbContext = GetDbContext(connectionString, databaseName, backwardCompatibility);
                
                try
                {
                    // Ensure database is created
                    var task = dbContext.Database.EnsureCreatedAsync();
                    task.Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing {providerName}: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Gets a DbContext configured using CosmosDbOptionsBuilder.
        /// For SQLite in-memory databases, uses the shared connection.
        /// </summary>
        public static IdentityDbContext GetDbContext(string connectionString, string databaseName, bool backwardCompatibility = false)
        {
            var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
            
            // For SQLite in-memory, use the shared connection
            if (_sqliteConnection != null && 
                _currentProviderName == "SQLite" && 
                connectionString == _currentConnectionString &&
                connectionString.Contains(":memory:"))
            {
                optionsBuilder.UseSqlite(_sqliteConnection);
                return new IdentityDbContext(optionsBuilder.Options);
            }
            
            // For other providers, use the standard configuration
            CosmosDbOptionsBuilder.ConfigureDbOptions(optionsBuilder, connectionString);
            return new IdentityDbContext(optionsBuilder.Options);
        }

        /// <summary>
        /// Cleanup test database after tests complete.
        /// </summary>
        public static async Task CleanupDatabase(string providerName, string connectionString, string databaseName)
        {
            var config = GetConfiguration();
            var cleanupAfterTests = config.GetValue<bool>("TestSettings:CleanupAfterTests", true);

            if (!cleanupAfterTests)
            {
                return;
            }

            try
            {
                // For in-memory SQLite, close and dispose the connection
                if (providerName == "SQLite" && connectionString.Contains(":memory:"))
                {
                    if (_sqliteConnection != null)
                    {
                        await _sqliteConnection.CloseAsync();
                        await _sqliteConnection.DisposeAsync();
                        _sqliteConnection = null;
                    }
                    _currentProviderName = null;
                    _currentConnectionString = null;
                    return;
                }

                // For cloud databases, optionally delete test data
                using var dbContext = GetDbContext(connectionString, databaseName);
                await dbContext.Database.EnsureDeletedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up {providerName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a random number for test data generation.
        /// </summary>
        public int GetNextRandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityRole"/> for unit testing purposes.
        /// </summary>
        public async Task<IdentityRole> GetMockRandomRoleAsync(
            CosmosRoleStore<IdentityUser, IdentityRole, string> roleStore, bool saveToDatabase = true)
        {
            var role = new IdentityRole(GetNextRandomNumber(1, 9999).ToString());
            role.NormalizedName = role.Name.ToUpper();

            if (roleStore != null && saveToDatabase)
            {
                var result = await roleStore.CreateAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => $"[{e.Code}] {e.Description}"));
                    Assert.Fail($"Failed to create role '{role.Name}'. Errors: {errors}");
                }
                
                role = await roleStore.FindByIdAsync(role.Id);
                if (role == null)
                {
                    Assert.Fail($"Role with ID '{role.Id}' was created but could not be retrieved from database.");
                }
            }

            return role;
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityUser"/> for unit testing purposes.
        /// </summary>
        public async Task<IdentityUser> GetMockRandomUserAsync(
            CosmosUserStore<IdentityUser, IdentityRole, string> userStore, bool saveToDatabase = true)
        {
            var randomEmail = $"{GetNextRandomNumber(1000, 9999)}@{GetNextRandomNumber(10000, 99999)}.com";
            var user = new IdentityUser(randomEmail) { Email = randomEmail, Id = Guid.NewGuid().ToString() };

            user.NormalizedUserName = user.UserName.ToUpper();
            user.NormalizedEmail = user.Email.ToUpper();

            if (userStore != null && saveToDatabase)
            {
                var result = await userStore.CreateAsync(user);
                if (!result.Succeeded)
                {
                    // Provide detailed error information for debugging
                    var errors = string.Join(", ", result.Errors.Select(e => $"[{e.Code}] {e.Description}"));
                    Assert.Fail($"Failed to create user '{user.UserName}'. Errors: {errors}");
                }
                
                user = await userStore.FindByNameAsync(user.UserName.ToUpper());
                if (user == null)
                {
                    Assert.Fail($"User '{user.NormalizedUserName}' was created but could not be retrieved from database.");
                }
            }

            return user;
        }

        /// <summary>
        /// Gets a mock login info for testing purposes.
        /// </summary>
        public UserLoginInfo GetMockLoginInfoAsync()
        {
            return new UserLoginInfo("Twitter", Guid.NewGuid().ToString(), "Twitter");
        }

        /// <summary>
        /// Gets a mock claim for testing purposes.
        /// </summary>
        public Claim GetMockClaim(string seed = "")
        {
            return new Claim(Guid.NewGuid().ToString(), $"{Guid.NewGuid()}{seed}");
        }

        /// <summary>
        /// Gets a user manager for testing purposes.
        /// </summary>
        public UserManager<TUser> GetTestUserManager<TUser>(IUserStore<TUser> store)
            where TUser : class
        {
            var builder = new IdentityBuilder(typeof(IdentityUser), new ServiceCollection());

            var userType = builder.UserType;

            store = store ?? new Mock<IUserStore<TUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();

            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<TUser>>();
            var validator = new Mock<IUserValidator<TUser>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<TUser>>();
            pwdValidators.Add(new PasswordValidator<TUser>());
            var userManager = new UserManager<TUser>(store, options.Object, new PasswordHasher<TUser>(),
                userValidators, pwdValidators, MockLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<TUser>>>().Object);
            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

            return userManager;
        }

        /// <summary>
        /// Gets a role manager for testing purposes.
        /// </summary>
        public RoleManager<TRole> GetTestRoleManager<TRole>(IRoleStore<TRole> store)
            where TRole : class
        {
            store = store ?? new Mock<IRoleStore<TRole>>().Object;
            var roles = new List<IRoleValidator<TRole>>();
            roles.Add(new RoleValidator<TRole>());
            var roleManager = new RoleManager<TRole>(store, roles, MockLookupNormalizer(),
                new IdentityErrorDescriber(), new Mock<ILogger<RoleManager<TRole>>>().Object);
            return roleManager;
        }

        /// <summary>
        /// Gets a mock lookup normalizer for testing.
        /// </summary>
        public ILookupNormalizer MockLookupNormalizer()
        {
            var normalizerFunc = new Func<string, string>(i =>
            {
                if (i == null)
                {
                    return null;
                }
                else
                {
                    return i.ToUpperInvariant();
                }
            });
            var lookupNormalizer = new Mock<ILookupNormalizer>();
            lookupNormalizer.Setup(i => i.NormalizeName(It.IsAny<string>())).Returns(normalizerFunc);
            lookupNormalizer.Setup(i => i.NormalizeEmail(It.IsAny<string>())).Returns(normalizerFunc);
            return lookupNormalizer.Object;
        }
    }
}