// <copyright file="Setting.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Setting.
    /// </summary>
    public class Setting
    {
        /// <summary>
        /// Gets or sets setting ID.
        /// </summary>
        [Key]
        [Display(Name = "Id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets setting group.
        /// </summary>
        [Required]
        [MinLength(1)]
        [Display(Name = "Group")]
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets setting Name.
        /// </summary>
        [Required]
        [MinLength(1)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets setting value.
        /// </summary>
        [Required]
        [MinLength(1)]
        [Display(Name = "Value")]
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether setting value is required.
        /// </summary>
        [Required]
        [Display(Name = "Is Required")]
        public bool IsRequired { get; set; } = true;

        /// <summary>
        /// Gets or sets description of setting.
        /// </summary>
        [Required]
        [MinLength(0)]
        [Display(Name = "Description")]
        public string Description { get; set; } = " - ";
    }
}
