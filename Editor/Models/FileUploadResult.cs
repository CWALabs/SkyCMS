// <copyright file="FileUploadResult.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    /// <summary>
    /// File upload result.
    /// </summary>
    public class FileUploadResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether file is uploaded.
        /// </summary>
        public bool uploaded { get; set; }

        /// <summary>
        /// Gets or sets file upload unique ID.
        /// </summary>
        public string fileUid { get; set; }
    }
}