// <copyright file="CdnService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.CDN
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    /// <summary>
    ///     Configuration for Azure Front Door, Edgio or Microsoft CDN.
    /// </summary>
    public class CdnService : ICdnDriver
    {
        /// <summary>
        /// CDN group name constant.
        /// </summary>
        public static readonly string CDNGROUPNAME = "CDN";

        private readonly ILogger logger;
        private readonly HttpContext context;
        private readonly List<CdnSetting> settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CdnService"/> class.
        /// </summary>
        /// <param name="settings">CDN settings.</param>
        /// <param name="logger">Log service.</param>
        /// <param name="context">Access to http request.</param>
        public CdnService(List<CdnSetting> settings, ILogger logger, HttpContext context)
        {
            this.logger = logger;
            this.context = context;
            this.settings = settings;
        }

        /// <summary>
        /// Gets the name of the content delivery network (CDN) provider.
        /// </summary>
        public string ProviderName => "Sky CMD CDN";

        /// <summary>
        /// Gets the CDN service.
        /// </summary>
        /// <param name="dbContext">Application database context.</param>
        /// <param name="logger">Log service.</param>
        /// <param name="context">HTTP context.</param>
        /// <returns>CdnService.</returns>
        public static CdnService GetCdnService(ApplicationDbContext dbContext, ILogger logger, HttpContext context)
        {
            var cdnSettings = dbContext.Settings
                .Where(f => f.Group == CDNGROUPNAME)
                .AsNoTracking()
                .ToListAsync().Result
                .Select(s => JsonConvert.DeserializeObject<CdnSetting>(s.Value))
                .ToList();

            return new CdnService(cdnSettings, logger, context);
        }

        /// <summary>
        /// Indicates if CDN integration is configured.
        /// </summary>
        /// <returns>If true then a CDN or Front Door integration is configured.</returns>
        public bool IsConfigured()
        {
            return settings.Any();
        }

        /// <summary>
        /// Checks to see if a particular CDN type is configured.
        /// </summary>
        /// <param name="type">CDN type to check for.</param>
        /// <returns>True of false.</returns>
        public bool IsConfigured(CdnProviderEnum type)
        {
            return settings.Any(a => a.CdnProvider == type);
        }

        /// <summary>
        /// Purges the CDN (or Front Door) if either is configured.
        /// </summary>
        /// <param name="purgeUrls">Purge URL Paths.</param>
        /// <returns>ArmOperation results.</returns>
        public async Task<List<CdnResult>> PurgeCdn(List<string> purgeUrls)
        {
            var results = new List<CdnResult>();

            // Since this uses the default azure credential, we need to check if the host is localhost.
            // If it is, then we don't need to do anything.
            //if (context.Request.Host.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    results.Add(new CdnResult
            //    {
            //        Status = HttpStatusCode.OK,
            //        ReasonPhrase = "Localhost",
            //        IsSuccessStatusCode = true,
            //        ClientRequestId = Guid.NewGuid().ToString(),
            //        Id = Guid.NewGuid().ToString(),
            //        EstimatedFlushDateTime = DateTimeOffset.UtcNow.AddMinutes(10),
            //        Message = "Localhost CDN purge request.",
            //    });
            //    return results;
            //}

            purgeUrls = purgeUrls.Distinct().ToList();

            foreach (var setting in settings)
            {
                ICdnDriver driver = null;

                switch (setting.CdnProvider)
                {
                    case CdnProviderEnum.AzureFrontdoor:
                    case CdnProviderEnum.AzureCDN:
                        driver = new AzureCdnDriver(setting, logger);
                        break;
                    case CdnProviderEnum.Cloudflare:
                        driver = new CloudflareCdnDriver(setting, logger);
                        break;
                    case CdnProviderEnum.Sucuri:
                        driver = new SucuriCdnService(setting, logger);
                        break;
                    default:
                        break;
                }

                results.AddRange(await driver.PurgeCdn(purgeUrls));
            }

            return results;
        }

        /// <summary>
        /// Purges the entire CDN for the current endpoint.
        /// </summary>
        /// <returns>CDN purge results.</returns>
        public async Task<List<CdnResult>> PurgeCdn()
        {
            var results = new List<CdnResult>();
            foreach (var setting in settings)
            {
                ICdnDriver driver = null;
                switch (setting.CdnProvider)
                {
                    case CdnProviderEnum.AzureFrontdoor:
                    case CdnProviderEnum.AzureCDN:
                        driver = new AzureCdnDriver(setting, logger);
                        break;
                    case CdnProviderEnum.Cloudflare:
                        driver = new CloudflareCdnDriver(setting, logger);
                        break;
                    case CdnProviderEnum.Sucuri:
                        driver = new SucuriCdnService(setting, logger);
                        break;
                    default:
                        break;
                }

                results.AddRange(await driver.PurgeCdn());
            }

            return results;
        }
    }
}