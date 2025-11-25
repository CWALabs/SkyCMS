// <copyright file="Cosmos___SettingsController.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Controllers
{
    using Cosmos.Common.Data;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Sky.Editor.Models;
    using Sky.Editor.Services.CDN;
    using Sky.Editor.Services.EditorSettings;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// The settings controller.
    /// </summary>
    [Authorize(Roles = "Administrators")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "The URL must be unique and not have a changes of conflicting with user authored web page URLs.")]
    public class Cosmos___SettingsController : Controller
    {
        /// <summary>
        /// Editor settings group name.
        /// </summary>
        public static readonly string EDITORSETGROUPNAME = "EDITORSETTINGS";

        private readonly ApplicationDbContext dbContext;
        private readonly ILogger<Cosmos___SettingsController> logger;
        private readonly IEditorSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cosmos___SettingsController"/> class.
        /// </summary>
        /// <param name="dbContext">Sets the database context.</param>
        /// <param name="logger">Log service.</param>
        /// <param name="settings">Editor settings.</param>
        public Cosmos___SettingsController(
            ApplicationDbContext dbContext,
            ILogger<Cosmos___SettingsController> logger,
            IEditorSettings settings)
        {
            this.dbContext = dbContext;
            this.logger = logger;
            this.settings = settings;
        }

        /// <summary>
        /// Gets the index page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        public IActionResult Index()
        {
            var model = new EditorConfig((EditorSettings)settings);
            model.IsMultiTenantEditor = false; // This is set by environment variables and cannot be changed.
            return View(model);
        }

        /// <summary>
        /// Updates the index page.
        /// </summary>
        /// <param name="model">Editor config model.</param>
        /// <returns>IActionResult.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EditorConfig model)
        {
            // Check if mode is static website, and if so, set the blob URL.
            if (model.StaticWebPages)
            {
                model.BlobPublicUrl = "/";
            }

            var allSettings = await dbContext.Settings.ToListAsync();
            var settings = allSettings.FirstOrDefault(f => f.Group == EDITORSETGROUPNAME);
            if (settings == null)
            {
                settings = new Setting
                {
                    Group = EDITORSETGROUPNAME,
                    Name = "EditorSettings",
                    Value = Newtonsoft.Json.JsonConvert.SerializeObject(model),
                    Description = "Settings used by the Cosmos Editor",
                };
                dbContext.Settings.Add(settings);
            }
            else
            {
                settings.Value = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            }

            // Save the changes to the database.
            await dbContext.SaveChangesAsync();

            return View(model);
        }

        /// <summary>
        /// Gets the CDN configuration page.
        /// </summary>
        /// <returns>IActionResult.</returns>
        public async Task<IActionResult> CDN()
        {
            var model = new CdnViewModel();
            ViewData["Operation"] = null;

            var settings = await dbContext.Settings.Where(f => f.Group == CdnService.CDNGROUPNAME).ToListAsync();
            foreach (var setting in settings)
            {
                var cdnSetting = JsonConvert.DeserializeObject<CdnSetting>(setting.Value);

                switch (cdnSetting.CdnProvider)
                {
                    case CdnProviderEnum.AzureCDN:
                    case CdnProviderEnum.AzureFrontdoor:
                        model.AzureCdn = JsonConvert.DeserializeObject<AzureCdnConfig>(cdnSetting.Value);
                        break;
                    case CdnProviderEnum.Cloudflare:
                        model.Cloudflare = JsonConvert.DeserializeObject<CloudflareCdnConfig>(cdnSetting.Value);
                        break;
                    case CdnProviderEnum.Sucuri:
                        model.Sucuri = JsonConvert.DeserializeObject<SucuriCdnConfig>(cdnSetting.Value);
                        break;
                    default:
                        break;
                }
            }

            return View(model);
        }

        /// <summary>
        /// Updates the CDN configuration.
        /// </summary>
        /// <param name="model">The CDN configuration.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CDN(CdnViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Clear out the old settings
            var settings = await dbContext.Settings.Where(f => f.Group == CdnService.CDNGROUPNAME).ToListAsync();
            dbContext.Settings.RemoveRange(settings);
            await dbContext.SaveChangesAsync();

            if (!string.IsNullOrEmpty(model.AzureCdn.ProfileName))
            {
                var setting = new Setting
                {
                    Group = CdnService.CDNGROUPNAME,
                    Name = CdnProviderEnum.AzureCDN.ToString(),
                    Value = JsonConvert.SerializeObject(new CdnSetting
                    {
                        CdnProvider = model.AzureCdn.IsFrontDoor ? CdnProviderEnum.AzureFrontdoor : CdnProviderEnum.AzureCDN,
                        Value = JsonConvert.SerializeObject(model.AzureCdn),
                    }),
                    Description = "Azure CDN or Front Door configuration.",
                };

                dbContext.Settings.Add(setting);
            }

            if (!string.IsNullOrEmpty(model.Cloudflare.ApiToken))
            {
                var setting = new Setting
                {
                    Group = CdnService.CDNGROUPNAME,
                    Name = CdnProviderEnum.Cloudflare.ToString(),
                    Value = JsonConvert.SerializeObject(new CdnSetting
                    {
                        CdnProvider = CdnProviderEnum.Cloudflare,
                        Value = JsonConvert.SerializeObject(model.Cloudflare),
                    }),
                    Description = "Cloudflare CDN configuration.",
                };
                dbContext.Settings.Add(setting);
            }

            if (!string.IsNullOrEmpty(model.Sucuri.ApiKey))
            {
                var setting = new Setting
                {
                    Group = CdnService.CDNGROUPNAME,
                    Name = CdnProviderEnum.Sucuri.ToString(),
                    Value = JsonConvert.SerializeObject(new CdnSetting
                    {
                        CdnProvider = CdnProviderEnum.Sucuri,
                        Value = JsonConvert.SerializeObject(model.Sucuri),
                    }),
                    Description = "Sucuri CDN configuration.",
                };
                dbContext.Settings.Add(setting);
            }

            await dbContext.SaveChangesAsync();

            var operation = await TestConnection();
            ViewData["TestResult"] = operation;

            return View(model);
        }

        /// <summary>
        /// Removes the CDN configuration.
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public async Task<IActionResult> Remove()
        {
            var cdnConfiguration = await dbContext.Settings.Where(f => f.Group == CdnService.CDNGROUPNAME).ToListAsync();

            if (cdnConfiguration.Any())
            {
                dbContext.Settings.RemoveRange(cdnConfiguration);
                await dbContext.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        private async Task<List<CdnResult>> TestConnection()
        {
            var cdnService = CdnService.GetCdnService(dbContext, logger, HttpContext);

            try
            {
                var result = await cdnService.PurgeCdn(new List<string> { "/" });
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error testing CDN connection.");
                return new List<CdnResult> { new CdnResult { IsSuccessStatusCode = false, Message = ex.Message } };
            }
        }
    }
}
