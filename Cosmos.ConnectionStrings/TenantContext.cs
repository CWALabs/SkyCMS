// <copyright file="TenantContext.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.DynamicConfig
{
    /// <summary>
    /// Provides ambient tenant context for operations outside of HTTP request scope.
    /// </summary>
    /// <remarks>
    /// This class uses AsyncLocal to maintain tenant context across async operations
    /// while ensuring isolation between different execution contexts.
    /// Use this for background jobs, Hangfire tasks, or any operation that needs
    /// tenant context without an active HttpContext.
    /// </remarks>
    public static class TenantContext
    {
        private static readonly AsyncLocal<string?> _currentDomain = new AsyncLocal<string?>();

        /// <summary>
        /// Gets or sets the current tenant domain for the ambient context.
        /// </summary>
        /// <remarks>
        /// This value is maintained per async execution context and will not leak
        /// between different async operations or threads.
        /// </remarks>
        public static string? CurrentDomain
        {
            get => _currentDomain.Value;
            set => _currentDomain.Value = value?.ToLowerInvariant();
        }

        /// <summary>
        /// Checks if a tenant context is currently set.
        /// </summary>
        public static bool HasContext => !string.IsNullOrWhiteSpace(_currentDomain.Value);

        /// <summary>
        /// Clears the current tenant context.
        /// </summary>
        public static void Clear()
        {
            _currentDomain.Value = null;
        }

        /// <summary>
        /// Executes an action within a tenant context.
        /// </summary>
        /// <param name="domain">Tenant domain name.</param>
        /// <param name="action">Action to execute.</param>
        public static void Execute(string domain, Action action)
        {
            var previousDomain = CurrentDomain;
            try
            {
                CurrentDomain = domain;
                action();
            }
            finally
            {
                CurrentDomain = previousDomain;
            }
        }

        /// <summary>
        /// Executes an async function within a tenant context.
        /// </summary>
        /// <param name="domain">Tenant domain name.</param>
        /// <param name="func">Async function to execute.</param>
        public static async Task ExecuteAsync(string domain, Func<Task> func)
        {
            var previousDomain = CurrentDomain;
            try
            {
                CurrentDomain = domain;
                await func();
            }
            finally
            {
                CurrentDomain = previousDomain;
            }
        }

        /// <summary>
        /// Executes an async function with a result within a tenant context.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="domain">Tenant domain name.</param>
        /// <param name="func">Async function to execute.</param>
        /// <returns>Result of the function.</returns>
        public static async Task<T> ExecuteAsync<T>(string domain, Func<Task<T>> func)
        {
            var previousDomain = CurrentDomain;
            try
            {
                CurrentDomain = domain;
                return await func();
            }
            finally
            {
                CurrentDomain = previousDomain;
            }
        }

        private static string? GetCurrentTenantDomain()
        {
            return null;
        }
    }
}