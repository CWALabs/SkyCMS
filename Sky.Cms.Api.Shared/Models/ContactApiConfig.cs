// <copyright file="ContactApiConfig.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Api.Shared.Models;

using System.Text.Json;

/// <summary>
/// Configuration for the Contact API.
/// </summary>
public class ContactApiConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContactApiConfig"/> class.
    /// Default constructor with default values.
    /// </summary>
    public ContactApiConfig()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactApiConfig"/> class from JSON string.
    /// This constructor parses CAPTCHA settings from the database Settings table.
    /// Expected JSON format:
    /// {
    ///   "Provider": "turnstile" | "recaptcha",
    ///   "SiteKey": "your-site-key",
    ///   "SecretKey": "your-secret-key",
    ///   "RequireCaptcha": true
    /// }.
    /// </summary>
    /// <param name="jsonSettings">JSON string containing CAPTCHA configuration.</param>
    public ContactApiConfig(string jsonSettings)
    {
        if (string.IsNullOrWhiteSpace(jsonSettings))
        {
            RequireCaptcha = false;
            return;
        }

        try
        {
            var settings = JsonSerializer.Deserialize<CaptchaSettings>(jsonSettings, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (settings != null)
            {
                CaptchaProvider = settings.Provider;
                CaptchaSiteKey = settings.SiteKey;
                CaptchaSecretKey = settings.SecretKey;
                RequireCaptcha = settings.RequireCaptcha && !string.IsNullOrEmpty(settings.Provider);
            }
        }
        catch (JsonException)
        {
            // Invalid JSON - disable CAPTCHA
            RequireCaptcha = false;
        }
    }

    /// <summary>
    /// Gets or sets the administrator email address for contact form submissions.
    /// </summary>
    public string AdminEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the maximum message length allowed.
    /// </summary>
    public int MaxMessageLength { get; set; } = 5000;

    /// <summary>
    /// Gets or sets a value indicating whether CAPTCHA validation is required.
    /// </summary>
    public bool RequireCaptcha { get; set; }

    /// <summary>
    /// Gets or sets the CAPTCHA provider (e.g., "recaptcha", "turnstile").
    /// </summary>
    public string? CaptchaProvider { get; set; }

    /// <summary>
    /// Gets or sets the CAPTCHA site key (public key).
    /// For reCAPTCHA: Your reCAPTCHA site key.
    /// For Turnstile: Your Turnstile site key.
    /// </summary>
    public string? CaptchaSiteKey { get; set; }

    /// <summary>
    /// Gets or sets the CAPTCHA secret key (private key).
    /// For reCAPTCHA: Your reCAPTCHA secret key.
    /// For Turnstile: Your Turnstile secret key.
    /// </summary>
    public string? CaptchaSecretKey { get; set; }

    /// <summary>
    /// Creates a ContactApiConfig from database settings.
    /// </summary>
    /// <param name="adminEmail">Admin email address.</param>
    /// <param name="maxMessageLength">Maximum message length.</param>
    /// <param name="captchaJson">CAPTCHA settings as JSON string from Settings table.</param>
    /// <returns>Configured ContactApiConfig instance.</returns>
    public static ContactApiConfig FromDatabaseSettings(string adminEmail, int maxMessageLength, string? captchaJson)
    {
        var config = string.IsNullOrWhiteSpace(captchaJson)
            ? new ContactApiConfig()
            : new ContactApiConfig(captchaJson);

        config.AdminEmail = adminEmail;
        config.MaxMessageLength = maxMessageLength;

        return config;
    }

    /// <summary>
    /// Private class for deserializing CAPTCHA settings from JSON.
    /// </summary>
    private class CaptchaSettings
    {
        public string? Provider { get; set; }

        public string? SiteKey { get; set; }

        public string? SecretKey { get; set; }

        public bool RequireCaptcha { get; set; }
    }
}