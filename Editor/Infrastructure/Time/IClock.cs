// <copyright file="IClock.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Infrastructure.Time
{
    using System;

    /// <summary>
    /// Abstraction over system time for deterministic and testable components.
    /// </summary>
    public interface IClock
    {
        /// <summary>
        /// Gets the current UTC timestamp.
        /// </summary>
        DateTimeOffset UtcNow { get; }
    }

    /// <summary>
    /// Default clock implementation returning <see cref="DateTimeOffset.UtcNow"/>.
    /// </summary>
    public sealed class SystemClock : IClock
    {
        /// <inheritdoc/>
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}