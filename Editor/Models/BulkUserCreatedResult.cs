// <copyright file="BulkUserCreatedResult.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    using Cosmos.EmailServices;
    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// Returns the result of creating a user with the bulk-create method.
    /// </summary>
    public class BulkUserCreatedResult
    {
        /// <inheritdoc/>
        public IdentityResult IdentityResult { get; set; }

        /// <inheritdoc/>
        public SendResult SendResult { get; set; }

        /// <inheritdoc/>
        public UserCreateViewModel UserCreateViewModel { get; internal set; }
    }
}
