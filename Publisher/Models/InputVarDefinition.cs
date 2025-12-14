// <copyright file="InputVarDefinition.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Cms.Publisher.Models
{
    /// <summary>
    /// Input variable definition.
    /// </summary>
    public class InputVarDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InputVarDefinition"/> class.
        /// </summary>
        /// <param name="definition">Variable definition to use.</param>
        /// <example>InputVarDefinition("firstName:string:64").</example>
        public InputVarDefinition(string definition)
        {
            var parts = definition.Split(':');
            Name = parts[0];
            if (parts.Length > 1)
            {
                MaxLength = int.Parse(parts[1]);
            }
            else
            {
                MaxLength = 256;
            }
        }

        /// <summary>
        /// Gets or sets input variable name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets maximum number of string characters.
        /// </summary>
        public int MaxLength { get; set; } = 256;
    }
}
