// <copyright file="ISlugService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Slugs
{
    /// <summary>
    /// Provides slug (URL segment) normalization utilities.
    /// </summary>
    public interface ISlugService
    {
        /// <summary>
        /// Normalizes an arbitrary string into a URL-friendly slug (lowercase, underscores for spaces).
        /// </summary>
        /// <param name="input">Raw input text (may be null).</param>
        /// <returns>Normalized slug (never null; empty if input was null or whitespace).</returns>
        string Normalize(string input);
    }

    /// <summary>
    /// Default implementation of <see cref="ISlugService"/> performing a simple lowercase + space replacement.
    /// </summary>
    public sealed class SlugService : ISlugService
    {
        /// <inheritdoc/>
        public string Normalize(string input) =>
            (input ?? string.Empty).Trim().Replace(" ", "_").ToLowerInvariant();
    }
}