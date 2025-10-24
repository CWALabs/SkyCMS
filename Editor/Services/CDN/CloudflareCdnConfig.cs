// <copyright file="CloudflareCdnConfig.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.CDN
{

    /// <summary>
    /// Cloudflare settings.
    /// </summary>
    public class CloudflareCdnConfig
    {
        /// <summary>
        /// Gets or sets the API token.
        /// </summary>
        public string ApiToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the zone ID.
        /// </summary>
        public string ZoneId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets validation trigger to ensure all or none of the AzureCDN properties are set.
        /// </summary>
        [AllOrNoneRequired("ApiToken", "ZoneId", ErrorMessage = "Cloudflare settings are not complete.")]
        public string ValidationTrigger { get; set; } = string.Empty; // dummy property to attach the attribute
    }
}
