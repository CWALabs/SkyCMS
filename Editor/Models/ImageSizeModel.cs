// <copyright file="ImageSizeModel.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    /// <summary>
    ///     Image size mode used for thumbnail generator.
    /// </summary>
    public class ImageSizeModel
    {
        /// <summary>
        ///     Gets or sets width in pixels.
        /// </summary>
        public int Width { get; set; } = 80;

        /// <summary>
        ///     Gets or sets height in pixels.
        /// </summary>
        public int Height { get; set; } = 80;
    }
}
