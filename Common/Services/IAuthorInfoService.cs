// <copyright file="IAuthorInfoService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;

    /// <summary>
    /// Provides author profile lookup, creation, and update operations used when attributing content.
    /// Implementations should cache frequently accessed author metadata to reduce database load.
    /// </summary>
    public interface IAuthorInfoService
    {
        /// <summary>
        /// Gets existing author info for a user or creates a new record if one does not already exist.
        /// </summary>
        /// <param name="userId">The unique user identifier (typically an application user primary key).</param>
        /// <param name="displayNameFactory">
        /// Optional factory to produce a display name when creating a new author record. If null and the author
        /// does not exist, an implementation-specific default should be used.
        /// </param>
        /// <param name="cancellationToken">Token to observe while awaiting the task.</param>
        /// <returns>The existing or newly created author info.</returns>
        Task<AuthorInfo> GetOrCreateAsync(Guid userId, Func<string> displayNameFactory = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to retrieve author info without creating a new record on a miss.
        /// </summary>
        /// <param name="userId">The unique user identifier.</param>
        /// <param name="cancellationToken">Token to observe while awaiting the task.</param>
        /// <returns>The author info if found; otherwise <c>null</c>.</returns>
        Task<AuthorInfo> FindAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates basic author profile fields and persists changes.
        /// </summary>
        /// <param name="author">The author info entity to update.</param>
        /// <param name="cancellationToken">Token to observe while awaiting the task.</param>
        /// <returns>The updated author info.</returns>
        Task<AuthorInfo> UpdateAsync(AuthorInfo author, CancellationToken cancellationToken = default);
    }
}