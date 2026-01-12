// <copyright file="ICaptchaValidator.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Api.Shared.Services;

/// <summary>
/// Interface for CAPTCHA validation services.
/// </summary>
public interface ICaptchaValidator
{
    /// <summary>
    /// Validates a CAPTCHA token.
    /// </summary>
    /// <param name="token">The CAPTCHA response token.</param>
    /// <param name="remoteIpAddress">The IP address of the user.</param>
    /// <returns>A <see cref="Task{bool}"/> indicating whether the CAPTCHA is valid.</returns>
    Task<bool> ValidateAsync(string token, string? remoteIpAddress = null);
}