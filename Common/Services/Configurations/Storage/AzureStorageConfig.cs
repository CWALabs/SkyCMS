// <copyright file="AzureStorageConfig.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Cms.Common.Services.Configurations.Storage
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Azure storage config.
    /// </summary>
    public class AzureStorageConfig
    {
        /// <summary>
        ///     Gets or sets connection string.
        /// </summary>
        [Display(Name = "Conn. String")]
        public string AzureBlobStorageConnectionString { get; set; }

        /// <summary>
        ///     Gets or sets container name.
        /// </summary>
        [Display(Name = "Container")]
        public string AzureBlobStorageContainerName { get; set; } = "$web";

        /// <summary>
        ///     Gets or sets storage end point.
        /// </summary>
        [Display(Name = "Website URL")]
        public string AzureBlobStorageEndPoint { get; set; } = "/";
    }
}