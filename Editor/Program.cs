// <copyright file="Program.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using System.Web;
using AspNetCore.Identity.FlexDb.Extensions;
using Azure.Identity;
using Cosmos.BlobService;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Common;
using Cosmos.Common.Data;
using Cosmos.Common.Models;
using Cosmos.Common.Services.Configurations;
using Cosmos.DynamicConfig;
using Cosmos.EmailServices;
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
using Sky.Editor.Services.EditorSettings;
using Sky.Editor.Services.Html;
using Sky.Editor.Services.Publishing;
using Sky.Editor.Services.Redirects;
using Sky.Editor.Services.ReservedPaths;
using Sky.Editor.Services.Scheduling;
using Sky.Editor.Services.Slugs;
using Sky.Editor.Services.Templates;
using Sky.Editor.Services.Titles;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

var isMultiTenantEditor = builder.Configuration.GetValue<bool?>("MultiTenantEditor") ?? false;
var versionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();

// ---------------------------------------------------------------
// Register base services that are common to both single-tenant and multi-tenant modes
//
builder.Services.AddMemoryCache(); // Add memory cache for Cosmos data logic and other services.
var defaultAzureCredential = new DefaultAzureCredential(); // Create one instance of the DefaultAzureCredential to be used throughout the application.
builder.Services.AddSingleton(defaultAzureCredential);
builder.Services.AddApplicationInsightsTelemetry();

// Register SiteSettings directly from configuration
builder.Services.Configure<SiteSettings>(settings =>
{
    settings.MultiTenantEditor = builder.Configuration.GetValue<bool?>("MultiTenantEditor") ?? false;
    settings.AllowSetup = builder.Configuration.GetValue<bool?>("CosmosAllowSetup") ?? false;
    settings.CosmosRequiresAuthentication = builder.Configuration.GetValue<bool?>("CosmosRequiresAuthentication") ?? false;
    settings.AllowLocalAccounts = builder.Configuration.GetValue<bool?>("AllowLocalAccounts") ?? true;
    settings.AllowedFileTypes = ".js,.css,.htm,.html,.mov,.webm,.avi,.mp4,.mpeg,.ts,.svg,.json";
    settings.MultiTenantRedirectUrl = builder.Configuration.GetValue<string>("MultiTenantRedirectUrl") ?? string.Empty;
    settings.MicrosoftAppId = builder.Configuration.GetValue<string>("MicrosoftAppId") ?? string.Empty;
});

// If you need MicrosoftAppId separately for some services:
var microsoftAuth = builder.Configuration.GetSection("MicrosoftOAuth").Get<AzureAD>()
                    ?? builder.Configuration.GetSection("AzureAD").Get<AzureAD>();
if (microsoftAuth != null)
{
    builder.Services.AddSingleton(microsoftAuth);
}

// ---------------------------------------------------------------
// Build the app based on single-tenant or multi-tenant mode
// ---------------------------------------------------------------
if (isMultiTenantEditor)
{
    System.Console.WriteLine($"Starting Cosmos CMS Editor in Multi-Tenant Mode (v.{versionNumber}).");
    MultiTenant.Configure(builder, defaultAzureCredential);
}
else
{
    System.Console.WriteLine($"Starting Cosmos CMS Editor in Single-Tenant Mode (v.{versionNumber}).");
    SingleTenant.Configure(builder);
}

// Register transient services - common to both single-tenant and multi-tenant modes
// Transient services are created each time they are requested.
builder.Services.AddTransient<ICdnServiceFactory, CdnServiceFactory>();
builder.Services.AddTransient<ITemplateService, TemplateService>();
builder.Services.AddTransient<IArticleHtmlService, ArticleHtmlService>();
builder.Services.AddTransient<IAuthorInfoService, AuthorInfoService>();
builder.Services.AddTransient<ICatalogService, CatalogService>();
builder.Services.AddTransient<IClock, Sky.Editor.Infrastructure.Time.SystemClock>();
builder.Services.AddTransient<IDomainEventDispatcher>(sp => new DomainEventDispatcher(type => sp.GetServices(type)));
builder.Services.AddTransient<IEditorSettings, EditorSettings>();
builder.Services.AddTransient<IPublishingService, PublishingService>();
builder.Services.AddTransient<IRedirectService, RedirectService>();
builder.Services.AddTransient<IReservedPaths, ReservedPaths>();
builder.Services.AddTransient<ISlugService, SlugService>();
builder.Services.AddTransient<IViewRenderService, ViewRenderService>();
builder.Services.AddTransient<ITitleChangeService, TitleChangeService>();
builder.Services.AddTransient<IBlogRenderingService, BlogRenderingService>();
builder.Services.AddTransient<StorageContext>();
builder.Services.AddTransient<ArticleScheduler>();
builder.Services.AddTransient<ArticleEditLogic>();
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------------
// Register Vertical Slice Architecture Feature Handlers
// ---------------------------------------------------------------
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped<ICommandHandler<CreateArticleCommand, CommandResult<ArticleViewModel>>, CreateArticleHandler>();
builder.Services.AddScoped<ICommandHandler<SaveArticleCommand, CommandResult<ArticleUpdateResult>>, SaveArticleHandler>();

// ---------------------------------------------------------------
// Continue registering services common to both single-tenant and multi-tenant modes
// ---------------------------------------------------------------
builder.Services.AddHangFireScheduling(builder.Configuration); // Add Hangfire services for scheduling

// Add logging to see Hangfire queries
builder.Logging.AddFilter("Hangfire", LogLevel.Debug);
builder.Services.AddCosmosEmailServices(builder.Configuration); // Add Email services
builder.Services.AddFlexDbDataProtection(builder.Configuration); // Add shared data protection here
builder.Services.AddSignalR(); // Add SignalR services

// Add this before identity
// See also: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/response?view=aspnetcore-7.0
builder.Services.AddControllersWithViews();

// Add Cosmos Identity here
builder.Services.AddCosmosIdentity<ApplicationDbContext, IdentityUser, IdentityRole, string>(
      options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI() // Use this if Identity Scaffolding added
    .AddDefaultTokenProviders();

// -------------------------------
// SUPPORTED OAuth Providers
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

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromSeconds(3600);
    options.Cookie.IsEssential = true;
});
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
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllCors",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod();
        });
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

    // For multi-tenant: dynamically set cookie domain based on x-origin-hostname header
    if (isMultiTenantEditor)
    {
        options.Events.OnValidatePrincipal = async context =>
        {
            var httpContext = context.HttpContext;
            var xOriginHostname = httpContext.Request.Headers["x-origin-hostname"].ToString();
            
            // Get the current domain from x-origin-hostname header or request host
            var currentDomain = !string.IsNullOrWhiteSpace(xOriginHostname) 
                ? xOriginHostname.ToLowerInvariant() 
                : httpContext.Request.Host.Host.ToLowerInvariant();
            
            // If cookie was issued for a different domain, reject the authentication
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
            
            // Store the domain where the cookie was issued
            var currentDomain = !string.IsNullOrWhiteSpace(xOriginHostname) 
                ? xOriginHostname.ToLowerInvariant() 
                : httpContext.Request.Host.Host.ToLowerInvariant();
            
            var identity = (System.Security.Claims.ClaimsIdentity)context.Principal.Identity;
            identity.AddClaim(new System.Security.Claims.Claim("CookieDomain", currentDomain));
            
            return Task.CompletedTask;
        };
        
        // Don't set a specific domain - let it default to the current request domain
        options.Cookie.Domain = null;
    }
    
    // Ensure cookie security
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;

    // This section docs are here: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/scaffold-identity?view=aspnetcore-3.1&tabs=visual-studio#full
    // The following is when using Docker container with a proxy like
    // Azure front door. It ensures relative paths for redirects
    // which is necessary when the public DNS at Front door is www.mycompany.com 
    // and the DNS of the App Service is something like myappservice.azurewebsites.net.
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

// Throttle certain endpoints to protect the website.
builder.Services.AddRateLimiter(_ => _
    .AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(8);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));

var app = builder.Build();

if (isMultiTenantEditor)
{
    app.UseMiddleware<DomainMiddleware>();
}

app.UseCosmosCmsDataProtection(); // Enable data protection services for Cosmos CMS.
app.UseForwardedHeaders(); // https://seankilleen.com/2020/06/solved-net-core-azure-ad-in-docker-container-incorrectly-uses-an-non-https-redirect-uri/

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Ensure .js files are served with correct MIME type
        if (ctx.File.Name.EndsWith(".js"))
        {
            ctx.Context.Response.Headers.Append("Content-Type", "application/javascript");
        }
    }
});
app.UseRouting();
app.UseCors();
app.UseResponseCaching(); // https://docs.microsoft.com/en-us/aspnet/core/performance/caching/middleware?view=aspnetcore-3.1
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.UseHangfireDashboard("/Editor/CCMS___PageScheduler", new DashboardOptions()
{
    DashboardTitle = "SkyCMS - Page Scheduler",
    Authorization = new[] { new Sky.Editor.Services.Scheduling.HangfireAuthorizationFilter() },
});

app.MapGet("ccms__antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
{
    var tokens = forgeryService.GetAndStoreTokens(context);
    context.Response.Headers["XSRF-TOKEN"] = tokens.RequestToken;
    return Results.Ok();
});

app.MapHub<LiveEditorHub>("/___cwps_hubs_live_editor"); // Point to the route that will return the SignalR Hub.
app.MapControllerRoute("MsValidation", ".well-known/microsoft-identity-association.json", new { controller = "Home", action = "GetMicrosoftIdentityAssociation" }).AllowAnonymous();
app.MapControllerRoute("MyArea", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(name: "pub", pattern: "pub/{*index}", defaults: new { controller = "Pub", action = "Index" });
app.MapControllerRoute(name: "blog", pattern: "blog/{page?}", defaults: new { controller = "Blog", action = "Index" });
app.MapControllerRoute(name: "blog_post", pattern: "blog/post/{*slug}", defaults: new { controller = "Blog", action = "Post" });
app.MapControllerRoute(name: "blog_rss", pattern: "blog/rss", defaults: new { controller = "Blog", action = "Rss" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapFallbackToController("Index", "Home"); // Deep path

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var recurring = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // For async method on a service type:
    recurring.AddOrUpdate<ArticleScheduler>(
        "article-version-publisher",
        x => x.ExecuteAsync(),
        Cron.MinuteInterval(10)); // Runs every 10 minutes
}

await app.RunAsync();