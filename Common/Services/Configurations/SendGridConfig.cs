// <copyright file="SendGridConfig.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Cms.Common.Services.Configurations
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     SendGrid Authentication Options.
    /// </summary>
    public class SendGridConfig
    {
        /// <summary>
        ///     Gets or sets sendGrid key.
        /// </summary>
        [Required]
        [Display(Name = "SendGrid key")]
        public string SendGridKey { get; set; }

        /// <summary>
        ///     Gets or sets from Email address.
        /// </summary>
        [Required]
        [EmailAddress]
        [Display(Name = "From email address")]
        public string EmailFrom { get; set; }
    }
}