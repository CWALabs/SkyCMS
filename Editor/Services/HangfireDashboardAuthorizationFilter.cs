// <copyright file="HangfireDashboardAuthorizationFilter.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// </copyright>

namespace Sky.Editor.Services
{
    using Hangfire.Dashboard;

    /// <summary>
    /// Authorization filter for Hangfire Dashboard.
    /// </summary>
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// Authorizes access to the Hangfire dashboard.
        /// </summary>
        /// <param name="context">Dashboard context.</param>
        /// <returns>True if authorized.</returns>
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Only allow access if user is authenticated and in Admin role
            return httpContext.User.Identity.IsAuthenticated &&
                   httpContext.User.IsInRole("Administrators");
        }
    }
}