// <copyright file="EditorField.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    /// <summary>
    /// Editor field metadata.
    /// </summary>
    public class EditorField
    {
        /// <summary>
        /// Gets or sets field ID.
        /// </summary>
        public string FieldId { get; set; }

        /// <summary>
        /// Gets or sets field Name.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets editor mode.
        /// </summary>
        public EditorMode EditorMode { get; set; }

        /// <summary>
        /// Gets or sets icon URL.
        /// </summary>
        public string IconUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets tool tip content.
        /// </summary>
        public string ToolTip { get; set; } = string.Empty;
    }

    /// <summary>
    /// Monaco Editor Mode.
    /// </summary>
    public enum EditorMode
    {
        /// <summary>
        /// JavaScript mode
        /// </summary>
        JavaScript = 0,

        /// <summary>
        /// HTML Mode
        /// </summary>
        Html = 1,

        /// <summary>
        /// CSS Mode
        /// </summary>
        Css = 2,

        /// <summary>
        /// XML Mode
        /// </summary>
        Xml = 3,

        /// <summary>
        /// JSON
        /// </summary>
        Json = 4
    }
}
