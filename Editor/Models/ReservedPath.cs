// <copyright file="ReservedPath.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Reserved path.
    /// </summary>
    /// <remarks>
    /// A reserved path prevents a page from being named that conflicts with a path.
    /// </remarks>
    public class ReservedPath
    {
        /// <summary>
        /// Gets or sets row ID.
        /// </summary>
        [Key]
        [Display(Name = "Id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets reserved Path.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Reserved Path")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is required by Cosmos.
        /// </summary>
        [Display(Name = "Required by Cosmos?")]
        public bool CosmosRequired { get; set; } = false;

        /// <summary>
        /// Gets or sets reason by protected.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
    }
}
