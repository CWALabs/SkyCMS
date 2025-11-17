// <copyright file="StatusCodeEnum.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Data.Logic
{
    /// <summary>
    ///     Article status code.
    /// </summary>
    public enum StatusCodeEnum
    {
        /// <summary>
        ///     Active, able to display if publish date given.
        /// </summary>
        Active = 0,

        /// <summary>
        ///     In active, can be displayed by logged in users
        /// </summary>
        Inactive = 1,

        /// <summary>
        ///     Considered removed, no one can display until status changes.
        /// </summary>
        Deleted = 2,

        /// <summary>
        ///     The article is a redirect.
        /// </summary>
        Redirect = 3
    }
}