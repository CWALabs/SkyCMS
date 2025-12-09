// <copyright file="Index.cshtml.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Areas.Setup.Pages
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Microsoft.Extensions.Configuration;
    using Sky.Editor.Services.Setup;

    /// <summary>
    /// Setup wizard welcome page.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly ISetupService setupService;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexModel"/> class.
        /// </summary>
        /// <param name="setupService">Setup service.</param>
        /// <param name="configuration">Configuration.</param>
        public IndexModel(ISetupService setupService, IConfiguration configuration)
        {
            this.setupService = setupService;
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Handles GET requests.
        /// </summary>
        /// <returns>Page result or redirect.</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            // Check if setup is allowed
            var allowSetup = configuration.GetValue<bool?>("CosmosAllowSetup") ?? false;
            
            if (!allowSetup)
            {
                return RedirectToPage("/Index", new { area = "" });
            }

            // Check if setup is already complete
            var existingConfig = await setupService.GetCurrentSetupAsync();
            if (existingConfig?.IsComplete == true)
            {
                return RedirectToPage("/Index", new { area = "" });
            }

            return Page();
        }

        /// <summary>
        /// Handles POST requests to start setup.
        /// </summary>
        /// <returns>Redirect to next step.</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // ✅ Validate database connection FIRST
                var dbConnectionString = configuration.GetConnectionString("ApplicationDbContextConnection");
                if (string.IsNullOrEmpty(dbConnectionString))
                {
                    ErrorMessage = "Database connection string not found. Please configure 'ApplicationDbContextConnection' in appsettings.json or user secrets.";
                    return Page();
                }

                var testResult = await setupService.TestDatabaseConnectionAsync(dbConnectionString);
                if (!testResult.Success)
                {
                    ErrorMessage = $"Database connection failed: {testResult.Message}";
                    return Page();
                }

                // Initialize a new setup session
                await setupService.InitializeSetupAsync();
                
                return RedirectToPage("./Step1_Storage");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to initialize setup: {ex.Message}";
                return Page();
            }
        }
    }
}