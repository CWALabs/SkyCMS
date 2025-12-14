// <copyright file="CdnSetting.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.CDN
{
    /// <summary>
    /// Represents a CDN (Content Delivery Network) setting with a provider and its associated value.
    /// </summary>
    public class CdnSetting
    {
        /// <summary>
        /// Gets or sets the CDN provider.
        /// </summary>
        public CdnProviderEnum CdnProvider { get; set; } = CdnProviderEnum.None;

        /// <summary>
        /// Gets or sets the value associated with the CDN provider, such as a URL or key.
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }
}
