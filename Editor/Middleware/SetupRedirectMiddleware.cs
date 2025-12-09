// <copyright file="SetupRedirectMiddleware.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Middleware
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Middleware that automatically redirects to the setup wizard if the application is not configured.
    /// </summary>
    public class SetupRedirectMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<SetupRedirectMiddleware> logger;
        private readonly bool allowSetup;
        private static bool? isSetupCompleted = null;
        private static readonly object lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupRedirectMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next request delegate.</param>
        /// <param name="logger">Logger instance.</param>
        /// <param name="configuration">Configuration provider.</param>
        public SetupRedirectMiddleware(RequestDelegate next, ILogger<SetupRedirectMiddleware> logger, IConfiguration configuration)
        {
            this.next = next;
            this.logger = logger;
            allowSetup = configuration.GetValue<bool?>("CosmosAllowSetup") ?? false;
        }

        /// <summary>
        /// Invokes the middleware.
        /// </summary>
        /// <param name="context">HTTP context.</param>
        /// <param name="dbContext">Database context.</param>
        /// <returns>Task.</returns>
        public async Task InvokeAsync(
            HttpContext context,
            ApplicationDbContext dbContext)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Skip for setup pages themselves (by path or area)
            if (context.Request.Path.StartsWithSegments("/___setup") || 
                IsSetupArea(context))
            {
                await next(context);
                return;
            }

            // Skip for static resources
            if (IsStaticResource(path))
            {
                await next(context);
                return;
            }

            // Skip for Identity pages (login, register, etc.)
            if (context.Request.Path.StartsWithSegments("/Identity"))
            {
                await next(context);
                return;
            }

            // If setup is not allowed, skip the check entirely
            if (!allowSetup)
            {
                await next(context);
                return;
            }

            // Check setup status only once (lazy initialization with thread safety)
            if (!isSetupCompleted.HasValue)
            {
                lock (lockObject)
                {
                    if (!isSetupCompleted.HasValue)
                    {
                        isSetupCompleted = CheckSetupCompletedAsync(dbContext).Result;
                    }
                }
            }

            // Redirect to setup if not completed and we are not already in setup wizard.
            if (isSetupCompleted == false && context.Request.Path.StartsWithSegments("/Setup") == false)
            {
                logger.LogInformation("Setup not completed, redirecting to setup wizard");
                context.Response.Redirect("/___setup");
                return;
            }

            await next(context);
        }

        /// <summary>
        /// Checks if the current request is for the Setup area.
        /// </summary>
        /// <param name="context">HTTP context.</param>
        /// <returns>True if request is for Setup area, false otherwise.</returns>
        private static bool IsSetupArea(HttpContext context)
        {
            // Check if the area route value is "Setup"
            if (context.GetRouteData()?.Values?.TryGetValue("area", out var area) == true)
            {
                return area?.ToString()?.Equals("Setup", StringComparison.OrdinalIgnoreCase) == true;
            }

            // Also check the page route value for Razor Pages in Setup area
            if (context.GetRouteData()?.Values?.TryGetValue("page", out var page) == true)
            {
                var pagePath = page?.ToString() ?? string.Empty;
                return pagePath.StartsWith("/Setup/", StringComparison.OrdinalIgnoreCase) ||
                       pagePath.StartsWith("/Areas/Setup/", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Checks if the application setup has been completed (called only once).
        /// </summary>
        /// <param name="dbContext">Database context.</param>
        /// <returns>True if setup is complete, false otherwise.</returns>
        private async Task<bool> CheckSetupCompletedAsync(ApplicationDbContext dbContext)
        {
            try
            {
                // Check if the Settings table exists and has the AllowSetup = false entry
                var setting = await dbContext.Settings
                    .FirstOrDefaultAsync(s => s.Group == "SYSTEM" && s.Name == "AllowSetup");

                var completed = setting != null && setting.Value.Equals("false", StringComparison.OrdinalIgnoreCase);

                logger.LogInformation("Setup completion check: {Status}", completed ? "Completed" : "Not completed");

                return completed;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Unable to check setup completion status - assuming setup needed");
                return false;
            }
        }

        /// <summary>
        /// Determines if the path is for a static resource.
        /// </summary>
        /// <param name="path">Request path.</param>
        /// <returns>True if static resource, false otherwise.</returns>
        private static bool IsStaticResource(string path)
        {
            var staticPaths = new[] { "/lib", "/css", "/js", "/images", "/fonts", "/pub" };
            
            if (staticPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Check for file extensions (e.g., .js, .css, .png, etc.)
            var extension = System.IO.Path.GetExtension(path);
            if (!string.IsNullOrEmpty(extension))
            {
                var staticExtensions = new[] { ".js", ".css", ".png", ".jpg", ".jpeg", ".gif", ".svg", ".woff", ".woff2", ".ttf", ".eot", ".ico", ".map" };
                return staticExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}