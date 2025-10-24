// <copyright file="ITitleChangeService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Titles
{
    using System.Threading.Tasks;
    using Cosmos.Common.Data;

    /// <summary>
    /// Handles side effects that must occur when an article title changes (e.g., slug updates, redirects, events).
    /// </summary>
    public interface ITitleChangeService
    {
        /// <summary>
        /// Processes a title change for the supplied <paramref name="article"/>, given the previous title.
        /// </summary>
        /// <param name="article">The article entity whose title has just been modified (unsaved or saved, per calling convention).</param>
        /// <param name="oldTitle">The prior title value used to detect changes and generate redirects if required.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task HandleTitleChangeAsync(Article article, string oldTitle);

        /// <summary>
        /// Validates whether a proposed title is usable (not reserved and not used by a different article).
        /// </summary>
        /// <param name="title">Proposed title.</param>
        /// <param name="articleNumber">Current article number (null when creating new).</param>
        /// <returns>True if available; false if conflict.</returns>
        Task<bool> ValidateTitle(string title, int? articleNumber);
    }
}