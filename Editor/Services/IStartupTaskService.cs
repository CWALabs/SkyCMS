// <copyright file="IStartupTaskService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Editor.Services
{
    using System.Threading.Tasks;

    /// <summary>
    /// Service interface for running startup tasks asynchronously.
    /// </summary>
    internal interface IStartupTaskService
    {
        /// <summary>
        /// Runs the startup tasks asynchronously.
        /// </summary>
        /// <returns>Task.</returns>
        Task RunAsync();
    }
}
