// <copyright file="Complete.cshtml.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Areas.Setup.Pages
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Sky.Editor.Services;
    using Sky.Editor.Services.Setup;

    /// <summary>
    /// Setup completion page.
    /// </summary>
    public class CompleteModel : PageModel
    {
        private readonly ISetupService setupService;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly ILogger<CompleteModel> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteModel"/> class.
        /// </summary>
        /// <param name="setupService">Setup service.</param>
        /// <param name="hostApplicationLifetime">Host application lifetime.</param>
        /// <param name="logger">Logger instance.</param>
        public CompleteModel(
            ISetupService setupService,
            IHostApplicationLifetime hostApplicationLifetime,
            ILogger<CompleteModel> logger)
        {
            this.setupService = setupService;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.logger = logger;
        }

        /// <summary>
        /// Gets or sets the admin email.
        /// </summary>
        public string AdminEmail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether email is configured.
        /// </summary>
        public bool EmailConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether CDN is configured.
        /// </summary>
        public bool CdnConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether running in Docker.
        /// </summary>
        public bool IsDocker { get; set; }

        /// <summary>
        /// Handles GET requests.
        /// </summary>
        /// <returns>Page result.</returns>
        public async Task OnGetAsync()
        {
            var config = await setupService.GetCurrentSetupAsync();
            if (config != null)
            {
                AdminEmail = config.AdminEmail;
                EmailConfigured = !string.IsNullOrEmpty(config.SendGridApiKey) ||
                                  !string.IsNullOrEmpty(config.AzureEmailConnectionString) ||
                                  !string.IsNullOrEmpty(config.SmtpHost);

                CdnConfigured = !string.IsNullOrEmpty(config.AzureCdnSubscriptionId) ||
                                !string.IsNullOrEmpty(config.CloudflareApiToken) ||
                                !string.IsNullOrEmpty(config.SucuriApiKey);
            }

            // Detect if running in Docker
            IsDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        }

        /// <summary>
        /// Handles application restart request.
        /// </summary>
        /// <returns>Content result.</returns>
        public IActionResult OnPostRestart()
        {
            logger.LogInformation("Application restart requested from setup completion page");

            // Trigger graceful shutdown (Docker will automatically restart the container)
            hostApplicationLifetime.StopApplication();

            return Content("Application is restarting...");
        }
    }
}
