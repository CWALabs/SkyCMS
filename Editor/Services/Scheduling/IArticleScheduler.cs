// <copyright file="ArticleVersionPublisher.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>
namespace Sky.Editor.Services.Scheduling
{
    using System.Threading.Tasks;

    /// <summary>
    /// Scheduled service that activates article versions with multiple published dates,
    /// ensuring only the most recent non-future version is actively published.
    /// </summary>
    public interface IArticleScheduler
    {
        /// <summary>
        /// Executes the scheduled job to process article versions with multiple published dates.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync();
    }
}