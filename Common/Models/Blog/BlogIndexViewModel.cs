// <copyright file="BlogIndexViewModel.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Models.Blog
{
    using System.Collections.Generic;

    /// <summary>
    /// View model representing a paged set of blog posts for index/list screens.
    /// </summary>
    public class BlogIndexViewModel
    {
        /// <summary>
        /// Gets or sets the collection of blog posts for the current page.
        /// </summary>
        public IEnumerable<BlogListItem> Posts { get; set; } = new List<BlogListItem>();

        /// <summary>
        /// Gets or sets the current (1-based) page number.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the number of posts displayed per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages available for the current filter.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets or sets the category filter applied to the listing (empty if no filter).
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }
}
