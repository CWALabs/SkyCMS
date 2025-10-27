// <copyright file="ISlugService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Slugs
{
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;

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

    /// <summary>
    /// URL-safe slug implementation for Razor Pages route segments.
    /// Produces only ASCII letters/digits and separator characters; removes diacritics.
    /// </summary>
    public sealed class SlugService : ISlugService
    {
        // Choose your separator to match your style/SEO: '-' (common) or '_' (your original).
        private const char Separator = '-';

        /// <inheritdoc cref="Sky.Editor.Services.Slugs.ISlugService.Normalize(string, string)"/>
        public string Normalize(string input, string blogKey = "")
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            // 1) Normalize and lowercase
            var s = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);

            // 2) Remove diacritics and map everything non [a-z0-9] to the separator
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (cat is UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.EnclosingMark)
                {
                    continue;
                }

                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9'))
                {
                    sb.Append(ch);
                }
                else
                {
                    sb.Append(Separator);
                }
            }

            // 3) Collapse duplicate separators and trim unsafe ends
            var collapsed = Regex.Replace(sb.ToString(), $"{Regex.Escape(Separator.ToString())}{{2,}}", Separator.ToString());
            var slug = collapsed.Trim(Separator, '/', '.', '_', '~');

            // 4) Guard against reserved dot-segments
            if (slug is "." or "..")
            {
                slug = slug.Replace('.', Separator);
            }

            if (!string.IsNullOrWhiteSpace(blogKey))
            {
                slug = $"{blogKey}/{slug}";
            }

            return slug;
        }
    }
}