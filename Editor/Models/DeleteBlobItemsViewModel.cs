// <copyright file="DeleteBlobItemsViewModel.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Delete blob items.
    /// </summary>
    public class DeleteBlobItemsViewModel
    {
        /// <summary>
        /// Gets or sets parent path when delete was executed.
        /// </summary>
        public string ParentPath { get; set; }

        /// <summary>
        /// Gets or sets full paths of items to delete.
        /// </summary>
        public List<string> Paths { get; set; }
    }
}
