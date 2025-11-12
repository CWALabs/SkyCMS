// <copyright file="IBlogRenderingService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.BlogPublishing
{
    using Cosmos.Common.Data;
    using System.Threading.Tasks;

    /// <summary>
    /// Blog rendering service interface.
    /// </summary>
    public interface IBlogRenderingService
    {
        /// <summary>
        /// Generates HTML for a blog stream page displaying the latest blog entries.
        /// </summary>
        /// <param name="article">The blog stream article containing the stream configuration (banner image, title, and introduction used as description).</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the rendered HTML string for the blog stream page.</returns>
        /// <remarks>
        /// This method:
        /// - Loads the "blog-stream" template from the database.
        /// - Populates the stream banner from <paramref name="article"/>'s <see cref="Article.BannerImage"/> (hidden if empty).
        /// - Sets the stream title from <paramref name="article"/>'s <see cref="Article.Title"/>.
        /// - Sets the stream description from <paramref name="article"/>'s <see cref="Article.Introduction"/>.
        /// - Queries up to 10 most recent published, non-deleted, non-redirected articles associated with the same <see cref="Article.BlogKey"/>.
        /// - Generates article preview cards including banner images and titles for each entry.
        /// - Falls back to the first paragraph of <see cref="Article.Content"/> if an entry's <see cref="Article.Introduction"/> is empty.
        /// </remarks>
        Task<string> GenerateBlogStreamHtml(Article article);

        /// <summary>
        /// Generates HTML for an individual blog entry (article) page.
        /// </summary>
        /// <param name="article">The article to render, including title, content, and optional banner image.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the rendered HTML string for the blog entry page.</returns>
        /// <remarks>
        /// This method:
        /// - Loads the "blog-post" template from the database.
        /// - Injects the article's <see cref="Article.BannerImage"/> (if provided) into the designated image container with class "ccms-blog-title-image".
        /// - Sets the article <see cref="Article.Title"/> in the page heading element with class "ccms-blog-item-title".
        /// - Populates the content area (element with class "ccms-blog-item-content") with the article's HTML <see cref="Article.Content"/>.
        /// </remarks>
        Task<string> GenerateBlogEntryHtml(Article article);
    }
}