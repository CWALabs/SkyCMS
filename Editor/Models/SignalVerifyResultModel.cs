// <copyright file="SignalVerifyResultModel.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    using System;

    /// <summary>
    ///     This is a verification message that an Editor can be reached.
    /// </summary>
    public class SignalVerifyResult
    {
        /// <summary>
        ///     Gets or sets data that was recieved.
        /// </summary>
        public string Echo { get; set; }

        /// <summary>
        ///     Gets or sets dateTime in UTC for echo.
        /// </summary>
        public DateTime Stamp { get; set; }
    }
}