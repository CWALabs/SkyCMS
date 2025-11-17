// <copyright file="EditImagePostViewModel.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    /// <summary>
    /// Filerobot image post model.
    /// </summary>
    public class FileRobotImagePost
    {
        /// <summary>
        /// Gets or sets file name without extension.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets file name with extension.
        /// </summary>
        public string fullName { get; set; }

        /// <summary>
        /// Gets or sets file extension.
        /// </summary>
        public string extension { get; set; }

        /// <summary>
        /// Gets or sets mime type.
        /// </summary>
        public string mimeType { get; set; }

        /// <summary>
        /// Gets or sets base 64 image data.
        /// </summary>
        public string imageBase64 { get; set; }

        /// <summary>
        /// Gets or sets quantity.
        /// </summary>
        public double? quantity { get; set; } = null;

        /// <summary>
        /// Gets or sets image width.
        /// </summary>
        public string width { get; set; }

        /// <summary>
        /// Gets or sets image height.
        /// </summary>
        public string height { get; set; }

        /// <summary>
        /// Gets or sets folder where image should reside.
        /// </summary>
        public string folder { get; set; }
    }
}
