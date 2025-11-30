// <copyright file="HttpUrlAttribute.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.DynamicConfig.Validation
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Validates that a URL uses HTTP or HTTPS protocol only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class HttpUrlAttribute : ValidationAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpUrlAttribute"/> class.
        /// </summary>
        public HttpUrlAttribute()
            : base("The {0} field must be a valid HTTP or HTTPS URL.")
        {
        }

        /// <summary>
        /// Validates that the URL uses HTTP or HTTPS protocol.
        /// </summary>
        /// <param name="value">The URL value to validate.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>Validation result indicating success or failure.</returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                // Let [Required] attribute handle null/empty validation
                return ValidationResult.Success;
            }

            var urlString = value.ToString()!;

            // Try to parse as a URI
            if (!Uri.TryCreate(urlString, UriKind.Absolute, out var uri))
            {
                return new ValidationResult(
                    FormatErrorMessage(validationContext.DisplayName),
                    new[] { validationContext.MemberName ?? string.Empty });
            }

            // Check if scheme is HTTP or HTTPS
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                return new ValidationResult(
                    $"The {validationContext.DisplayName} field must use HTTP or HTTPS protocol. Protocol '{uri.Scheme}' is not allowed.",
                    new[] { validationContext.MemberName ?? string.Empty });
            }

            return ValidationResult.Success;
        }
    }
}