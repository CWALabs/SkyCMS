// <copyright file="ApiArgument.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Models
{
    /// <summary>
    /// GET or POST argument.
    /// </summary>
    public class ApiArgument
    {
        /// <summary>
        /// Gets or sets argument key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets argment value.
        /// </summary>
        public string Value { get; set; }
    }
}
