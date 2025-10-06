using Cosmos.BlobService;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Common.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Sky.Editor.Data.Logic;
using Sky.Editor.Domain.Events;
using Sky.Editor.Infrastructure.Time;
using Sky.Editor.Services.Catalog;
using Sky.Editor.Services.Html;
using Sky.Editor.Services.Publishing;
using Sky.Editor.Services.Redirects;
using Sky.Editor.Services.Slugs;
using Sky.Editor.Services.Titles;

namespace Sky.Tests
{
    /// <summary>
    /// Base fixture for tests targeting <see cref="ArticleEditLogic"/>.
    /// Sets up an isolated in‑memory EF Core context and supporting services.
    /// Provides a capture dispatcher to assert domain event publishing.
    /// </summary>
    public abstract class ArticleEditLogicTestBase : IAsyncDisposable
    {
        protected ApplicationDbContext Db = null!;
        protected ArticleEditLogic Logic = null!;
        protected StorageContext Storage = null!;
        protected IMemoryCache Cache = null!;
        protected Guid TestUserId;
        protected EditorSettings EditorSettings = null!;
        protected IHttpContextAccessor HttpContextAccessor = null!;
        protected TestDomainEventDispatcher EventDispatcher = null!;

        /// <summary>
        /// Initialize test context. Call from [TestInitialize].
        /// </summary>
        /// <param name="seedLayout">Seed default layout required by logic layer.</param>
        protected void InitializeTestContext(bool seedLayout = true)
        {
            TestUserId = Guid.NewGuid();

            // In-memory DB (unique per test run).
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            Db = new ApplicationDbContext(options);

            if (seedLayout)
            {
                Db.Layouts.Add(new Layout
                {
                    Id = Guid.NewGuid(),
                    LayoutName = "Default",
                    IsDefault = true,
                    Head = string.Empty,
                    HtmlHeader = string.Empty,
                    FooterHtmlContent = string.Empty
                });
                Db.SaveChanges();
            }

            Cache = new MemoryCache(new MemoryCacheOptions());
            var cfg = Options.Create(new CosmosConfig());

            // Lightweight configuration (all in-memory).
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(ArticleEditLogicTestBase).Assembly, optional: false)
                .AddInMemoryCollection()
                .Build();

            HttpContextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
            HttpContextAccessor.HttpContext!.Request.Host = new HostString("example.com");

            EditorSettings = new EditorSettings(configuration, Db, HttpContextAccessor, Cache);

            // Provide a safe fallback for storage if no connection string is configured.
            var storageConnectionString = configuration.GetConnectionString("StorageConnectionString")
                                         ?? "UseDevelopmentStorage=true;";
            Storage = new StorageContext(storageConnectionString, Cache);

            // Core service graph.
            var clock = new SystemClock();
            var slugService = new SlugService();
            var htmlService = new ArticleHtmlService();
            var catalogLogger = new LoggerFactory().CreateLogger<CatalogService>();
            var catalogService = new CatalogService(Db, htmlService, clock, catalogLogger);
            EventDispatcher = new TestDomainEventDispatcher();
            var redirectService = new RedirectService(Db, slugService, clock);
            var publishingService = new PublishingService(Db, clock, EventDispatcher);
            var titleChangeService = new TitleChangeService(Db, slugService, redirectService, clock, EventDispatcher);

            // Construct logic (using explicit DI constructor).
            Logic = new ArticleEditLogic(
                Db,
                cfg,
                Cache,
                Storage,
                new NullLogger<ArticleEditLogic>(),
                HttpContextAccessor,
                EditorSettings,
                clock,
                slugService,
                htmlService,
                catalogService,
                publishingService,
                titleChangeService,
                redirectService);
            
            AfterInitialize();
        }

        /// <summary>
        /// Override for additional seeding in derived test classes.
        /// </summary>
        protected virtual void AfterInitialize() { }

        protected Task<int> ArticleCountAsync() => Db.Articles.CountAsync();

        public virtual async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            Cache.Dispose();
        }

        /// <summary>
        /// Captures domain events for assertions in tests.
        /// </summary>
        protected sealed class TestDomainEventDispatcher : IDomainEventDispatcher
        {
            private readonly List<IDomainEvent> events = new();

            public IReadOnlyList<IDomainEvent> Events => events;

            public Task DispatchAsync(IEnumerable<IDomainEvent> events)
            {
                if (events != null) this.events.AddRange(events);
                return Task.CompletedTask;
            }

            public Task DispatchAsync(IDomainEvent @event)
            {
                if (@event != null) events.Add(@event);
                return Task.CompletedTask;
            }

            // Cancellation-capable overloads required by interface.
            public Task DispatchAsync(IDomainEvent @event, CancellationToken cancellationToken)
            {
                if (@event == null) return Task.CompletedTask;
                cancellationToken.ThrowIfCancellationRequested();
                events.Add(@event);
                return Task.CompletedTask;
            }

            public Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken)
            {
                if (domainEvents != null)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    this.events.AddRange(domainEvents);
                }
                return Task.CompletedTask;
            }

            public T? Last<T>() where T : class, IDomainEvent =>
                events.LastOrDefault(e => e is T) as T;

            public void Clear() => events.Clear();
        }
    }
}