// <copyright file="RoleItemViewModel.cs" company="Moonrise Software, LLC">
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
    /// Role item view model.
    /// </summary>
    [Serializable]
    public class RoleItemViewModel
    {
        /// <summary>
        ///     Gets or sets role ID.
        /// </summary>
        [Key]
        [Display(Name = "Role ID")]
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets friendly role name.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Role Name")]
        public string RoleName { get; set; }

        /// <summary>
        ///     Gets or sets role used to search on.
        /// </summary>
        [Display(Name = "Role Normalized Name")]
        public string RoleNormalizedName { get; set; }
    }
}
