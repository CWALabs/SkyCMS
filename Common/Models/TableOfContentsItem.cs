// <copyright file="TableOfContentsItem.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Table of Contents (TOC) Item.
    /// </summary>
    public class TableOfContentsItem
    {
        /// <summary>
        /// Gets or sets uRL Path to page.
        /// </summary>
        [MaxLength(1999)]
        public string UrlPath { get; set; }

        /// <summary>
        /// Gets or sets title of page.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets published date and time.
        /// </summary>
        public DateTimeOffset Published { get; set; }

        /// <summary>
        /// Gets or sets when last updated.
        /// </summary>
        public DateTimeOffset Updated { get; set; }

        /// <summary>
        /// Gets or sets banner or preview image.
        /// </summary>
        public string BannerImage { get; set; }

        /// <summary>
        /// Gets or sets author name.
        /// </summary>
        public string AuthorInfo { get; set; }

        /// <summary>
        /// Gets or sets page content.
        /// </summary>
        /// <remarks>Not stored in the database.</remarks>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the page introduction.
        /// </summary>
        public string Introduction { get; set; }
    }
}
