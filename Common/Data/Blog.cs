// <copyright file="Blog.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Represents a distinct blog stream (multi-blog root) that groups related
    /// <see cref="Article"/> entities via a shared <see cref="BlogKey"/>.
    /// </summary>
    /// <remarks>
    /// Key concepts:
    /// - BlogKey: route-safe stable identifier (lowercase) used in lookups and filtering.
    /// - Title / Description: human friendly metadata (exposed in UI and SEO).
    /// - HeroImage: optional banner or cover image URL (relative or absolute).
    /// - IsDefault: marks the fallback stream for reassignment when deleting others.
    /// - CreatedUtc / UpdatedUtc: lifecycle timestamps (UTC).
    /// </remarks>
    [Index(nameof(BlogKey), IsUnique = true)]
    public class Blog
    {
        /// <summary>
        /// Primary key (GUID). Generated on instantiation.
        /// </summary>
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Stable identifier used for routing and association with articles
        /// (e.g. all articles where <c>Article.BlogKey == BlogKey</c> belong to this stream).
        /// Constraints: lowercase letters, digits, dash, underscore.
        /// </summary>
        [Required]
        [MaxLength(64)]
        [RegularExpression("^[a-z0-9-_]+$", ErrorMessage = "Lowercase letters, numbers, dash, underscore only.")]
        public string BlogKey { get; set; }

        /// <summary>
        /// Human readable title of the blog stream (e.g. "Engineering Updates").
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string Title { get; set; }

        /// <summary>
        /// Optional descriptive text (teaser or SEO meta source). Soft limit 512 chars.
        /// </summary>
        [MaxLength(512)]
        public string Description { get; set; }

        /// <summary>
        /// Optional hero/cover image path or URL (stored as-is, not validated here).
        /// </summary>
        public string HeroImage { get; set; } = string.Empty;

        /// <summary>
        /// Flag indicating this is the default (fallback) blog stream.
        /// Only one should typically be true. Enforced at application logic level.
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// UTC timestamp when the blog was created.
        /// </summary>
        public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// UTC timestamp of last metadata update. (Set in code when edited.)
        /// </summary>
        public DateTimeOffset? UpdatedUtc { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Optional sort order (ascending) if you want deterministic ordering in UI.
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }
}