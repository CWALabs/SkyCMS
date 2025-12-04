using Cosmos.BlobService;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Sky.Tests
{
    // Runs once for the entire Sky.Tests assembly, before any [TestClass] executes.
    [TestClass]
    public class AssemblyHooks
    {
        [AssemblyInitialize]
        public static void GlobalInitialize(TestContext context)
        {
            // One-time setup for all tests in this assembly.
            // Example: configure environment, logging, seed static data, etc.
            // Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            // Validate environment variables - halt all tests if validation fails
            if (!EnvironmentValidator.ValidateEnvironmentVariables(context))
            {
                // Abort further initialization if environment is invalid
                return;
            }

            // Lightweight configuration (all in-memory).
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(SkyCmsTestBase).Assembly, optional: true)
                .AddInMemoryCollection()
                .AddEnvironmentVariables()
                .Build();

            // Provide a safe fallback for storage if no connection string is configured.
            var storageConnectionString = configuration.GetConnectionString("StorageConnectionString")
                                         ?? "UseDevelopmentStorage=true;";

            var cache = new MemoryCache(new MemoryCacheOptions());

            var storage = new StorageContext(storageConnectionString, cache);

            var task = storage.DeleteFolderAsync("/");
            task.Wait();
        }

        [AssemblyCleanup]
        public static void GlobalCleanup()
        {
            // One-time teardown after all tests in this assembly have run.
        }
    }
}