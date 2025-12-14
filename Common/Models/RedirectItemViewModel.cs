// <copyright file="RedirectItemViewModel.cs" company="Moonrise Software, LLC">
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
    /// Redirect from and to URL item.
    /// </summary>
    public class RedirectItemViewModel
    {
        /// <summary>
        /// Gets or sets redirect ID.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets redirect from this URL (local to this web server).
        /// </summary>
        [RedirectUrl]
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Redirect from URL")]
        public string FromUrl { get; set; }

        /// <summary>
        /// Gets or sets redirect to this URL.
        /// </summary>
        [RedirectUrl]
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Redirect to URL")]
        public string ToUrl { get; set; }
    }
}
