// <copyright file="PowerBiTokenRequest.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Power BI Token request model.
    /// </summary>
    public class PowerBiTokenRequest
    {
        /// <summary>
        /// Gets or sets the Power BI Workspace ID.
        /// </summary>
        public Guid PowerBiWorkspaceId { get; set; }

        /// <summary>
        /// Gets or sets the report ID.
        /// </summary>
        public Guid ReportId { get; set; }

        /// <summary>
        /// Gets or sets the dataset IDs.
        /// </summary>
        public IList<Guid> DatasetIds { get; set; }
    }
}
