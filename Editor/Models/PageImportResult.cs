// <copyright file="PageImportResult.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    /// <summary>
    /// Page import result.
    /// </summary>
    public class PageImportResult : FileUploadResult
    {
        /// <summary>
        /// Gets or sets errors.
        /// </summary>
        public string Errors { get; set; } = string.Empty;
    }
}