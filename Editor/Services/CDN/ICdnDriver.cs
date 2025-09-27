// <copyright file="ICdnDriver.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.CDN
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// CDN Driver interface.
    /// </summary>
    public interface ICdnDriver
    {
        /// <summary>
        /// Gets the provider name.
        /// </summary>
        public string ProviderName { get; }

        /// <summary>
        /// Purges the specified list of URLs from the CDN.
        /// </summary>
        /// <param name="purgeUrls">List of URLs to purge.</param>
        /// <returns>CDN purge results.</returns>
        public Task<List<CdnResult>> PurgeCdn(List<string> purgeUrls);

        /// <summary>
        /// Purges the entire CDN for the current endpoint.
        /// </summary>
        /// <returns>CDN purge results.</returns>
        public Task<List<CdnResult>> PurgeCdn();
    }
}