// <copyright file="BlobMetadataItem.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.BlobService.Models
{
    /// <summary>
    ///  Blob metadata item.
    /// </summary>
    public class BlobMetadataItem
    {
        /// <summary>
        ///  Gets or sets the metadata item key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value represented by this property.
        /// </summary>
        public string Value { get; set; }
    }
}
