// <copyright file="CdnServiceFactory.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.CDN
{
    using Cosmos.Common.Data;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Factory for creating CDN service instances.
    /// </summary>
    public class CdnServiceFactory : ICdnServiceFactory
    {
        /// <inheritdoc/>
        public CdnService CreateCdnService(ApplicationDbContext dbContext, ILogger logger, HttpContext httpContext)
        {
            return CdnService.GetCdnService(dbContext, logger, httpContext);
        }
    }
}