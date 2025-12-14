// <copyright file="DomainDispatcher.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Scheduling
{
    using System;
    using System.Threading;

    /// <inheritdoc/>
    public class DomainDispatcher : IDomainDispatcher
    {
        private static readonly AsyncLocal<string> CurrentDomainContext = new AsyncLocal<string>();

        /// <inheritdoc/>
        public string CurrentDomain => CurrentDomainContext.Value;

        /// <inheritdoc/>
        public IDisposable SetDomainScope(string domainName)
        {
            var previousDomain = CurrentDomainContext.Value;
            CurrentDomainContext.Value = domainName;
            return new DomainScope(previousDomain);
        }

        private class DomainScope : IDisposable
        {
            private readonly string previousDomain;

            public DomainScope(string previousDomain)
            {
                this.previousDomain = previousDomain;
            }

            public void Dispose()
            {
                CurrentDomainContext.Value = previousDomain;
            }
        }
    }
}
