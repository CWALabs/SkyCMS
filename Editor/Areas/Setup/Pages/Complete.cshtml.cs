    // <copyright file="Complete.cshtml.cs" company="Moonrise Software, LLC">
    // Copyright (c) Moonrise Software, LLC. All rights reserved.
    // Licensed under the MIT License (https://opensource.org/licenses/MIT)
    // See https://github.com/CWALabs/SkyCMS
    // for more information concerning the license and the contributors participating to this project.
    // </copyright>

    namespace Sky.Editor.Areas.Setup.Pages
    {
        using System;
        using System.IO;
        using System.Threading.Tasks;
        using Microsoft.AspNetCore.Hosting;
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
            private readonly IWebHostEnvironment webHostEnvironment;
            private readonly ILogger<CompleteModel> logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="CompleteModel"/> class.
            /// </summary>
            /// <param name="setupService">Setup service.</param>
            /// <param name="hostApplicationLifetime">Host application lifetime.</param>
            /// <param name="webHostEnvironment">Web host environment.</param>
            /// <param name="logger">Logger instance.</param>
            public CompleteModel(
                ISetupService setupService,
                IHostApplicationLifetime hostApplicationLifetime,
                IWebHostEnvironment webHostEnvironment,
                ILogger<CompleteModel> logger)
            {
                this.setupService = setupService;
                this.hostApplicationLifetime = hostApplicationLifetime;
                this.webHostEnvironment = webHostEnvironment;
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
            /// Gets or sets a value indicating whether restart has been triggered.
            /// </summary>
            public bool RestartTriggered { get; set; }

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

                    // Check if restart has already been triggered
                    RestartTriggered = config.RestartTriggered;
                }

                // Detect if running in Docker
                IsDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
            }

            /// <summary>
            /// Handles application restart request.
            /// </summary>
            /// <returns>JSON result indicating success.</returns>
            public async Task<IActionResult> OnPostRestartAsync()
            {
                try
                {
                    // Check if setup is still in progress
                    var config = await setupService.GetCurrentSetupAsync();
                    if (config == null)
                    {
                        logger.LogWarning("Restart attempted but no setup configuration found");
                        return new JsonResult(new { success = false, message = "Setup not found" });
                    }

                    // Security: Only allow restart once during initial setup
                    if (config.RestartTriggered)
                    {
                        logger.LogWarning("Restart attempt blocked - restart already triggered for setup ID: {SetupId}", config.Id);
                        return new JsonResult(new { success = false, message = "Restart already triggered" });
                    }

                    // Mark restart as triggered to prevent subsequent attempts
                    await setupService.MarkRestartTriggeredAsync(config.Id);

                    logger.LogInformation("Application restart requested from setup completion page for setup ID: {SetupId}", config.Id);

                    // Strategy 1: Touch web.config for IIS hosting
                    var webConfigPath = Path.Combine(webHostEnvironment.ContentRootPath, "web.config");
                    if (System.IO.File.Exists(webConfigPath))
                    {
                        logger.LogInformation("Triggering IIS restart by touching web.config");
                        System.IO.File.SetLastWriteTimeUtc(webConfigPath, DateTime.UtcNow);
                    }

                    // Strategy 2: Trigger graceful shutdown (works for Docker, systemd, and other process managers)
                    logger.LogInformation("Triggering application shutdown via IHostApplicationLifetime");

                    // Use a short delay to allow the response to be sent before shutdown begins
                    Task.Run(async () =>
                    {
                        await Task.Delay(1000); // Give 1 second for response to send
                        hostApplicationLifetime.StopApplication();
                    });

                    return new JsonResult(new { success = true, message = "Application is restarting..." });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to restart application");
                    return new JsonResult(new { success = false, message = $"Restart failed: {ex.Message}" });
                }
            }
        }
    }
