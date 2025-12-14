// <copyright file="PreloadViewModel.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    /// <summary>
    /// Website preload options.
    /// </summary>
    public class PreloadViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether preload CDN.
        /// </summary>
        public bool PreloadCdn { get; set; } = true;

        /// <summary>
        /// Gets or sets redis objects created.
        /// </summary>
        public int? PageCount { get; set; }

        /// <summary>
        /// Gets or sets number of editors involved with preload operation.
        /// </summary>
        public int EditorCount { get; set; } = 0;
    }
}
