// <copyright file="IPublishingService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Publishing
{
    using System;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;

    /// <summary>
    /// Provides operations for publishing and unpublishing <see cref="Article"/> instances.
    /// </summary>
    /// <remarks>
    /// Implementations encapsulate the domain logic required to:
    /// - Immediately publish an article or schedule it for future publication.
    /// - Persist / update publication state (e.g. <see cref="Article.Published"/>, status codes).
    /// - Revoke publication (unpublish) based on an article number.
    /// This interface is intentionally minimal; implementations may enforce additional validation
    /// (e.g., version checks, concurrency tokens, status transitions, audit logging, scheduling queues).
    /// </remarks>
    public interface IPublishingService
    {
        /// <summary>
        /// Publishes the specified <paramref name="article"/> either immediately or at a scheduled UTC time.
        /// </summary>
        /// <param name="article">The article instance to publish (must not be null and should have its identity populated).</param>
        /// <param name="when">
        /// The UTC instant when the article should be considered published. If <c>null</c>, the implementation
        /// should publish immediately (typically using <see cref="DateTimeOffset.UtcNow"/>).
        /// </param>
        /// <returns>A task that completes when the publication request has been persisted or scheduled.</returns>
        /// <remarks>
        /// Expected implementation responsibilities:
        /// - Validate the article can transition to a published state.
        /// - Set or schedule the <see cref="Article.Published"/> timestamp.
        /// - Adjust status codes / workflow fields as appropriate.
        /// - Optionally enqueue background work if future scheduling is required.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="article"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the article cannot be published due to its current state or validation failure.</exception>
        Task PublishAsync(Article article, DateTimeOffset? when);

        /// <summary>
        /// Unpublishes (withdraws) the article identified by the supplied <paramref name="articleNumber"/>.
        /// </summary>
        /// <param name="articleNumber">The logical article number (not the database primary key GUID).</param>
        /// <returns>A task that completes when the unpublish operation has been persisted.</returns>
        /// <remarks>
        /// Typical responsibilities:
        /// - Locate the current published (or scheduled) version for the given article number.
        /// - Clear or nullify publication metadata (e.g., <see cref="Article.Published"/>).
        /// - Update status codes to represent an unpublished / draft state.
        /// - Optionally record audit events.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the article cannot be unpublished due to business rules.</exception>
        Task UnpublishAsync(int articleNumber);
    }
}