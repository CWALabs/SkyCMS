// <copyright file="IEmailConfigurationService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// </copyright>

using System.Threading.Tasks;

namespace Sky.Editor.Services.Email
{
    /// <summary>
    /// Service interface for retrieving email configuration.
    /// </summary>
    public interface IEmailConfigurationService
    {
        /// <summary>
        /// Gets email settings from environment variables or database.
        /// </summary>
        /// <returns>Email settings.</returns>
        Task<EmailSettings> GetEmailSettingsAsync();
    }
}