// <copyright file="ISlugService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
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
        ///  Normalizes the input string into a URL-safe slug.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="blogKey">Blog key (if is a blog entry).</param>
        /// <returns>Normalized slug.</returns>
        string Normalize(string input, string blogKey = "");
    }
}
