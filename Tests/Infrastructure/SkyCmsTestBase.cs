using Cosmos.BlobService;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Common.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using RazorLight;
using Sky.Cms.Services;
using Sky.Editor.Data.Logic;
using Sky.Editor.Domain.Events;
using Sky.Editor.Infrastructure.Time;
using Sky.Editor.Services.Authors;
using Sky.Editor.Services.BlogPublishing;
using Sky.Editor.Services.BlogRenderingService;
using Sky.Editor.Services.Catalog;
using Sky.Editor.Services.Html;
using Sky.Editor.Services.Publishing;
using Sky.Editor.Services.Redirects;
using Sky.Editor.Services.ReservedPaths;
using Sky.Editor.Services.Slugs;
using Sky.Editor.Services.Templates;
using Sky.Editor.Services.Titles;
using System.Reflection;

namespace Sky.Tests
{
    /// <summary>
    /// Base fixture for tests targeting <see cref="ArticleEditLogic"/>.
    /// Sets up an isolated in‑memory EF Core context and supporting services.
    /// Provides a capture dispatcher to assert domain event publishing.
    /// </summary>
    public abstract class SkyCmsTestBase : IAsyncDisposable
    {
        protected AuthorInfoService AuthorInfoService = null!;
        protected ApplicationDbContext Db = null!;
        protected ArticleEditLogic Logic = null!;
        protected CatalogService CatalogService = null!;
        protected StorageContext Storage = null!;
        protected IMemoryCache Cache = null!;
        protected Guid TestUserId;
        protected ISlugService SlugService = null!;
        protected EditorSettings EditorSettings = null!;
        protected IHttpContextAccessor HttpContextAccessor = null!;
        protected TestDomainEventDispatcher EventDispatcher = null!;
        protected IPublishingService PublishingService = null!;
        protected IArticleHtmlService ArticleHtmlService = null!;
        protected IReservedPaths ReservedPaths = null!;
        protected IRedirectService RedirectService = null!;
        protected ITitleChangeService TitleChangeService = null!;
        protected IClock Clock { get; } = new SystemClock();
        protected UserManager<IdentityUser> UserManager = null!;
        protected ITemplateService TemplateService = null!;
        protected IBlogRenderingService BlogRenderingService = null!;
        protected IViewRenderService ViewRenderService = null!;

        private async Task EnsureBlogStreamTemplateExistsAsync()
        {
            var existingTemplate = await Db.Templates
                .FirstOrDefaultAsync(t => t.PageType == "blog-stream");
            if (existingTemplate == null)
            {
                var t = TemplateService.GetTemplateByKeyAsync("blog-stream").Result;
                var template = new Template
                {
                    Id = Guid.NewGuid(),
                    PageType = "blog-stream",
                    Content = t.Content
                };
                Db.Templates.Add(template);
                await Db.SaveChangesAsync();
            }
        }

        private async Task EnsureBlogPostTemplateExistsAsync()
        {
            var existingTemplate = await Db.Templates
                .FirstOrDefaultAsync(t => t.PageType == "blog-post");

            if (existingTemplate == null)
            {
                var t = TemplateService.GetTemplateByKeyAsync("blog-post").Result;
                var template = new Template
                {
                    Id = Guid.NewGuid(),
                    PageType = "blog-post",
                    Content = t.Content
                };
                Db.Templates.Add(template);
                await Db.SaveChangesAsync();
            }
        }

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
                .AddUserSecrets(typeof(SkyCmsTestBase).Assembly, optional: false)
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
            SlugService = new SlugService();
            ArticleHtmlService = new ArticleHtmlService();
            var catalogLogger = new LoggerFactory().CreateLogger<CatalogService>();
            CatalogService = new CatalogService(Db, ArticleHtmlService, Clock, catalogLogger);
            EventDispatcher = new TestDomainEventDispatcher();
            var authorInfoService = new AuthorInfoService(Db, Cache);
            BlogRenderingService = new BlogRenderingService(Db);
            ReservedPaths = new ReservedPaths(Db);
            AuthorInfoService = new AuthorInfoService(Db, Cache);
            
            var mockViewRenderService = new Mock<IViewRenderService>();
            mockViewRenderService
                .Setup(x => x.RenderToStringAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync("<html>Mocked Rendered View</html>");

            ViewRenderService = mockViewRenderService.Object;

            PublishingService = new PublishingService(Db, Storage, EditorSettings,
                new LoggerFactory().CreateLogger<PublishingService>(),
                HttpContextAccessor, authorInfoService,
                Clock,
                BlogRenderingService,
                ViewRenderService);
            
            RedirectService = new RedirectService(Db, SlugService, Clock, PublishingService);
            TitleChangeService = new TitleChangeService(Db, SlugService, RedirectService, Clock, EventDispatcher, PublishingService, ReservedPaths, BlogRenderingService, new LoggerFactory().CreateLogger<TitleChangeService>());
            var webHostEnvironmentMock = new Mock<IWebHostEnvironment>();
            var assem = Assembly.GetAssembly(typeof(TemplateService));
            var path = Path.GetDirectoryName(assem!.Location)!;
            webHostEnvironmentMock.Setup(m => m.ContentRootPath).Returns(path);
            var webHostEnvironment = webHostEnvironmentMock.Object;

            TemplateService = new TemplateService(webHostEnvironment, new LoggerFactory().CreateLogger<TemplateService>(), Db);
            TemplateService.EnsureDefaultTemplatesExistAsync().Wait();


            var publishingArtifactService = new PublishingService(
                Db,
                Storage,
                EditorSettings,
                new NullLogger<PublishingService>(),
                HttpContextAccessor,
                authorInfoService, Clock, BlogRenderingService, ViewRenderService
            );
            var redirectService = new RedirectService(Db, SlugService, Clock, publishingArtifactService);

            var titleChangeService = new TitleChangeService(
                Db, SlugService, redirectService, Clock, EventDispatcher, publishingArtifactService, ReservedPaths, BlogRenderingService, new NullLogger<TitleChangeService>());

            // Construct logic (using explicit DI constructor).
            Logic = new ArticleEditLogic(
                Db,
                cfg,
                Cache,
                Storage,
                new NullLogger<ArticleEditLogic>(),
                HttpContextAccessor,
                EditorSettings,
                Clock,
                SlugService,
                ArticleHtmlService,
                CatalogService,
                PublishingService,
                titleChangeService,
                redirectService,
                TemplateService);

            // User manager setup.
            var userStore = new UserStore<IdentityUser>(Db);
            UserManager = new UserManager<IdentityUser>(
                    userStore,
                    Options.Create(new IdentityOptions()),
                    new PasswordHasher<IdentityUser>(),
                    Array.Empty<IUserValidator<IdentityUser>>(),
                    Array.Empty<IPasswordValidator<IdentityUser>>(),
                    new UpperInvariantLookupNormalizer(),
                    new IdentityErrorDescriber(),
                    null!,
                    new NullLogger<UserManager<IdentityUser>>());

            EnsureBlogStreamTemplateExistsAsync().Wait();
            EnsureBlogPostTemplateExistsAsync().Wait();

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