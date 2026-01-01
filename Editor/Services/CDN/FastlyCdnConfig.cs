// <copyright file="FastlyCdnConfig.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.CDN
{
    /// <summary>
    /// Configuration settings for Fastly CDN.
    /// </summary>
    public class FastlyCdnConfig
    {
        /// <summary>
        /// Gets or sets the Fastly Service ID.
        /// </summary>
        /// <remarks>
        /// Found in the Fastly dashboard under Service settings.
        /// Format: alphanumeric string (e.g., "5QoWSdPgkxvL9eJHXP4vV1").
        /// </remarks>
        public string ServiceId { get; set; }

        /// <summary>
        /// Gets or sets the Fastly API token.
        /// </summary>
        /// <remarks>
        /// Generate an API token with "purge_all" and "purge_select" scopes in your Fastly account.
        /// Keep this secure and never commit to source control.
        /// </remarks>
        public string ApiToken { get; set; }

        /// <summary>
        /// Gets or sets the domain name for URL purging.
        /// </summary>
        /// <remarks>
        /// The domain configured in your Fastly service (e.g., "www.example.com").
        /// Used when purging individual URLs via PURGE method.
        /// </remarks>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use soft purge.
        /// </summary>
        /// <remarks>
        /// When true, marks content as stale rather than deleting it.
        /// Allows serving stale content while fetching fresh content from origin.
        /// Default is false (hard purge).
        /// </remarks>
        public bool SoftPurge { get; set; } = false;
    }
}