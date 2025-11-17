// <copyright file="ArticlePermisionItem.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Models
{
    /// <summary>
    /// Article permission item.
    /// </summary>
    public class ArticlePermisionItem
    {
        /// <summary>
        /// Gets or sets role or user ID.
        /// </summary>
        public string IdentityObjectId { get; set; }

        /// <summary>
        /// Gets or sets role name or user email.
        /// </summary>
        public string Name { get; set; }
    }
}
