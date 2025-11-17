// <copyright file="EmailMessageViewModel.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Models
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Email message view model.
    /// </summary>
    public class EmailMessageViewModel
    {
        /// <summary>
        /// Gets or sets sender name.
        /// </summary>
        [Display(Name = "Your name:")]
        [Required(AllowEmptyStrings = false)]
        public string SenderName { get; set; }

        /// <summary>
        /// Gets or sets email address.
        /// </summary>
        [EmailAddress]
        [MaxLength(156)]
        [Required(AllowEmptyStrings = false)]
        [Display(Name = "Your email address (required/will not be shared):")]
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets email subject.
        /// </summary>
        [MaxLength(256)]
        [Display(Name = "Subject (optional):")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets email content.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        [MaxLength(2048)]
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets send success.
        /// </summary>
        public bool? SendSuccess { get; set; }
    }
}
