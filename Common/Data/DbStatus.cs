// <copyright file="DbStatus.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Data
{
    /// <summary>
    /// Enumeration representing the status of a database.
    /// </summary>
    public enum DbStatus
    {
        /// <summary>
        /// Database does not exist.
        /// </summary>
        DoesNotExist = 0,

        /// <summary>
        /// Database exists with one or more users.
        /// </summary>
        ExistsWithUsers = 1,

        /// <summary>
        /// Database exists with no users.
        /// </summary>
        ExistsWithNoUsers = 2,

        /// <summary>
        /// Database exists but is missing one or more containers.
        /// </summary>
        ExistsWithMissingContainers = 3,

        /// <summary>
        /// Database failed to be created.
        /// </summary>
        CreationFailed = 4,
    }
}
