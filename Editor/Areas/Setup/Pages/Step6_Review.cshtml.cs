// <copyright file="Step6_Review.cshtml.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Areas.Setup.Pages
{
    using System;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Logging;
    using Sky.Editor.Services.Setup;

    /// <summary>
    /// Setup wizard step 6: Review and complete setup.
    /// </summary>
    public class Step6_Review : PageModel
    {
        private readonly ISetupService setupService;
        private readonly ILogger<Step6_Review> logger;
        private readonly ISetupCheckService setupCheckService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Step6_Review"/> class.
        /// </summary>
        /// <param name="setupService">Setup service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="setupCheckService">Setup check service.</param>
        public Step6_Review(ISetupService setupService, ILogger<Step6_Review> logger, ISetupCheckService setupCheckService)
        {
            this.setupService = setupService;
            this.logger = logger;
            this.setupCheckService = setupCheckService;
        }

        /// <summary>
        /// Gets or sets the setup session ID.
        /// </summary>
        [BindProperty]
        public Guid SetupId { get; set; }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        public SetupConfiguration Config { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Handles GET requests.
        /// </summary>
        /// <returns>Page result.</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // Check if setup has been completed
            if (await setupCheckService.IsSetup())
            {
                // Redirect to setup page
                Response.Redirect("/");
            }

            Config = await setupService.GetCurrentSetupAsync();
            if (Config == null)
            {
                return RedirectToPage("./Index");
            }

            SetupId = Config.Id;

            // Validate all required fields are set
            //if (string.IsNullOrEmpty(Config.DatabaseConnectionString))
            //{
            //    ErrorMessage = "Database connection string is missing. Please go back and configure the database.";
            //    return Page();
            //}

            if (string.IsNullOrEmpty(Config.StorageConnectionString))
            {
                ErrorMessage = "Storage connection string is missing. Please go back and configure storage.";
                return Page();
            }

            if (string.IsNullOrEmpty(Config.SenderEmail) || string.IsNullOrEmpty(Config.AdminPassword))
            {
                ErrorMessage = "Administrator account is incomplete. Please go back and create the admin account.";
                return Page();
            }

            if (string.IsNullOrEmpty(Config.PublisherUrl))
            {
                ErrorMessage = "Publisher URL is missing. Please go back and configure the publisher.";
                return Page();
            }

            return Page();
        }

        /// <summary>
        /// Handles POST requests to complete setup.
        /// </summary>
        /// <returns>Redirect to login page.</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            // Check if setup has been completed
            if (await setupCheckService.IsSetup())
            {
                // Redirect to setup page
                Response.Redirect("/");
            }

            try
            {
                logger.LogInformation("Starting setup completion process for setup ID: {SetupId}", SetupId);

                // Complete the setup process
                var result = await setupService.CompleteSetupAsync(SetupId);

                if (!result.Success)
                {
                    ErrorMessage = result.Message;
                    Config = await setupService.GetCurrentSetupAsync();
                    return Page();
                }

                logger.LogInformation("Setup completed successfully. Setup ID: {SetupId}", SetupId);

                // Redirect to completion success page
                return RedirectToPage("./Complete");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to complete setup. Setup ID: {SetupId}", SetupId);
                ErrorMessage = $"Failed to complete setup: {ex.Message}";
                Config = await setupService.GetCurrentSetupAsync();
                return Page();
            }
        }
    }
}
