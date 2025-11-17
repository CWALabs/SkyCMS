// <copyright file="ArticleLogJsonModel.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Article log json model.
    /// </summary>
    public class ArticleLogJsonModel
    {
        /// <summary>
        /// Gets or sets id.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets activity notes and description.
        /// </summary>
        public string ActivityNotes { get; set; }

        /// <summary>
        ///     Gets or sets date and Time (UTC by default).
        /// </summary>
        public DateTimeOffset DateTimeStamp { get; set; }

        /// <summary>
        /// Gets or sets identity User Id.
        /// </summary>
        public string IdentityUserId { get; set; }
    }
}