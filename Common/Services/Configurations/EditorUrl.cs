// <copyright file="EditorUrl.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Cms.Common.Services.Configurations
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Editor URL information.
    /// </summary>
    public class EditorUrl
    {
        /// <summary>
        ///     Gets or sets cloud provider.
        /// </summary>
        [Required]
        [UIHint("CloudProvider")]
        [Display(Name = "Cloud")]
        public string CloudName { get; set; }

        /// <summary>
        ///     Gets or sets editor Url.
        /// </summary>
        [Required]
        [Url]
        [Display(Name = "Url")]
        [RegularExpression(@"^(https://)", ErrorMessage = "Must start with https://")]
        public string Url { get; set; }
    }
}