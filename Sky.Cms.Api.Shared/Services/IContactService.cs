// <copyright file="IContactService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Api.Shared.Services;

using Sky.Cms.Api.Shared.Models;

/// <summary>
/// Service interface for handling contact form submissions.
/// </summary>
public interface IContactService
{
    /// <summary>
    /// Processes a contact form submission and sends it to the administrator.
    /// </summary>
    /// <param name="request">The contact form request.</param>
    /// <param name="remoteIpAddress">The IP address of the requester.</param>
    /// <returns>A <see cref="Task{ContactFormResponse}"/> representing the result of the operation.</returns>
    Task<ContactFormResponse> SubmitContactFormAsync(ContactFormRequest request, string remoteIpAddress);

    /// <summary>
    /// Validates a CAPTCHA token.
    /// </summary>
    /// <param name="token">The CAPTCHA token to validate.</param>
    /// <param name="remoteIpAddress">The IP address of the requester.</param>
    /// <returns>A <see cref="Task{bool}"/> indicating whether the CAPTCHA is valid.</returns>
    Task<bool> ValidateCaptchaAsync(string token, string remoteIpAddress);
}