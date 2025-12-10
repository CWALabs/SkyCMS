using AspNetCore.Identity.FlexDb.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;

namespace AspNetCore.Identity.CosmosDb.Tests.Net9
{
    public abstract class CosmosIdentityTestsBase
    {
        protected static TestUtilities _testUtilities;
        protected static Random _random;
        private static readonly Dictionary<string, bool> _initializedProviders = new Dictionary<string, bool>();
        private static readonly object _initLock = new object();

        protected static void InitializeClass(string connectionString, string databaseName, bool backwardCompatibility = false)
        {
            //
            // Setup context.
            //
            _testUtilities = new TestUtilities();
            _random = new Random();

            // Create a unique key for this provider configuration
            var providerKey = $"{connectionString}_{databaseName}";

            lock (_initLock)
            {
                // Only initialize once per provider configuration
                if (_initializedProviders.ContainsKey(providerKey))
                {
                    return;
                }

                try
                {
                    // Different strategies for different providers
                    // Check provider type first before creating context
                    var isCosmosDb = connectionString.Contains("AccountEndpoint=", StringComparison.OrdinalIgnoreCase);
                    var isSqlite = connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) 
                                   && !connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase);
                    var isSqlServer = connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) 
                                      && (connectionString.Contains("User ID=", StringComparison.OrdinalIgnoreCase) 
                                          || connectionString.Contains("Integrated Security=", StringComparison.OrdinalIgnoreCase));
                    var isMySql = connectionString.Contains("uid=", StringComparison.OrdinalIgnoreCase) 
                                  || connectionString.Contains("user id=", StringComparison.OrdinalIgnoreCase);

                    if (isCosmosDb)
                    {
                        // Cosmos DB - ensure created
                        using var dbContext = _testUtilities.GetDbContext(connectionString, databaseName, backwardCompatibility: backwardCompatibility);
                        var createTask = dbContext.Database.EnsureCreatedAsync();
                        createTask.Wait();

                        if (createTask.IsFaulted)
                        {
                            throw new InvalidOperationException(
                                $"Cosmos DB creation failed: {createTask.Exception?.GetBaseException().Message}",
                                createTask.Exception);
                        }
                        
                        // Cosmos DB doesn't have traditional tables, skip verification
                    }
                    else if (isSqlite)
                    {
                        // SQLite - always recreate for clean state
                        // Use a fresh context for each operation to avoid disposal issues
                        
                        // Step 1: Delete existing database file directly (handles both encrypted and unencrypted)
                        // Extract the Data Source path from connection string
                        var dataSourceMatch = System.Text.RegularExpressions.Regex.Match(
                            connectionString, 
                            @"Data Source\s*=\s*([^;]+)", 
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        
                        if (dataSourceMatch.Success)
                        {
                            var dbPath = dataSourceMatch.Groups[1].Value.Trim();
                            
                            // Skip file deletion for in-memory databases
                            if (!dbPath.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
                            {
                                // Try to delete the file directly first
                                if (System.IO.File.Exists(dbPath))
                                {
                                    try
                                    {
                                        System.IO.File.Delete(dbPath);
                    
                                        // Also delete any associated files (-wal, -shm, etc.)
                                        var directory = System.IO.Path.GetDirectoryName(dbPath);
                                        var fileName = System.IO.Path.GetFileName(dbPath);
                    
                                        if (!string.IsNullOrEmpty(directory))
                                        {
                                            var associatedFiles = System.IO.Directory.GetFiles(directory, $"{fileName}*");
                                            foreach (var file in associatedFiles)
                                            {
                                                if (file != dbPath) // Already deleted
                                                {
                                                    try { System.IO.File.Delete(file); } catch { /* Ignore */ }
                                                }
                                            }
                                        }
                                    }
                                    catch (System.IO.IOException)
                                    {
                                        // If file is locked, try EnsureDeleted instead
                                        using (var dbContext = _testUtilities.GetDbContext(connectionString, databaseName, backwardCompatibility: backwardCompatibility))
                                        {
                                            dbContext.Database.CloseConnection();
                                            var deleted = dbContext.Database.EnsureDeleted();
                                        }
                                    }
                                }
                            }
                        }
                        
                        // Small delay to ensure file system has released the file
                        System.Threading.Thread.Sleep(100);
                        
                        // Step 2: Create fresh database with a new context
                        using (var dbContext = _testUtilities.GetDbContext(connectionString, databaseName, backwardCompatibility: backwardCompatibility))
                        {
                            var created = dbContext.Database.EnsureCreatedAsync().Result;
                            // Give SQLite a moment to finalize the schema
                            System.Threading.Thread.Sleep(100);

                            if (!created)
                            {
                                throw new InvalidOperationException("SQLite database creation returned false");
                            }

                            VerifyDatabaseSchema(dbContext);
                        }

                        // Step 3: Verify tables with yet another fresh context
                        using (var dbContext = _testUtilities.GetDbContext(connectionString, databaseName, backwardCompatibility: backwardCompatibility))
                        {
                            VerifyTablesExist(dbContext, retryCount: 3);
                        }
                    }
                    else if (isSqlServer || isMySql)
                    {
                        // SQL Server / MySQL - use migrations if available, otherwise ensure created
                        using var dbContext = _testUtilities.GetDbContext(connectionString, databaseName, backwardCompatibility: backwardCompatibility);

                        var created = dbContext.Database.EnsureCreated();

                        // Verify tables exist
                        VerifyTablesExist(dbContext, retryCount: 2);
                    }
                    else
                    {
                        // Unknown provider - use default behavior
                        using var dbContext = _testUtilities.GetDbContext(connectionString, databaseName, backwardCompatibility: backwardCompatibility);
                        var createTask = dbContext.Database.EnsureCreatedAsync();
                        createTask.Wait();

                        if (createTask.IsFaulted)
                        {
                            throw new InvalidOperationException(
                                $"Database creation failed: {createTask.Exception?.GetBaseException().Message}",
                                createTask.Exception);
                        }
                    }

                    // Mark this provider as initialized
                    _initializedProviders[providerKey] = true;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Database initialization failed: {ex.Message}",
                        ex);
                }
            }
        }

        /// <summary>
        /// Verifies the database schema is properly configured
        /// </summary>
        private static void VerifyDatabaseSchema(DbContext dbContext)
        {
            try
            {
                // Get the model from the context
                var model = dbContext.Model;
                
                // Check if Identity entities are registered
                var userEntityType = model.FindEntityType(typeof(IdentityUser));
                var roleEntityType = model.FindEntityType(typeof(IdentityRole));
                
                if (userEntityType == null)
                {
                    throw new InvalidOperationException(
                        "IdentityUser entity is not registered in the model. " +
                        "This suggests the DbContext configuration is incorrect.");
                }
                
                if (roleEntityType == null)
                {
                    throw new InvalidOperationException(
                        "IdentityRole entity is not registered in the model. " +
                        "This suggests the DbContext configuration is incorrect.");
                }
                
                // Get table names to verify they're set
                var userTableName = userEntityType.GetTableName();
                var roleTableName = roleEntityType.GetTableName();
                
                if (string.IsNullOrEmpty(userTableName))
                {
                    throw new InvalidOperationException("IdentityUser table name is not configured");
                }
                
                if (string.IsNullOrEmpty(roleTableName))
                {
                    throw new InvalidOperationException("IdentityRole table name is not configured");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Database schema verification failed: {ex.Message}. " +
                    "This may indicate the DbContext is not properly inheriting from IdentityDbContext " +
                    "or OnModelCreating is not calling base.OnModelCreating() for relational databases.",
                    ex);
            }
        }

        /// <summary>
        /// Verifies that critical Identity tables exist in the database
        /// </summary>
        private static void VerifyTablesExist(DbContext dbContext, int retryCount = 1)
        {
            Exception lastException = null;
            
            for (int attempt = 0; attempt < retryCount; attempt++)
            {
                try
                {
                    var connection = dbContext.Database.GetDbConnection();
                    var wasOpen = connection.State == System.Data.ConnectionState.Open;
                    
                    if (!wasOpen)
                    {
                        connection.Open();
                    }

                    try
                    {
                        using var command = connection.CreateCommand();
                        
                        // Check for AspNetRoles table (critical for the failing tests)
                        if (dbContext.Database.IsSqlite())
                        {
                            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='AspNetRoles'";
                        }
                        else if (dbContext.Database.IsSqlServer())
                        {
                            command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetRoles'";
                        }
                        else if (dbContext.Database.IsMySql())
                        {
                            command.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'AspNetRoles' AND TABLE_SCHEMA = DATABASE()";
                        }
                        else
                        {
                            return; // Skip verification for other providers
                        }

                        var result = command.ExecuteScalar();
                        
                        if (result == null)
                        {
                            // List all tables for diagnostic purposes
                            string allTablesQuery;
                            if (dbContext.Database.IsSqlite())
                            {
                                allTablesQuery = "SELECT name FROM sqlite_master WHERE type='table'";
                            }
                            else if (dbContext.Database.IsSqlServer())
                            {
                                allTablesQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES";
                            }
                            else if (dbContext.Database.IsMySql())
                            {
                                allTablesQuery = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE()";
                            }
                            else
                            {
                                throw new InvalidOperationException("AspNetRoles table was not created during database initialization");
                            }
                            
                            command.CommandText = allTablesQuery;
                            var tables = new List<string>();
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    tables.Add(reader.GetString(0));
                                }
                            }
                            
                            var tableList = tables.Any() ? string.Join(", ", tables) : "No tables found";
                            throw new InvalidOperationException(
                                $"AspNetRoles table was not created during database initialization. " +
                                $"Tables that exist: {tableList}. " +
                                $"This indicates EnsureCreated() ran but didn't create Identity tables.");
                        }
                        
                        // Success - table exists
                        return;
                    }
                    finally
                    {
                        if (!wasOpen)
                        {
                            connection.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    // If this isn't the last attempt, wait and retry
                    if (attempt < retryCount - 1)
                    {
                        System.Threading.Thread.Sleep(200); // Wait 200ms before retry
                    }
                }
            }
            
            // If we get here, all retries failed
            throw lastException ?? new InvalidOperationException("Table verification failed after all retries");
        }

        /// <summary>
        /// Initialize for a specific database provider
        /// </summary>
        protected static void InitializeForProvider(TestDatabaseProvider provider, bool backwardCompatibility = false)
        {
            InitializeClass(provider.ConnectionString, provider.DatabaseName, backwardCompatibility);
        }

        /// <summary>
        /// Gets a random number
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        protected int GetNextRandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityRole"/> for unit testing purposes
        /// </summary>
        /// <returns></returns>
        protected async Task<IdentityRole> GetMockRandomRoleAsync(
            CosmosRoleStore<IdentityUser, IdentityRole, string> roleStore, bool saveToDatabase = true)
        {
            // Use full GUID to ensure absolute uniqueness across all test runs and providers
            // Format: TestRole_a1b2c3d4e5f6 (shorter, fully unique)
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 12); // 12 hex chars = 2^48 combinations
            var roleName = $"TestRole_{uniqueId}";
            
            var role = new IdentityRole(roleName);
            role.NormalizedName = role.Name.ToUpper();

            if (roleStore != null && saveToDatabase)
            {
                var result = await roleStore.CreateAsync(role);
                
                // If creation fails due to duplicate (extremely unlikely but possible), retry once with new GUID
                if (!result.Succeeded && result.Errors.Any(e => 
                    e.Code == "DuplicateRoleName" || 
                    e.Description.Contains("duplicate", StringComparison.OrdinalIgnoreCase)))
                {
                    // Retry with new GUID
                    uniqueId = Guid.NewGuid().ToString("N");
                    roleName = $"TestRole_{uniqueId}";
                    role = new IdentityRole(roleName);
                    role.NormalizedName = role.Name.ToUpper();
                    result = await roleStore.CreateAsync(role);
                }
                
                // Improved error reporting
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => $"[{e.Code}] {e.Description}"));
                    Assert.Fail($"Failed to create role '{roleName}': {errors}");
                }
                
                Assert.IsTrue(result.Succeeded, $"Failed to create role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                
                // Verify role was actually created and can be retrieved
                role = await roleStore.FindByIdAsync(role.Id);
                
                if (role == null)
                {
                    Assert.Fail($"Role was created successfully but could not be retrieved by ID.");
                }
            }

            return role;
        }

        /// <summary>
        /// Gets a mock <see cref="IdentityUser"/> for unit testing purposes
        /// </summary>
        /// <returns></returns>
        protected async Task<IdentityUser> GetMockRandomUserAsync(
            CosmosUserStore<IdentityUser, IdentityRole, string> userStore, bool saveToDatabase = true)
        {
            // Use GUID to ensure uniqueness across all test runs
            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var randomEmail = $"test{uniqueId}_{GetNextRandomNumber(1000, 9999)}@test{GetNextRandomNumber(10000, 99999)}.com";
            
            var user = new IdentityUser(randomEmail) 
            { 
                Email = randomEmail, 
                Id = Guid.NewGuid().ToString() 
            };

            user.NormalizedUserName = user.UserName.ToUpper();
            user.NormalizedEmail = user.Email.ToUpper();

            if (userStore != null && saveToDatabase)
            {
                var result = await userStore.CreateAsync(user);
                
                // Improved error reporting
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Assert.Fail($"Failed to create user: {errors}");
                }
                
                Assert.IsTrue(result.Succeeded, $"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                user = await userStore.FindByNameAsync(user.UserName.ToUpper());
            }

            return user;
        }

        /// <summary>
        /// Gets a mock login info for testing purposes
        /// </summary>
        /// <returns></returns>
        protected UserLoginInfo GetMockLoginInfoAsync()
        {
            return new UserLoginInfo("Twitter", Guid.NewGuid().ToString(), "Twitter");
        }

        protected Claim GetMockClaim(string seed = "")
        {
            return new Claim(Guid.NewGuid().ToString(), $"{Guid.NewGuid().ToString()}{seed}");
        }

        /// <summary>
        /// Gets a user manager for testing purposes
        /// </summary>
        /// <typeparam name="TUser"></typeparam>
        /// <param name="store"></param>
        /// <returns></returns>
        public UserManager<TUser> GetTestUserManager<TUser>(IUserStore<TUser> store)
            where TUser : class
        {
            var builder = new IdentityBuilder(typeof(IdentityUser), new ServiceCollection());

            var userType = builder.UserType;

            var dataProtectionProviderType = typeof(DataProtectorTokenProvider<>).MakeGenericType(userType);
            var phoneNumberProviderType = typeof(PhoneNumberTokenProvider<>).MakeGenericType(userType);
            var emailTokenProviderType = typeof(EmailTokenProvider<>).MakeGenericType(userType);
            var authenticatorProviderType = typeof(AuthenticatorTokenProvider<>).MakeGenericType(userType);
            //var authenticatorProviderType = typeof(UserTwoFactorTokenProvider<>).MakeGenericType(userType);


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

        [TestCleanup]
        public void TestCleanup()
        {
            // Clear the initialization cache to force database recreation on next test
            // This ensures test isolation
            lock (_initLock)
            {
                
                _initializedProviders.Clear();
            }
        }
    }
}