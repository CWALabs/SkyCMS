// <copyright file="NoOpCaptchaValidator.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Api.Shared.Services.Captcha;

/// <summary>
/// No-operation CAPTCHA validator used when CAPTCHA is disabled.
/// </summary>
public class NoOpCaptchaValidator : ICaptchaValidator
{
    /// <inheritdoc/>
    public Task<bool> ValidateAsync(string token, string? remoteIpAddress = null)
    {
        // Always return true when CAPTCHA is disabled
        return Task.FromResult(true);
    }
}