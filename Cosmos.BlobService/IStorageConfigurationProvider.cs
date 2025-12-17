// <copyright file="IStorageConfigurationProvider.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.BlobService
{
    /// <summary>
    /// Provides storage connection string configuration.
    /// </summary>
    public interface IStorageConfigurationProvider
    {
        /// <summary>
        /// Gets the storage connection string from configuration or database.
        /// </summary>
        /// <returns>Storage connection string, or null if not configured.</returns>
        string GetStorageConnectionString();
    }
}