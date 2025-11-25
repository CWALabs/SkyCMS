using Cosmos.DynamicConfig;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sky.Tests.TestHelpers
{
    /// <summary>
    /// Testable version of DynamicConfigurationProvider that uses in-memory database.
    /// </summary>
    internal class TestableConfigurationProvider : DynamicConfigurationProvider
    {
        private readonly DbContextOptions<DynamicConfigDbContext> _testOptions;

        public TestableConfigurationProvider(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IMemoryCache memoryCache,
            ILogger<DynamicConfigurationProvider> logger,
            DbContextOptions<DynamicConfigDbContext> testOptions)
            : base(configuration, httpContextAccessor, memoryCache, logger)
        {
            _testOptions = testOptions;
        }

        protected override DynamicConfigDbContext GetDbContext()
        {
            return new DynamicConfigDbContext(_testOptions);
        }
    }
}