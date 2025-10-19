using Cosmos.BlobService;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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

            // Lightweight configuration (all in-memory).
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(ArticleEditLogicTestBase).Assembly, optional: false)
                .AddInMemoryCollection()
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