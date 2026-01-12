// <copyright file="ValidateCaptchaQuery.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Api.Shared.Features.ContactForm.ValidateCaptcha;

using Cosmos.Common.Features.Shared;

/// <summary>
/// Query for validating CAPTCHA tokens.
/// </summary>
public class ValidateCaptchaQuery : IQuery<bool>
{
    /// <summary>
    /// Gets or sets the CAPTCHA token to validate.
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// Gets or sets the remote IP address of the requester.
    /// </summary>
    public string? RemoteIpAddress { get; set; }

    /// <summary>
    /// Gets or sets the Captcha Provider.
    /// </summary>
    public string? CaptchaProvider { get; set; }

    /// <summary>
    /// Gets or sets the secrets key.
    /// </summary>
    public string? SecretKey { get; set; }
}