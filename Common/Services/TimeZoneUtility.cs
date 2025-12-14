// <copyright file="TimeZoneUtility.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Cms.Common.Services
{
    using System;

    /// <summary>
    ///     Time zone conversion utility.
    /// </summary>
    public static class TimeZoneUtility
    {
        /// <summary>
        ///     Converts a UTC date to Pacific Standard Time.
        /// </summary>
        /// <param name="utcDateTime">Date time to convert.</param>
        /// <returns>Converted <see cref="DateTime"/>.</returns>
        public static DateTime ConvertUtcDateTimeToPst(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            return TimeZoneInfo.ConvertTime(utcDateTime, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
        }

        /// <summary>
        ///     Converts a PST date to UTC date.
        /// </summary>
        /// <param name="dateTime">Date/time to convert.</param>
        /// <returns>Converted date/time.</returns>
        public static DateTime ConvertPstDateTimeToUtc(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified),
                TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
        }
    }
}
