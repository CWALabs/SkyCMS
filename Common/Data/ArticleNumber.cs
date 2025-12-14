// <copyright file="ArticleNumber.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Data
{
    using System;

    /// <summary>
    /// Tracks article numbers.
    /// </summary>
    public class ArticleNumber
    {
        /// <summary>
        /// Gets or sets record ID.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets date and time number set.
        /// </summary>
        public DateTimeOffset SetDateTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// Gets or sets last Article Number.
        /// </summary>
        public int LastNumber { get; set; }
    }
}
