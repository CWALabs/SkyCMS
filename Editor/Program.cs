// <copyright file="Program.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

using AspNetCore.Identity.FlexDb.Extensions;
using Azure.Identity;
using Cosmos.BlobService;
using Cosmos.Cms.Common.Services.Configurations;
using Cosmos.Common.Data;
using Cosmos.Common.Services;
using Cosmos.Common.Services.Configurations;
using Cosmos.DynamicConfig;
using Cosmos.Editor.Services;
using Cosmos.EmailServices;
using Hangfire;
using Hangfire.InMemory;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Sky.Cms.Hubs;
using Sky.Cms.Services;
using Sky.Editor.Boot;
using Sky.Editor.Data.Logic;
using Sky.Editor.Domain.Events;
using Sky.Editor.Infrastructure.Time;
using Sky.Editor.Services;
using Sky.Editor.Services.Authors;
using Sky.Editor.Services.Catalog;
using Sky.Editor.Services.Html;
using Sky.Editor.Services.Publishing;
using Sky.Editor.Services.Redirects;
using Sky.Editor.Services.ReservedPaths;
using Sky.Editor.Services.Scheduling;
using Sky.Editor.Services.Slugs;
using Sky.Editor.Services.Titles;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using System.Web;

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

// Application Insights telemetry
// Add Cosmos Options
// Get the boot variables loaded, and
// do some validation to make sure Cosmos can boot up
// based on the values given.
var cosmosStartup = new CosmosStartup(builder.Configuration);
var options = cosmosStartup.Build();
builder.Services.AddSingleton(options);

// Add Hang fire services
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
    SingleTenant.Configure(builder, options);
}

// Register transient services - common to both single-tenant and multi-tenant modes
// Transient services are created each time they are requested.
builder.Services.AddTransient<ArticleVersionPublisher>();
builder.Services.AddTransient<ArticleEditLogic>();
builder.Services.AddTransient<IArticleHtmlService, ArticleHtmlService>();
builder.Services.AddTransient<IAuthorInfoService, AuthorInfoService>();
builder.Services.AddTransient<ICatalogService, CatalogService>();
builder.Services.AddTransient<IClock, SystemClock>();
builder.Services.AddTransient<IDomainEventDispatcher>(sp => new DomainEventDispatcher(type => sp.GetServices(type)));
builder.Services.AddTransient<IEditorSettings, EditorSettings>();
builder.Services.AddTransient<IPublishingService, PublishingService>();
builder.Services.AddTransient<IRedirectService, RedirectService>();
builder.Services.AddTransient<IReservedPaths, ReservedPaths>();
builder.Services.AddTransient<ISlugService, SlugService>();
builder.Services.AddTransient<ITitleChangeService, TitleChangeService>();
builder.Services.AddTransient<IViewRenderService, ViewRenderService>();
builder.Services.AddHttpContextAccessor();

// ---------------------------------------------------------------
// Continue registering services common to both single-tenant and multi-tenant modes
// ---------------------------------------------------------------
builder.Services.AddCosmosEmailServices(builder.Configuration); // Add Email services
builder.Services.AddCosmosStorageContext(builder.Configuration); // Add the BLOB and File Storage contexts for Cosmos
builder.Services.AddCosmosCmsDataProtection(builder.Configuration, defaultAzureCredential); // Add shared data protection here
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
    recurring.AddOrUpdate<ArticleVersionPublisher>(
        "article-version-publisher",
        x => x.ExecuteAsync(),
        "*/5 * * * *");
}

await app.RunAsync();