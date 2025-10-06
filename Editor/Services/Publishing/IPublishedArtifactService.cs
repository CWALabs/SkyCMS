// <copyright file="IPublishedArtifactService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Publishing;

using System.Collections.Generic;
using System.Threading.Tasks;
using Cosmos.Common.Data;
using Sky.Editor.Services.CDN;

/// <summary>
/// Provides operations for publishing, redirecting, and removing article-based
/// static artifacts (e.g., generated HTML/pages) and coordinating related CDN actions.
/// </summary>
/// <remarks>
/// Typical flow:
/// 1. Call <see cref="PublishAsync(Article)"/> to (re)publish an article and optionally
///    trigger CDN cache refresh/invalidation.
/// 2. Call <see cref="PublishRedirectAsync(Article)"/> when an article represents a redirect
///    (e.g., its <c>RedirectTarget</c> metadata is set) so that a redirect artifact is emitted.
/// 3. Call <see cref="DeleteStaticAsync(string)"/> to remove previously published static content
///    for a URL path (e.g., when unpublishing or deleting an article).
/// </remarks>
public interface IPublishedArtifactService
{
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
    /// Publishes a redirect artifact for the supplied article when it represents a redirect
    /// (e.g., its metadata indicates a <c>RedirectTarget</c>). Implementations should emit/overwrite
    /// the static redirect representation and handle any required CDN invalidation.
    /// </summary>
    /// <param name="redirectArticle">The logical redirect article descriptor.</param>
    Task PublishRedirectAsync(Article redirectArticle);

    /// <summary>
    /// Deletes previously published static artifacts (and optionally triggers CDN purge) for the given URL path.
    /// </summary>
    /// <param name="urlPath">The canonical URL path (without domain) whose static output should be removed.</param>
    Task DeleteStaticAsync(string urlPath);
}