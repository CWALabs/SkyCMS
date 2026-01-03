// <copyright file="Program.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

using AspNetCore.Identity.FlexDb.Extensions;
using Azure.Identity;
using Cosmos.BlobService;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Common;
using Cosmos.Common.Data;
using Cosmos.Common.Models;
using Cosmos.Common.Services.Configurations;
using Cosmos.DynamicConfig;
using Hangfire;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Sky.Cms.Hubs;
using Sky.Cms.Services;
using Sky.Editor.Boot;
using Sky.Editor.Data.Logic;
using Sky.Editor.Domain.Events;
using Sky.Editor.Features.Articles.Create;
using Sky.Editor.Features.Articles.Save;
using Sky.Editor.Features.Shared;
using Sky.Editor.Infrastructure.Time;
using Sky.Editor.Services.Authors;
using Sky.Editor.Services.BlogPublishing;
using Sky.Editor.Services.Catalog;
using Sky.Editor.Services.CDN;
using Sky.Editor.Services.Diagnostics;
using Sky.Editor.Services.EditorSettings;
using Sky.Editor.Services.Email;
using Sky.Editor.Services.Html;
using Sky.Editor.Services.Layouts;
using Sky.Editor.Services.Publishing;
using Sky.Editor.Services.Redirects;
using Sky.Editor.Services.ReservedPaths;
using Sky.Editor.Services.Scheduling;
using Sky.Editor.Services.Setup;
using Sky.Editor.Services.Slugs;
using Sky.Editor.Services.Templates;
using Sky.Editor.Services.Titles;
using Sky.Editor.Middleware;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using System.Web;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// STEP 1: DETERMINE DEPLOYMENT MODE
// ---------------------------------------------------------------
var isMultiTenantEditor = builder.Configuration.GetValue<bool?>("MultiTenantEditor") ?? false;
var allowSetup = builder.Configuration.GetValue<bool?>("CosmosAllowSetup") ?? false;
var enableDiagnostics = builder.Configuration.GetValue<bool?>("EnableDiagnostics") ?? false;
var versionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();

if (enableDiagnostics)
{
    System.Console.WriteLine("⚠️ DIAGNOSTIC MODE ENABLED - Normal startup will be bypassed");
}
else
{
    System.Console.WriteLine($"Starting Cosmos CMS Editor in {(isMultiTenantEditor ? "Multi-Tenant" : "Single-Tenant")} Mode (v.{versionNumber}).");
}

// ---------------------------------------------------------------
// STEP 1.5: ENTER DIAGNOSTIC MODE IF INDICATED
// ---------------------------------------------------------------
if (enableDiagnostics)
{
    bool configurationValid = true;
    ValidationResult? earlyValidationResult = null;

    System.Console.WriteLine("Diagnostic mode is enabled - performing early configuration validation...");

    // Perform synchronous validation WITHOUT requiring any services
    var loggerFactory = LoggerFactory.Create(config => config.AddConsole());
    var logger = loggerFactory.CreateLogger<ConfigurationValidator>();
    var validator = new ConfigurationValidator(builder.Configuration, logger);

    // Run validation synchronously at startup
    earlyValidationResult = validator.ValidateAsync().GetAwaiter().GetResult();
    configurationValid = earlyValidationResult.IsValid;

    if (!configurationValid)
    {
        System.Console.WriteLine("⚠️ Configuration validation FAILED - starting in diagnostic-only mode");
        System.Console.WriteLine($"   Errors: {earlyValidationResult.Checks.Count(c => c.Status == CheckStatus.Error)}");
        System.Console.WriteLine($"   Warnings: {earlyValidationResult.Checks.Count(c => c.Status == CheckStatus.Warning)}");
    }
    else
    {
        System.Console.WriteLine("✅ Configuration validation passed");
    }

    // DIAGNOSTIC-ONLY MODE: Minimal services to show diagnostic page
    System.Console.WriteLine("Registering minimal services for diagnostic-only mode...");

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ConfigurationValidator>();
    builder.Services.AddRazorPages();
    builder.Services.AddControllersWithViews();

    // Build minimal app
    var diagnosticApp = builder.Build();

    // Configure minimal middleware pipeline
    diagnosticApp.UseRouting();
    diagnosticApp.UseStaticFiles();

    // Redirect ALL requests to diagnostic page
    diagnosticApp.Use(async (context, next) =>
    {
        if (!context.Request.Path.StartsWithSegments("/___diagnostics") &&
            !context.Request.Path.StartsWithSegments("/lib") &&
            !context.Request.Path.StartsWithSegments("/css") &&
            !context.Request.Path.StartsWithSegments("/js") &&
            !context.Request.Path.Value.EndsWith(".css") &&
            !context.Request.Path.Value.EndsWith(".js"))
        {
            context.Response.Redirect("/___diagnostics");
            return;
        }

        await next();
    });

    diagnosticApp.MapRazorPages();

    System.Console.WriteLine("🔧 Application started in DIAGNOSTIC-ONLY mode");
    System.Console.WriteLine("   Navigate to: /___diagnostics");
    System.Console.WriteLine("   Fix configuration issues and restart the application");

    await diagnosticApp.RunAsync();
    return; // Exit here - don't continue with normal startup
}

// ---------------------------------------------------------------
// STEP 2: Register Core Infrastructure (Common to Both Modes)
// ---------------------------------------------------------------
builder.Services.AddMemoryCache();
var defaultAzureCredential = new DefaultAzureCredential();
builder.Services.AddSingleton(defaultAzureCredential);
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// ---------------------------------------------------------------
// STEP 3: Configure Mode-Specific Services
// ---------------------------------------------------------------
if (isMultiTenantEditor)
{
    // Multi-tenant: Dynamic configuration per tenant
    builder.Services.AddSingleton<IDynamicConfigurationProvider, DynamicConfigurationProvider>();
    builder.Services.AddScoped<IMultiTenantSetupService, MultiTenantSetupService>();
    MultiTenant.Configure(builder, defaultAzureCredential);
}
else
{
    // Single-tenant: Traditional setup wizard
    SingleTenant.Configure(builder);
}

// ---------------------------------------------------------------
// STEP 4: Register Site Settings
// ---------------------------------------------------------------
builder.Services.Configure<SiteSettings>(settings =>
{
    settings.MultiTenantEditor = isMultiTenantEditor;
    settings.AllowSetup = allowSetup;
    settings.CosmosRequiresAuthentication = builder.Configuration.GetValue<bool?>("CosmosRequiresAuthentication") ?? false;
    settings.AllowLocalAccounts = builder.Configuration.GetValue<bool?>("AllowLocalAccounts") ?? true;
    settings.AllowedFileTypes = ".js,.css,.htm,.html,.mov,.webm,.avi,.mp4,.mpeg,.ts,.svg,.json";
    settings.MultiTenantRedirectUrl = builder.Configuration.GetValue<string>("MultiTenantRedirectUrl") ?? string.Empty;
    settings.MicrosoftAppId = builder.Configuration.GetValue<string>("MicrosoftAppId") ?? string.Empty;
});

// ---------------------------------------------------------------
// STEP 5: Register OAuth Configuration (if available)
// ---------------------------------------------------------------
var microsoftAuth = builder.Configuration.GetSection("MicrosoftOAuth").Get<AzureAD>()
                    ?? builder.Configuration.GetSection("AzureAD").Get<AzureAD>();
if (microsoftAuth != null)
{
    builder.Services.AddSingleton(microsoftAuth);
}

// ---------------------------------------------------------------
// STEP 6: Register Application Services
// ---------------------------------------------------------------

// Scoped services (per-request lifecycle, can access HttpContext)
builder.Services.AddScoped<ISetupService, SetupService>();
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped<ICommandHandler<CreateArticleCommand, CommandResult<ArticleViewModel>>, CreateArticleHandler>();
builder.Services.AddScoped<ICommandHandler<SaveArticleCommand, CommandResult<ArticleUpdateResult>>, SaveArticleHandler>();
builder.Services.AddScoped<ILayoutImportService, LayoutImportService>();
builder.Services.AddScoped<IStorageContext, StorageContext>();
builder.Services.AddScoped<IEditorSettings, EditorSettings>(); // CHANGED: Scoped for per-request tenant context
builder.Services.AddScoped<IViewRenderService, ViewRenderService>(); // CHANGED: Scoped for Razor view rendering

// Transient services (stateless operations, created each time)
builder.Services.AddTransient<ICdnServiceFactory, CdnServiceFactory>();
builder.Services.AddTransient<ITemplateService, TemplateService>();
builder.Services.AddTransient<IArticleHtmlService, ArticleHtmlService>();
builder.Services.AddTransient<IAuthorInfoService, AuthorInfoService>();
builder.Services.AddTransient<ICatalogService, CatalogService>();
builder.Services.AddTransient<IClock, Sky.Editor.Infrastructure.Time.SystemClock>();
builder.Services.AddTransient<IDomainEventDispatcher>(sp => new DomainEventDispatcher(type => sp.GetServices(type)));
builder.Services.AddTransient<IPublishingService, PublishingService>();
builder.Services.AddTransient<IRedirectService, RedirectService>();
builder.Services.AddTransient<IReservedPaths, ReservedPaths>();
builder.Services.AddTransient<ISlugService, SlugService>();
builder.Services.AddTransient<ITitleChangeService, TitleChangeService>();
builder.Services.AddTransient<IBlogRenderingService, BlogRenderingService>();
builder.Services.AddTransient<IEmailConfigurationService, EmailConfigurationService>();
builder.Services.AddTransient<ArticleScheduler>();
builder.Services.AddTransient<ArticleEditLogic>();
builder.Services.AddTransient<ISetupCheckService, SetupCheckService>();

// Register validator for diagnostic page (if setup allowed)
if (allowSetup)
{
    builder.Services.AddScoped<ConfigurationValidator>();
}

// ---------------------------------------------------------------
// STEP 7: Register Background Job Services
// ---------------------------------------------------------------
builder.Services.AddHangFireScheduling(builder.Configuration);

// Reduce Hangfire logging noise
builder.Logging.AddFilter("Hangfire", LogLevel.Warning);
builder.Logging.AddFilter("Hangfire.Server.ServerHeartbeatProcess", LogLevel.Error);
builder.Logging.AddFilter("Hangfire.Server.BackgroundProcessingServer", LogLevel.Warning);
builder.Logging.AddFilter("Hangfire.Server.ServerWatchdog", LogLevel.Error);

// ---------------------------------------------------------------
// STEP 8: Register Data Protection & SignalR
// ---------------------------------------------------------------
builder.Services.AddFlexDbDataProtection(builder.Configuration);
builder.Services.AddSignalR();

// ---------------------------------------------------------------
// STEP 9: Register MVC & Razor Pages
// ---------------------------------------------------------------
builder.Services.AddControllersWithViews();

builder.Services.AddCosmosIdentity<ApplicationDbContext, IdentityUser, IdentityRole, string>(
      options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI()
    .AddDefaultTokenProviders();

// ---------------------------------------------------------------
// STEP 10: Configure OAuth Providers
// ---------------------------------------------------------------
var googleOAuth = builder.Configuration.GetSection("GoogleOAuth").Get<OAuth>();
if (googleOAuth != null && googleOAuth.IsConfigured())
{
    builder.Services.AddAuthentication().AddGoogle(options =>
    {
        options.ClientId = googleOAuth.ClientId;
        options.ClientSecret = googleOAuth.ClientSecret;
    });
}

var entraIdOAuth = builder.Configuration.GetSection("MicrosoftOAuth").Get<OAuth>();
if (entraIdOAuth != null && entraIdOAuth.IsConfigured())
{
    builder.Services.AddAuthentication().AddMicrosoftAccount(options =>
    {
        options.ClientId = entraIdOAuth.ClientId;
        options.ClientSecret = entraIdOAuth.ClientSecret;

        if (!string.IsNullOrEmpty(entraIdOAuth.TenantId))
        {
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

// ---------------------------------------------------------------
// STEP 11: Configure Session & Razor Pages Routes
// ---------------------------------------------------------------
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(3600);
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages(options =>
{
    if (!isMultiTenantEditor)
    {
        // Single-tenant setup wizard pages
        options.Conventions.AddAreaPageRoute("Setup", "/Index", "___setup");
        options.Conventions.AddAreaPageRoute("Setup", "/Step1_Mode", "___setup/mode");
        options.Conventions.AddAreaPageRoute("Setup", "/Step2_Storage", "___setup/storage");
        options.Conventions.AddAreaPageRoute("Setup", "/Step3_AdminAccount", "___setup/admin");
        options.Conventions.AddAreaPageRoute("Setup", "/Step4_Publisher", "___setup/publisher");
        options.Conventions.AddAreaPageRoute("Setup", "/Step5_Email", "___setup/email");
        options.Conventions.AddAreaPageRoute("Setup", "/Step6_Review", "___setup/review");
        options.Conventions.AddAreaPageRoute("Setup", "/Complete", "___setup/complete");
    }
    else
    {
        // Multi-tenant setup pages (simplified)
        options.Conventions.AddAreaPageRoute("Setup", "/Tenant/Index", "___setup");
        options.Conventions.AddAreaPageRoute("Setup", "/Tenant/Index", "___setup/tenant");
        options.Conventions.AddAreaPageRoute("Setup", "/Tenant/Admin", "___setup/tenant/admin");
        options.Conventions.AddAreaPageRoute("Setup", "/Tenant/Complete", "___setup/tenant/complete");
    }
});

builder.Services.AddRazorPages();

builder.Services.AddMvc()
    .AddNewtonsoftJson(options =>
        options.SerializerSettings.ContractResolver = new DefaultContractResolver())
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
        options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
    });

// ---------------------------------------------------------------
// STEP 12: Configure CORS & Security
// ---------------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllCors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod();
    });
});

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

    // For multi-tenant: dynamically set cookie domain based on x-origin-hostname header
    if (isMultiTenantEditor)
    {
        options.Events.OnValidatePrincipal = async context =>
        {
            var httpContext = context.HttpContext;
            var xOriginHostname = httpContext.Request.Headers["x-origin-hostname"].ToString();
            
            var currentDomain = !string.IsNullOrWhiteSpace(xOriginHostname) 
                ? xOriginHostname.ToLowerInvariant() 
                : httpContext.Request.Host.Host.ToLowerInvariant();
            
            if (context.Principal?.Identity?.IsAuthenticated == true)
            {
                var cookieDomainClaim = context.Principal.FindFirst("CookieDomain");
                if (cookieDomainClaim != null && !cookieDomainClaim.Value.Equals(currentDomain, StringComparison.OrdinalIgnoreCase))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
                }
            }
        };
        
        options.Events.OnSigningIn = context =>
        {
            var httpContext = context.HttpContext;
            var xOriginHostname = httpContext.Request.Headers["x-origin-hostname"].ToString();
            
            var currentDomain = !string.IsNullOrWhiteSpace(xOriginHostname) 
                ? xOriginHostname.ToLowerInvariant() 
                : httpContext.Request.Host.Host.ToLowerInvariant();
            
            var identity = (System.Security.Claims.ClaimsIdentity)context.Principal.Identity;
            identity.AddClaim(new System.Security.Claims.Claim("CookieDomain", currentDomain));
            
            return Task.CompletedTask;
        };
        
        options.Cookie.Domain = null;
    }
    
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;

    options.Events.OnRedirectToLogin = x =>
    {
        var queryParams = System.Web.HttpUtility.ParseQueryString(x.Request.QueryString.Value);

        var website = queryParams["ccmswebsite"];
        var opt = queryParams["ccmsopt"];
        var email = queryParams["ccmsemail"];
        queryParams.Remove("ccmswebsite");
        queryParams.Remove("ccmsopt");
        queryParams.Remove("ccmsemail");
        var queryString = HttpUtility.UrlEncode(queryParams.ToString());

        if (x.Request.Path.Equals("/Preview", StringComparison.InvariantCultureIgnoreCase))
        {
            x.Response.Redirect($"/Identity/Account/Login?returnUrl=/Home/Preview?{queryString}");
        }

        x.Response.Redirect($"/Identity/Account/Login?returnUrl={x.Request.Path}&{queryString}");
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

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ---------------------------------------------------------------
// STEP 13: Configure Rate Limiting
// ---------------------------------------------------------------
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(8);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));

// Configure rate limiting for deployment API
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("deployment", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(5);
        opt.PermitLimit = 10;  // Max 10 deployments per 5 minutes per IP
        opt.QueueLimit = 0;    // No queuing
    });

    // Add a global rate limiter for general API protection
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Exempt deployment endpoint from global limits (it has its own)
        if (context.Request.Path.StartsWithSegments("/api/deployment"))
        {
            return RateLimitPartition.GetNoLimiter("deployment-exempt");
        }

        // Apply general rate limiting to other API endpoints
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 100
            });
    });

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new
            {
                success = false,
                error = "Too many requests. Please try again later."
            }, cancellationToken);
    };
});

// ---------------------------------------------------------------
// BUILD APPLICATION
// ---------------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------------
// CONFIGURE MIDDLEWARE PIPELINE
// --------------------------------------------------------------

// Multi-tenant middleware (must run early in pipeline)
if (isMultiTenantEditor)
{
    app.UseMiddleware<DomainMiddleware>();
    app.UseTenantSetupRedirect();
}

// ---------------------------------------------------------------
// STEP: Setup Detection (for valid configurations)
// ---------------------------------------------------------------
if (!isMultiTenantEditor && allowSetup)
{
    app.Use(async (context, next) =>
    {
        // Skip setup check for setup wizard pages, static files, and health checks
        if (context.Request.Path.StartsWithSegments("/___setup") ||
        context.Request.Path.StartsWithSegments("/setup") ||
            context.Request.Path.StartsWithSegments("/lib") ||
            context.Request.Path.StartsWithSegments("/css") ||
            context.Request.Path.StartsWithSegments("/js") ||
            context.Request.Path.StartsWithSegments("/images") ||
            context.Request.Path.StartsWithSegments("/fonts") ||
            context.Request.Path.Value.EndsWith(".css") ||
            context.Request.Path.Value.EndsWith(".js") ||
            context.Request.Path.Value.EndsWith(".map") ||
            context.Request.Path.StartsWithSegments("/healthz") ||
            context.Request.Path.StartsWithSegments("/.well-known"))
        {
            await next();
            return;
        }

        // Check if setup is complete
        var setupService = context.RequestServices.GetRequiredService<ISetupService>();
        var isComplete = await setupService.IsSetupCompleteAsync();

        if (!isComplete)
        {
            // Redirect to setup wizard
            context.Response.Redirect("/___setup");
            return;
        }

        await next();
    });
}

app.UseCosmosCmsDataProtection();

// CloudFront protocol mapping (before UseForwardedHeaders)
app.Use(async (context, next) =>
{
    var headers = context.Request.Headers;
    if (headers.TryGetValue("CloudFront-Forwarded-Proto", out var cfProto))
    {
        headers["X-Forwarded-Proto"] = cfProto;
    }

    await next();
});

app.UseForwardedHeaders();

// Setup wizard access control
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/___setup"))
    {
        var config = context.RequestServices.GetRequiredService<IConfiguration>();
        
        if (!isMultiTenantEditor)
        {
            var allowSetup = config.GetValue<bool?>("CosmosAllowSetup") ?? false;
            if (!allowSetup)
            {
                context.Response.Redirect("/");
                return;
            }
        }
        else
        {
            var setupService = context.RequestServices.GetService<IMultiTenantSetupService>();
            if (setupService != null)
            {
                var requiresSetup = await setupService.TenantRequiresSetupAsync();
                if (!requiresSetup)
                {
                    context.Response.Redirect("/");
                    return;
                }
            }
            else
            {
                context.Response.Redirect("/");
                return;
            }
        }
    }

    await next();
});

// Environment-specific middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".js"))
        {
            ctx.Context.Response.Headers.Append("Content-Type", "application/javascript");
        }
    }
});

app.UseRouting();
app.UseCors();
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.UseHangfireSchedulingSlice();

// ---------------------------------------------------------------
// CONFIGURE ENDPOINTS
// ---------------------------------------------------------------
app.MapGet("/___healthz", () => Results.Ok(new { status = "ok" }));

app.MapGet("ccms__antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
{
    var tokens = forgeryService.GetAndStoreTokens(context);
    context.Response.Headers["XSRF-TOKEN"] = tokens.RequestToken;
    return Results.Ok();
});

app.MapHub<LiveEditorHub>("/___cwps_hubs_live_editor");
app.MapControllerRoute("MsValidation", ".well-known/microsoft-identity-association.json", new { controller = "Home", action = "GetMicrosoftIdentityAssociation" }).AllowAnonymous();
app.MapControllerRoute("MyArea", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "pub", pattern: "pub/{*index}", defaults: new { controller = "Pub", action = "Index" });
app.MapControllerRoute(name: "blog", pattern: "blog/{page?}", defaults: new { controller = "Blog", action = "Index" });
app.MapControllerRoute(name: "blog_post", pattern: "blog/post/{*slug}", defaults: new { controller = "Blog", action = "Post" });
app.MapControllerRoute(name: "blog_rss", pattern: "blog/rss", defaults: new { controller = "Blog", action = "Rss" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages().WithMetadata(new { Area = "Setup" });
app.MapAreaControllerRoute(
    name: "setup",
    areaName: "Setup",
    pattern: "___setup/{*pathInfo}",
    defaults: new { page = "/Index" });

app.MapFallbackToController("Index", "Home");
app.MapRazorPages();

// ---------------------------------------------------------------
// CONFIGURE BACKGROUND JOBS
// ---------------------------------------------------------------
try
{
    using (var scope = app.Services.CreateScope())
    {
        var recurring = scope.ServiceProvider.GetService<IRecurringJobManager>();
        
        if (recurring != null)
        {
            recurring.AddOrUpdate<ArticleScheduler>(
                "article-version-publisher",
                x => x.ExecuteAsync(),
                Cron.MinuteInterval(10));
        }
    }
}
catch (Exception ex)
{
    app.Logger.LogInformation("Hangfire recurring jobs not configured: {Message}", ex.Message);
}

await app.RunAsync();
