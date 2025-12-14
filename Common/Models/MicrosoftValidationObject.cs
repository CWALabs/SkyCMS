// <copyright file="MicrosoftValidationObject.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Microsoft Validation Object.
    /// </summary>
    public class MicrosoftValidationObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftValidationObject"/> class.
        /// </summary>
        public MicrosoftValidationObject()
        {
            associatedApplications = new List<AssociatedApplication>();
        }

        /// <summary>
        /// Gets or sets list of applications.
        /// </summary>
        public List<AssociatedApplication> associatedApplications { get; set; }
    }
}
