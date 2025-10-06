// <copyright file="BlogViewModels.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Models.Blogs
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// View model for creating / editing / listing blog streams.
    /// Mirrors <see cref="Cosmos.Common.Data.Blog"/> while omitting audit fields.
    /// </summary>
    public class BlogStreamViewModel
    {
        public Guid Id { get; set; }

        [Required, MaxLength(64)]
        [RegularExpression("^[a-z0-9-_]+$", ErrorMessage = "Lowercase letters, numbers, dash, underscore only.")]
        [Display(Name = "Blog Key")]
        public string BlogKey { get; set; } = Guid.NewGuid().ToString();

        [Required, MaxLength(128)]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [MaxLength(512)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Hero Image (URL / Path)")]
        public string HeroImage { get; set; }

        [Display(Name = "Default Stream")]
        public bool IsDefault { get; set; }

        [Display(Name = "Sort Order")]
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Lightweight projection of a blog entry for list/table display.
    /// </summary>
    public class BlogEntryListItem
    {
        public string BlogKey { get; set; }
        public int ArticleNumber { get; set; }
        public string Title { get; set; }
        public DateTimeOffset? Published { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string UrlPath { get; set; }
        public string Introduction { get; set; }
        public string BannerImage { get; set; }
    }

    /// <summary>
    /// Container view model for entries within a blog stream.
    /// </summary>
    public class BlogEntriesListViewModel
    {
        public string BlogKey { get; set; }
        public string BlogTitle { get; set; }
        public string BlogDescription { get; set; }
        public string HeroImage { get; set; }
        public List<BlogEntryListItem> Entries { get; set; } = new();
    }

    /// <summary>
    /// Form model for create/edit of a blog entry.
    /// </summary>
    public class BlogEntryEditViewModel
    {
        public Guid? Id { get; set; }
        public int? ArticleNumber { get; set; }
        public string BlogKey { get; set; }

        [Required, MaxLength(254)]
        public string Title { get; set; }

        [MaxLength(512)]
        [Display(Name = "Introduction (teaser)")]
        public string Introduction { get; set; }

        [Display(Name = "Content (HTML)")]
        public string Content { get; set; }

        [Display(Name = "Banner Image (URL / Path)")]
        public string BannerImage { get; set; }

        [Display(Name = "Publish Now?")]
        public bool PublishNow { get; set; }
    }
}