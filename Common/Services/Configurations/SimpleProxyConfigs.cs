// <copyright file="SimpleProxyConfigs.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Cms.Common.Services.Configurations
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Simple proxy service config.
    /// </summary>
    public class SimpleProxyConfigs
    {
        /// <summary>
        ///     Gets or sets array of configurations.
        /// </summary>
        [Display(Name = "Proxy configuration(s)")]
        public ProxyConfig[] Configs { get; set; }
    }
}