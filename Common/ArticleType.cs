// <copyright file="ArticleType.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Data
{
    /// <summary>
    /// Defines the classification (intended use and presentation semantics) for an Article.
    /// Extend this enumeration as new article kinds are introduced into the system.
    /// </summary>
    /// <remarks>
    /// This value can be used for:
    /// <list type="bullet">
    ///   <item>Applying layout or template selection logic.</item>
    ///   <item>Routing / URL generation decisions.</item>
    ///   <item>Filtering, querying, or analytics segmentation.</item>
    ///   <item>Conditional workflow (e.g., publishing rules per type).</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// Example switch usage:
    /// <code>
    /// string template = article.Type switch
    /// {
    ///     ArticleType.General =&gt; "GeneralArticle",
    ///     ArticleType.BlogPost =&gt; "BlogPost",
    ///     _ =&gt; "Default"
    /// };
    /// </code>
    /// </example>
    public enum ArticleType
    {
        /// <summary>
        /// A standard, general-purpose article (default). Use when no specialized behavior is required.
        /// </summary>
        General = 0,

        /// <summary>
        /// A blog post entry, typically time-ordered and possibly featured in feeds or archives.
        /// </summary>
        BlogPost = 1,

        /// <summary>
        /// Represents a blog stream content type.
        /// </summary>
        BlogStream = 2

        // Add future types here (e.g., LandingPage = 2, Redirect = 3, Documentation = 4, etc.)
    }
}