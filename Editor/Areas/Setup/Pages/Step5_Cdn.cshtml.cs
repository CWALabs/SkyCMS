// <copyright file="Step5a_Cdn.cshtml.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Areas.Setup.Pages
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Sky.Editor.Services.Setup;

    /// <summary>
    /// Setup wizard step 5a: CDN configuration (optional).
    /// </summary>
    public class Step5_Cdn : PageModel
    {
        private readonly ISetupService setupService;
        private readonly ISetupCheckService setupCheckService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Step5_Cdn"/> class.
        /// </summary>
        /// <param name="setupService">Setup service.</param>
        /// <param name="setupCheckService">Setup check service.</param>
        public Step5_Cdn(ISetupService setupService, ISetupCheckService setupCheckService)
        {
            this.setupService = setupService;
            this.setupCheckService = setupCheckService;
        }

        /// <summary>
        /// Gets or sets the setup session ID.
        /// </summary>
        [BindProperty]
        public Guid SetupId { get; set; }

        /// <summary>
        /// Gets or sets the selected CDN provider.
        /// </summary>
        [BindProperty]
        public string SelectedProvider { get; set; } = "None";

        /// <summary>
        /// Gets or sets the Azure subscription ID.
        /// </summary>
        [BindProperty]
        [Display(Name = "Azure Subscription ID")]
        public string AzureSubscriptionId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Azure resource group name.
        /// </summary>
        [BindProperty]
        [Display(Name = "Resource Group")]
        public string AzureResourceGroup { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Azure CDN profile name.
        /// </summary>
        [BindProperty]
        [Display(Name = "Profile Name")]
        public string AzureProfileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Azure CDN endpoint name.
        /// </summary>
        [BindProperty]
        [Display(Name = "Endpoint Name")]
        public string AzureEndpointName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to use Azure Front Door instead of Azure CDN.
        /// </summary>
        [BindProperty]
        [Display(Name = "Use Front Door (instead of Azure CDN)")]
        public bool AzureIsFrontDoor { get; set; } = false;

        /// <summary>
        /// Gets or sets the Cloudflare API token.
        /// </summary>
        [BindProperty]
        [Display(Name = "API Token")]
        public string CloudflareApiToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Cloudflare zone ID.
        /// </summary>
        [BindProperty]
        [Display(Name = "Zone ID")]
        public string CloudflareZoneId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Sucuri API key.
        /// </summary>
        [BindProperty]
        [Display(Name = "API Key")]
        public string SucuriApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the Sucuri API secret.
        /// </summary>
        [BindProperty]
        [Display(Name = "API Secret")]
        public string SucuriApiSecret { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        public string SuccessMessage { get; set; }

        /// <summary>
        /// Gets a value indicating whether the CDN configuration is pre-configured.
        /// </summary>
        public bool IsPreConfigured { get; private set; }

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

            var config = await setupService.GetCurrentSetupAsync();
            if (config == null)
            {
                return RedirectToPage("./Index");
            }

            SetupId = config.Id;

            // Load existing CDN configuration if any
            if (!string.IsNullOrEmpty(config.AzureCdnSubscriptionId) &&
                !string.IsNullOrEmpty(config.AzureCdnResourceGroup) &&
                !string.IsNullOrEmpty(config.AzureCdnProfileName) &&
                !string.IsNullOrEmpty(config.AzureCdnEndpointName))
            {
                SelectedProvider = "Azure";
                AzureSubscriptionId = config.AzureCdnSubscriptionId;
                AzureResourceGroup = config.AzureCdnResourceGroup;
                AzureProfileName = config.AzureCdnProfileName;
                AzureEndpointName = config.AzureCdnEndpointName;
                AzureIsFrontDoor = config.AzureCdnIsFrontDoor;
            }
            else if (!string.IsNullOrEmpty(config.CloudflareApiToken))
            {
                SelectedProvider = "Cloudflare";
                CloudflareApiToken = config.CloudflareApiToken;
                CloudflareZoneId = config.CloudflareZoneId;
            }
            else if (!string.IsNullOrEmpty(config.SucuriApiKey))
            {
                SelectedProvider = "Sucuri";
                SucuriApiKey = config.SucuriApiKey;
                SucuriApiSecret = config.SucuriApiSecret;
            }

            return Page();
        }

        /// <summary>
        /// Handles POST requests to save and continue.
        /// </summary>
        /// <returns>Redirect to next step.</returns>
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
                // Validate based on selected provider
                if (SelectedProvider == "Azure")
                {
                    if (string.IsNullOrEmpty(AzureSubscriptionId) ||
                        string.IsNullOrEmpty(AzureResourceGroup) ||
                        string.IsNullOrEmpty(AzureProfileName) ||
                        string.IsNullOrEmpty(AzureEndpointName))
                    {
                        ErrorMessage = "All Azure CDN fields are required when Azure is selected.";
                        return Page();
                    }
                }
                else if (SelectedProvider == "Cloudflare")
                {
                    if (string.IsNullOrEmpty(CloudflareApiToken) ||
                        string.IsNullOrEmpty(CloudflareZoneId))
                    {
                        ErrorMessage = "Both Cloudflare API Token and Zone ID are required.";
                        return Page();
                    }
                }
                else if (SelectedProvider == "Sucuri")
                {
                    if (string.IsNullOrEmpty(SucuriApiKey) ||
                        string.IsNullOrEmpty(SucuriApiSecret))
                    {
                        ErrorMessage = "Both Sucuri API Key and API Secret are required.";
                        return Page();
                    }
                }

                // Save CDN configuration (may be empty if "None" selected)
                await setupService.UpdateCdnConfigAsync(
                    SetupId,
                    SelectedProvider == "Azure" ? AzureSubscriptionId : string.Empty,
                    SelectedProvider == "Azure" ? AzureResourceGroup : string.Empty,
                    SelectedProvider == "Azure" ? AzureProfileName : string.Empty,
                    SelectedProvider == "Azure" ? AzureEndpointName : string.Empty,
                    SelectedProvider == "Azure" && AzureIsFrontDoor,
                    SelectedProvider == "Cloudflare" ? CloudflareApiToken : string.Empty,
                    SelectedProvider == "Cloudflare" ? CloudflareZoneId : string.Empty,
                    SelectedProvider == "Sucuri" ? SucuriApiKey : string.Empty,
                    SelectedProvider == "Sucuri" ? SucuriApiSecret : string.Empty);

                await setupService.UpdateStepAsync(SetupId, 5);

                return RedirectToPage("./Step6_Review"); // ✅ FIX: Changed from ./Step5a_Cdn
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save CDN configuration: {ex.Message}";
                return Page();
            }
        }

        /// <summary>
        /// Handles POST requests to skip CDN configuration.
        /// </summary>
        /// <returns>Redirect to next step.</returns>
        public async Task<IActionResult> OnPostSkipAsync()
        {
            try
            {
                // ✅ ADD THIS: Ensure we have a valid SetupId
                var config = await setupService.GetCurrentSetupAsync();
                if (config == null)
                {
                    return RedirectToPage("./Index");
                }
                SetupId = config.Id;

                // Clear any existing CDN configuration
                await setupService.UpdateCdnConfigAsync(
                    SetupId,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    false,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    string.Empty);

                await setupService.UpdateStepAsync(SetupId, 5);

                return RedirectToPage("./Step6_Review"); // ✅ FIX: Changed from ./Step5a_Cdn
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to skip CDN configuration: {ex.Message}";
                return Page();
            }
        }
    }
}
