// <copyright file="IRedirectService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Redirects
{
    using System;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;

    /// <summary>
    /// Provides creation and update operations for redirect articles that map an old slug to a new destination.
    /// </summary>
    public interface IRedirectService
    {
        /// <summary>
        /// Creates or updates a redirect article so that requests for <paramref name="fromSlug"/> will point to <paramref name="toSlug"/>.
        /// </summary>
        /// <param name="fromSlug">Original slug (source). If normalized to root, the call is ignored.</param>
        /// <param name="toSlug">Destination slug (target).</param>
        /// <param name="userId">User initiating the redirect (for audit fields).</param>
        /// <returns>The created or updated redirect <see cref="Article"/>; may return <c>null</c> if source slug is root.</returns>
        Task<Article> CreateOrUpdateRedirectAsync(string fromSlug, string toSlug, Guid userId);
    }
}