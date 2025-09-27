// <copyright file="CdnProviderEnum.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
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
        /// Sucuri firewall/CDN.
        /// </summary>
        Sucuri,

        /// <summary>
        /// No CDN provider.
        /// </summary>
        None
    }
}
