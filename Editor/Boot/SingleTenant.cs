// <copyright file="SingleTenant.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Boot
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.RateLimiting;
    using System.Threading.Tasks;
    using System.Web;
    using AspNetCore.Identity.FlexDb.Extensions;
    using Azure.Identity;
    using Cosmos.BlobService;
    using Cosmos.Cms.Common.Services.Configurations;
    using Cosmos.Common.Data;
    using Cosmos.Common.Services;
    using Cosmos.Common.Services.Configurations;
    using Cosmos.EmailServices;
    using Hangfire;
    using Hangfire.InMemory;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
    using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.RateLimiting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json.Serialization;
    using Sky.Cms.Hubs;
    using Sky.Cms.Services;
    using Sky.Editor.Data.Logic;
    using Sky.Editor.Domain.Events;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services;
    using Sky.Editor.Services.Catalog;
    using Sky.Editor.Services.Html;
    using Sky.Editor.Services.Publishing;
    using Sky.Editor.Services.Redirects;
    using Sky.Editor.Services.Slugs;
    using Sky.Editor.Services.Titles;

    /// <summary>
    /// Boots up the multi-tenant editor.
    /// </summary>
    internal static class SingleTenant
    {
        /// <summary>
        /// Builds up the multi-tenant editor.
        /// </summary>
        /// <param name="builder">Web application builder.</param>
        /// <returns>Returns a web application ready to run.</returns>
        internal static WebApplication BuildApp(WebApplicationBuilder builder)
        {
            // Add memory cache for Cosmos data logic and other services.
            builder.Services.AddMemoryCache();

            // Create one instance of the DefaultAzureCredential to be used throughout the application.
            var defaultAzureCredential = new DefaultAzureCredential();
            builder.Services.AddSingleton(defaultAzureCredential);

            // The following line enables Application Insights telemetry collection.
            // See: https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core?tabs=netcore6
            builder.Services.AddApplicationInsightsTelemetry();

            // Add Cosmos Options
            // Get the boot variables loaded, and
            // do some validation to make sure Cosmos can boot up
            // based on the values given.
            var cosmosStartup = new CosmosStartup(builder.Configuration);
            var option = cosmosStartup.Build();
            builder.Services.AddSingleton(option);

            // Database connection string
            var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection");

            // Backup storage connection string
            var backupConnectionString = builder.Configuration.GetConnectionString("BackupStorageConnectionString");
            if (!string.IsNullOrEmpty(backupConnectionString))
            {
                // Create the blob storage context for backup and restore of the database.
                builder.Services.AddHangfire(config => config
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseIgnoredAssemblyVersionTypeResolver()
                        .UseInMemoryStorage(new InMemoryStorageOptions
                        {
                            IdType = InMemoryStorageIdType.Long
                        }));

                builder.Services.AddHangfireServer(options =>
                {
                    options.Queues = new[] { "critical", "default" };
                    options.WorkerCount = Math.Max(Environment.ProcessorCount, 1);
                    options.SchedulePollingInterval = TimeSpan.FromMinutes(1);
                    options.ShutdownTimeout = TimeSpan.FromMinutes(2);
                    options.HeartbeatInterval = TimeSpan.FromMinutes(5);
                });
            }

            // If there is a backup connection string, then restore the database file now.
            // Also, on shutdown of the application, upload the database file to storage.
            if (!string.IsNullOrEmpty(backupConnectionString))
            {
                // Restore any files from blob storage to local file system.
                var restoreService = new FileBackupRestoreService(builder.Configuration, new MemoryCache(new MemoryCacheOptions()));
                restoreService.DownloadAsync(connectionString).Wait();
            }

            // If this is set, the Cosmos identity provider will:
            // 1. Create the database if it does not already exist.
            // 2. Create the required containers if they do not already exist.
            // IMPORTANT: Remove this variable if after first run. It will improve startup performance.
            // If the following is set, it will create the Cosmos database and
            //  required containers.
            if (option.Value.SiteSettings.AllowSetup)
            {
                using var context = new ApplicationDbContext(connectionString);

                if (context.Database.IsCosmos())
                {
                    // EnsureCreated is necessary for Cosmos DB to create the database and containers.
                    // It does not support migrations.
                    context.Database.EnsureCreatedAsync().Wait();
                }
                else
                {
                    context.Database.MigrateAsync().Wait();
                }
            }

            // Add the DB context using this approach instead of AddDbContext.
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                AspNetCore.Identity.FlexDb.CosmosDbOptionsBuilder.ConfigureDbOptions(options, connectionString);
            });

            // This service has to appear right after DB Context.
            builder.Services.AddTransient<IEditorSettings, EditorSettings>();
            builder.Services.AddTransient<IClock, SystemClock>();
            builder.Services.AddTransient<ISlugService, SlugService>();
            // Register required services for ArticleEditLogic
            builder.Services.AddTransient<IArticleHtmlService, ArticleHtmlService>();
            builder.Services.AddTransient<ICatalogService, CatalogService>();
            builder.Services.AddTransient<IPublishingService, PublishingService>();
            builder.Services.AddTransient<ITitleChangeService, TitleChangeService>();
            builder.Services.AddTransient<IRedirectService, RedirectService>();
            builder.Services.AddHttpContextAccessor();

            // Add Cosmos Identity here
            builder.Services.AddCosmosIdentity<ApplicationDbContext, IdentityUser, IdentityRole, string>(
                  options => options.SignIn.RequireConfirmedAccount = true)
                .AddDefaultUI() // Use this if Identity Scaffolding added
                .AddDefaultTokenProviders();

            builder.Services.AddDataProtection()
            .UseCryptographicAlgorithms(
            new AuthenticatedEncryptorConfiguration
            {
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
            }).PersistKeysToDbContext<ApplicationDbContext>();

            // ===========================================================
            // SUPPORTED OAuth Providers

            //-------------------------------
            // Add Google if keys are present
            var googleOAuth = builder.Configuration.GetSection("GoogleOAuth").Get<OAuth>();

            if (googleOAuth != null && googleOAuth.IsConfigured())
            {
                builder.Services.AddAuthentication().AddGoogle(options =>
                {
                    options.ClientId = googleOAuth.ClientId;
                    options.ClientSecret = googleOAuth.ClientSecret;
                });
            }

            // ---------------------------------
            // Add Microsoft if keys are present
            var entraIdOAuth = builder.Configuration.GetSection("MicrosoftOAuth").Get<OAuth>();

            if (entraIdOAuth != null && entraIdOAuth.IsConfigured())
            {
                builder.Services.AddAuthentication().AddMicrosoftAccount(options =>
                {
                    options.ClientId = entraIdOAuth.ClientId;
                    options.ClientSecret = entraIdOAuth.ClientSecret;

                    if (!string.IsNullOrEmpty(entraIdOAuth.TenantId))
                    {
                        // This is for registered apps in the Azure portal that are single tenant.
                        options.AuthorizationEndpoint = $"https://login.microsoftonline.com/{entraIdOAuth.TenantId}/oauth2/v2.0/authorize";
                        options.TokenEndpoint = $"https://login.microsoftonline.com/{entraIdOAuth.TenantId}/oauth2/v2.0/token";
                    }

                    if (!string.IsNullOrEmpty(entraIdOAuth.CallbackDomain))
                    {
                        options.Events.OnRedirectToAuthorizationEndpoint = context =>
                        {
                            var redirectUrl = Regex.Replace(context.RedirectUri, "redirect_uri=(.)+%2Fsignin-", $"redirect_uri=https%3A%2F%2F{entraIdOAuth.CallbackDomain}%2Fsignin-");
                            context.Response.Redirect(redirectUrl);
                            return Task.CompletedTask;
                        };
                    }
                });
            }

            // Add Azure CDN/Front door configuration here.
            // builder.Services.Configure<CdnService>(builder.Configuration.GetSection("AzureCdnConfig"));

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
                options.Cookie.IsEssential = true;
            });

            // Add Email services
            builder.Services.AddCosmosEmailServices(builder.Configuration);

            // Add the BLOB and File Storage contexts for Cosmos
            builder.Services.AddCosmosStorageContext(builder.Configuration);

            builder.Services.AddTransient<ArticleEditLogic>();

            // This is used by the ViewRenderingService 
            // to export web pages for external editing.
            builder.Services.AddScoped<IViewRenderService, ViewRenderService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "AllCors",
                    policy =>
                    {
                        policy.AllowAnyOrigin().AllowAnyMethod();
                    });
            });

            // Add this before identity
            // See also: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/response?view=aspnetcore-7.0
            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages();

            builder.Services.AddMvc()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ContractResolver =
                        new DefaultContractResolver())
                .AddRazorPagesOptions(options =>
                {
                    // This section docs are here: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-3.1&tabs=visual-studio#full 
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
                });

            // https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-2.1&tabs=visual-studio#http-strict-transport-security-protocol-hsts
            builder.Services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "CosmosAuthCookie";
                options.ExpireTimeSpan = TimeSpan.FromDays(5);
                options.SlidingExpiration = true;

                // This section docs are here: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-3.1&tabs=visual-studio#full
                // The following is when using Docker container with a proxy like
                // Azure front door. It ensures relative paths for redirects
                // which is necessary when the public DNS at Front door is www.mycompany.com 
                // and the DNS of the App Service is something like myappservice.azurewebsites.net.
                options.Events.OnRedirectToLogin = x =>
                {
                    var queryString = HttpUtility.UrlEncode(x.Request.QueryString.Value);
                    if (x.Request.Path.Equals("/Preview", StringComparison.InvariantCultureIgnoreCase))
                    {
                        x.Response.Redirect($"/Identity/Account/Login?returnUrl=/Home/Preview{queryString}");
                    }

                    x.Response.Redirect($"/Identity/Account/Login?returnUrl={x.Request.Path}{queryString}");
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToLogout = x =>
                {
                    x.Response.Redirect("/Identity/Account/Logout");
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = x =>
                {
                    x.Response.Redirect("/Identity/Account/AccessDenied");
                    return Task.CompletedTask;
                };
            });

            // BEGIN
            // When deploying to a Docker container, the OAuth redirect_url
            // parameter may have http instead of https.
            // Providers often do not allow http because it is not secure.
            // So authentication will fail.
            // Article below shows instructions for fixing this.
            //
            // NOTE: There is a companion secton below in the Configure method. Must have this
            //
            // https://seankilleen.com/2020/06/solved-net-core-azure-ad-in-docker-container-incorrectly-uses-an-non-https-redirect-uri/
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                           ForwardedHeaders.XForwardedProto;

                // Only loopback proxies are allowed by default.
                // Clear that restriction because forwarders are enabled by explicit
                // configuration.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // END
            builder.Services.AddResponseCaching();

            // Add the SignalR service.
            // If there is a DB connection, then use SQL backplane.
            // See: https://github.com/IntelliTect/IntelliTect.AspNetCore.SignalR.SqlServer
            var signalRConnection = builder.Configuration.GetConnectionString("CosmosSignalRConnection");

            // Add the SignalR service.
            // If there is a DB connection, then use SQL backplane.
            // See: https://github.com/IntelliTect/IntelliTect.AspNetCore.SignalR.SqlServer
            builder.Services.AddSignalR();

            // Throttle certain endpoints to protect the website.
            builder.Services.AddRateLimiter(_ => _
                .AddFixedWindowLimiter(policyName: "fixed", options =>
                {
                    options.PermitLimit = 4;
                    options.Window = TimeSpan.FromSeconds(8);
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = 2;
                }));

            // Register DomainEventDispatcher as the implementation for IDomainEventDispatcher using DI, resolving handlers from the service provider.
            builder.Services.AddTransient<IDomainEventDispatcher>(sp =>
                new DomainEventDispatcher(type => sp.GetServices(type)));

            var app = builder.Build();



            // https://seankilleen.com/2020/06/solved-net-core-azure-ad-in-docker-container-incorrectly-uses-an-non-https-redirect-uri/
            app.UseForwardedHeaders();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // app.UseHttpsRedirection(); // See: https://github.com/dotnet/aspnetcore/issues/18594
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors();

            app.UseResponseCaching(); // https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-3.1

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRateLimiter();

            app.MapGet("ccms__antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
            {
                var tokens = forgeryService.GetAndStoreTokens(context);
                context.Response.Headers["XSRF-TOKEN"] = tokens.RequestToken;
                return Results.Ok();
            });

            // Point to the route that will return the SignalR Hub.
            app.MapHub<LiveEditorHub>("/___cwps_hubs_live_editor");

            app.MapControllerRoute(
                "MsValidation",
                ".well-known/microsoft-identity-association.json",
                new { controller = "Home", action = "GetMicrosoftIdentityAssociation" }).AllowAnonymous();

            app.MapControllerRoute(
                "MyArea",
                "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "pub",
                pattern: "pub/{*index}",
                defaults: new { controller = "Pub", action = "Index" });

            app.MapControllerRoute(
                name: "blog",
                pattern: "blog/{page?}",
                defaults: new { controller = "Blog", action = "Index" });
            app.MapControllerRoute(
                name: "blog_post",
                pattern: "blog/post/{*slug}",
                defaults: new { controller = "Blog", action = "Post" });
            app.MapControllerRoute(
                name: "blog_rss",
                pattern: "blog/rss",
                defaults: new { controller = "Blog", action = "Rss" });

            app.MapControllerRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");

            // Deep path
            app.MapFallbackToController("Index", "Home");

            app.MapRazorPages();

            // Configure Hangfire recurring jobs
            if (!string.IsNullOrEmpty(backupConnectionString))
            {
                // Use a separate scope to register the recurring job
                using (var scope = app.Services.CreateScope())
                {
                    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                    var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    var dataSourcePart = Array.Find(parts, p => p.StartsWith("Data Source=", StringComparison.InvariantCultureIgnoreCase));
                    
                    var databasefilePath = dataSourcePart.Split('=')[1];
                    var databaseFileName = Path.GetFileName(databasefilePath);

                    // Schedule the backup to run every 5 minutes
                    // Cron expression: "*/5 * * * *" means every 5 minutes
                    recurringJobManager.AddOrUpdate<FileBackupRestoreService>(
                        "database-backup", // Job ID
                        job => job.UploadAsync(connectionString), // Run task.
                        "*/5 * * * *", // Cron expression for every 5 minutes
                        new RecurringJobOptions
                        {
                            TimeZone = TimeZoneInfo.Utc // Use UTC to avoid timezone issues
                        });
                }

                // Optional: Enable Hangfire Dashboard for monitoring
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new HangfireDashboardAuthorizationFilter() }
                });
            }

            // If there is a backup connection string, then on shutdown of the application,
            // upload the database file to blob storage.
            if (!string.IsNullOrEmpty(backupConnectionString))
            {
                var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
                lifetime.ApplicationStopping.Register(() =>
                {
                    try
                    {
                        var backupService = app.Services.GetRequiredService<FileBackupRestoreService>();
                        backupService.UploadAsync(connectionString).Wait();
                    }
                    catch (Exception ex)
                    {
                        // Logging may not be available here, so consider other ways to log the exception if needed.
                        Console.WriteLine($"Error during database backup on shutdown: {ex.Message}");
                    }
                });
            }

            return app;
        }
    }
}
