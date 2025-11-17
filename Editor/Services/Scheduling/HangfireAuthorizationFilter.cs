// <copyright file="Class.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Scheduling
{
    using Hangfire.Dashboard;

    /// <summary>
    /// Initial implementation of Hangfire dashboard authorization filter.
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        /// <summary>
        /// Determines whether the current user is authorized based on their authentication status.
        /// </summary>
        /// <remarks>This method checks the authentication status of the user associated with the current
        /// HTTP context. Additional role or permission checks may be required depending on the application's
        /// requirements.</remarks>
        /// <param name="context">The <see cref="DashboardContext"/> containing the HTTP context for the current request.</param>
        /// <returns><see langword="true"/> if the user is authenticated; otherwise, <see langword="false"/>.</returns>
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity.IsAuthenticated && (httpContext.User.IsInRole("Administrators") || httpContext.User.IsInRole("Editors")); // Add role checks if needed
        }
    }
}
