// <copyright file="TestResult.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Setup
{
    /// <summary>
    /// Test result model.
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the test was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the test message.
        /// </summary>
        public string Message { get; set; }
    }
}