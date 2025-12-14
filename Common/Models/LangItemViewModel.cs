// <copyright file="LangItemViewModel.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Models
{
    /// <summary>
    ///     Language list item.
    /// </summary>
    public class LangItemViewModel
    {
        /// <summary>
        ///     Gets or sets iSO code for language.
        /// </summary>
        public string LanguageCode { get; set; }

        /// <summary>
        ///     Gets or sets friendly name of language.
        /// </summary>
        public string DisplayName { get; set; }
    }
}
