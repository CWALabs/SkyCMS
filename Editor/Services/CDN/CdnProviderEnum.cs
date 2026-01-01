// <copyright file="CdnProviderEnum.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.CDN
{
    /// <summary>
    ///  Indicates the CDN provider.
    /// </summary>
    public enum CdnProviderEnum
    {
        /// <summary>
        /// Microsoft Azure CDN.
        /// </summary>
        AzureFrontdoor,

        /// <summary>
        /// Microsoft Azure CDN.
        /// </summary>
        AzureCDN,

        /// <summary>
        /// Edgio CDN.
        /// </summary>
        Cloudflare,

        /// <summary>
        /// Amazon CloudFront CDN.
        /// </summary>
        CloudFront,

        /// <summary>
        /// Sucuri firewall/CDN.
        /// </summary>
        Sucuri,

        /// <summary>
        /// No CDN provider.
        /// </summary>
        None,

        /// <summary>
        /// Fastly CDN.
        /// </summary>
        Fastly
    }
}
