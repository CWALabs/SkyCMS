// <copyright file="LayoutCatalogViewModel.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Layout catalog view model.
    /// </summary>
    public class LayoutCatalogViewModel
    {
        /// <summary>
        /// Gets or sets layout ID.
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets layout name.
        /// </summary>
        [Display(Name = "Layout Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets layout description.
        /// </summary>
        [Display(Name = "Description/Notes")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets layout license.
        /// </summary>
        [Display(Name = "License")]
        public string License { get; set; }
    }
}
