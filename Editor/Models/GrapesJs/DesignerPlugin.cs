// <copyright file="DesignerPlugin.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Models.GrapesJs
{
    /// <summary>
    /// Designer plugin definition.
    /// </summary>
    public class DesignerPlugin
    {
        /// <summary>
        /// Gets or sets the plugin name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the URL to the source file.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        public string Options { get; set; }
    }
}
