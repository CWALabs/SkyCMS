// <copyright file="IDomainDispatcher.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Scheduling
{
    using System;

    /// <summary>
    /// Provides domain context for multi-tenant operations in background jobs.
    /// </summary>
    public interface IDomainDispatcher
    {
        /// <summary>
        /// Gets the current domain name for the scoped operation.
        /// </summary>
        string CurrentDomain { get; }

        /// <summary>
        /// Creates a scope with the specified domain context.
        /// </summary>
        /// <param name="domainName">The domain name to set as current.</param>
        /// <returns>A disposable scope that resets the domain when disposed.</returns>
        IDisposable SetDomainScope(string domainName);
    }
}
