// <copyright file="GoogleStorageConfig.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.BlobService.Config
{
    /// <summary>
    ///     Google configuration.
    /// </summary>
    public class GoogleStorageConfig
    {
        /// <summary>
        ///     Gets or sets project Id.
        /// </summary>
        public string GoogleProjectId { get; set; }

        /// <summary>
        ///     Gets or sets jSON authorization path.
        /// </summary>
        public string GoogleJsonAuthPath { get; set; }

        /// <summary>
        ///     Gets or sets bucket name.
        /// </summary>
        public string GoogleBucketName { get; set; }
    }
}