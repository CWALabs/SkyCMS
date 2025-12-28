// <copyright file="IMultiTenantSetupService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

using System;
using System.Threading.Tasks;

namespace Sky.Editor.Services.Setup
{
    /// <summary>
    /// Multi-tenant setup service for post-provisioning tenant configuration.
    /// Handles tenant-specific setup tasks after the tenant has been provisioned by the admin application.
    /// </summary>
    public interface IMultiTenantSetupService
    {
        /// <summary>
        /// Checks if the current tenant requires post-provisioning setup.
        /// </summary>
        /// <returns>True if setup is required, false otherwise.</returns>
        Task<bool> TenantRequiresSetupAsync();

        /// <summary>
        /// Gets the current tenant's setup status and configuration.
        /// </summary>
        /// <returns>Tenant setup status information.</returns>
        Task<TenantSetupStatus> GetTenantSetupStatusAsync();

        /// <summary>
        /// Creates the first administrator account for the tenant.
        /// </summary>
        /// <param name="email">Administrator email address.</param>
        /// <param name="password">Administrator password.</param>
        /// <returns>Result indicating success or failure.</returns>
        Task<SetupCompletionResult> CreateTenantAdminAsync(string email, string password);

        /// <summary>
        /// Imports a community layout for the tenant.
        /// </summary>
        /// <param name="layoutId">Community layout ID.</param>
        /// <returns>Result indicating success or failure.</returns>
        Task<SetupCompletionResult> ImportLayoutAsync(string layoutId);

        /// <summary>
        /// Creates the default home page for the tenant.
        /// </summary>
        /// <param name="title">Home page title.</param>
        /// <param name="templateId">Optional template ID to use.</param>
        /// <returns>Result indicating success or failure.</returns>
        Task<SetupCompletionResult> CreateHomePageAsync(string title, Guid? templateId = null);

        /// <summary>
        /// Completes the tenant setup process.
        /// </summary>
        /// <returns>Result indicating success or failure.</returns>
        Task<SetupCompletionResult> CompleteTenantSetupAsync();
    }

    /// <summary>
    /// Tenant setup status information.
    /// </summary>
    public class TenantSetupStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether setup is required.
        /// </summary>
        public bool SetupRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an admin account exists.
        /// </summary>
        public bool HasAdminAccount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a layout exists.
        /// </summary>
        public bool HasLayout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a home page exists.
        /// </summary>
        public bool HasHomePage { get; set; }

        /// <summary>
        /// Gets or sets the tenant website URL from the Connection configuration.
        /// </summary>
        public string WebsiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tenant owner email from the Connection configuration.
        /// </summary>
        public string OwnerEmail { get; set; } = string.Empty;
    }
}
