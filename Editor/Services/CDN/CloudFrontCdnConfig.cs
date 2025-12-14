// <copyright file="CloudFrontCdnConfig.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.CDN
{
    /// <summary>
    /// Amazon CloudFront CDN settings.
    /// </summary>
    public class CloudFrontCdnConfig
    {
        /// <summary>
        /// Gets or sets the AWS access key ID.
        /// </summary>
        public string AccessKeyId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the AWS secret access key.
        /// </summary>
        public string SecretAccessKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the CloudFront distribution ID.
        /// </summary>
        public string DistributionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the AWS region (e.g., us-east-1).
        /// </summary>
        public string Region { get; set; } = "us-east-1";
    }
}