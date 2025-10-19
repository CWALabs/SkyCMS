// <copyright file="IPublishedArtifactService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Publishing;

using Cosmos.Common.Data;
using Sky.Editor.Services.CDN;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// <para>
/// Provides operations for publishing the article to the database and static web pages,
/// redirecting, and removing article-based static artifacts (e.g., generated HTML/pages)
/// and coordinating related CDN actions.</para>
/// <para>DOES NOT handle catalog updates or table of contents updates.</para>
/// </summary>
public interface IPublishingService
{
    /// <summary>
    ///  Create static pages for published pages.
    /// </summary>
    /// <param name="ids">IDs of pages to publish.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CreateStaticPages(IEnumerable<Guid> ids);

    /// <summary>
    /// Publishes the supplied article as static/site content and performs any required
    /// CDN cache purge or refresh operations.
    /// </summary>
    /// <param name="article">The article to publish. Must contain a valid <c>UrlPath</c> and content.</param>
    /// <returns>
    /// A task producing a list of CDN operation results (one per provider or operation),
    /// which may be empty if no CDN interaction was necessary.
    /// </returns>
    Task<List<CdnResult>> PublishAsync(Article article);

    /// <summary>
    /// Unpublishes (withdraws) the article identified by the supplied <paramref name="articleNumber"/>.
    /// </summary>
    /// <param name="article">The article to unpublish.</param>
    /// <returns>A task that completes when the unpublish operation has been persisted.</returns>
    /// <remarks>
    /// Typical responsibilities:
    /// - Locate the current published (or scheduled) version for the given article number.
    /// - Clear or nullify publication metadata (e.g., <see cref="Article.Published"/>).
    /// - Update status codes to represent an unpublished / draft state.
    /// - Optionally record audit events.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the article cannot be unpublished due to business rules.</exception>
    Task UnpublishAsync(Article article);

    /// <summary>
    /// Write the table of contents to static storage.
    /// </summary>
    /// <param name="prefix">Prefix.</param>
    /// <returns>A task that completes when the operation has finished.</returns>
    Task WriteTocAsync(string prefix = "/");
}