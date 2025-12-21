// <copyright file="Step1_Storage.cshtml.cs" company="Moonrise Software, LLC">
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
    using Azure.Storage.Blobs;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Sky.Editor.Services.Setup;

    /// <summary>
    /// Setup wizard step 2: Storage configuration.
    /// </summary>
    public class Step1_Storage : PageModel
    {
        private readonly ISetupService setupService;
        private readonly ISetupCheckService setupCheckService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Step1_Storage"/> class.
        /// </summary>
        /// <param name="setupService">Setup service.</param>
        /// <param name="setupCheckService">Setup check service.</param>
        public Step1_Storage(ISetupService setupService, ISetupCheckService setupCheckService)
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
        /// Gets or sets the storage type.
        /// </summary>
        [BindProperty]
        public string StorageType { get; set; }

        /// <summary>
        /// Gets or sets the storage connection string.
        /// </summary>
        [BindProperty]
        public string StorageConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the container name (for Azure Blob Storage).
        /// </summary>
        [BindProperty]
        public string ContainerName { get; set; } = "$web";

        /// <summary>
        /// Gets or sets the blob public URL.
        /// </summary>
        [BindProperty]
        [Required(ErrorMessage = "Public URL is required")]
        public string BlobPublicUrl { get; set; } = "/";

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the success message.
        /// </summary>
        public string SuccessMessage { get; set; }

        /// <summary>
        /// Gets a value indicating whether storage is pre-configured.
        /// </summary>
        public bool IsPreConfigured { get; private set; }

        /// <summary>
        /// Gets a value indicating whether blob public URL is pre-configured.
        /// </summary>
        public bool BlobPublicUrlPreConfigured { get; private set; }

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

            try
            {
                var config = await setupService.GetCurrentSetupAsync();
                if (config == null)
                {
                    return RedirectToPage("./Index");
                }

                SetupId = config.Id;
                StorageConnectionString = config.StoragePreConfigured ? "**********************" : config.StorageConnectionString;
                BlobPublicUrl = config.BlobPublicUrl;
                IsPreConfigured = config.StoragePreConfigured;
                BlobPublicUrlPreConfigured = config.BlobPublicUrlPreConfigured;

                var testConnectionString = config.StoragePreConfigured
                    ? config.StorageConnectionString
                    : StorageConnectionString;

                // Infer storage type from connection string
                if (!string.IsNullOrEmpty(testConnectionString))
                {
                    StorageType = InferStorageType(testConnectionString);
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error checking setup status: {ex.Message}";
                return Page();
            }
        }

        /// <summary>
        /// Handles POST requests to proceed to next step.
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

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var config = await setupService.GetCurrentSetupAsync();
            SetupId = config.Id;

            if (!config.StoragePreConfigured && string.IsNullOrWhiteSpace(StorageConnectionString))
            {
                ModelState.AddModelError(nameof(StorageConnectionString), "Connection string is required.");
                return Page();
            }

            try
            {
                var testConnectionString = config.StoragePreConfigured
                    ? config.StorageConnectionString
                    : StorageConnectionString;

                var result = await setupService.TestStorageConnectionAsync(testConnectionString);
                if (!result.Success)
                {
                    ErrorMessage = "Storage connection test failed. Please ensure the connection string is correct.";
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to proceed: {ex.Message}";
                return Page();
            }

            try
            {

                // If this is an Azure Blob Storage, ensure container name is set
                var testConnectionString = config.StoragePreConfigured
                    ? config.StorageConnectionString
                    : StorageConnectionString;
                var inferredType = InferStorageType(testConnectionString);
                if (inferredType == "AzureBlob" && string.IsNullOrWhiteSpace(ContainerName))
                {
                    // Create default container.
                    var blobClient = new BlobServiceClient(testConnectionString);
                    var container = blobClient.GetBlobContainerClient("$web");
                    await container.CreateIfNotExistsAsync();

                    // Enable static website
                    var serviceProperties = await blobClient.GetPropertiesAsync();

                    if (!serviceProperties.Value.StaticWebsite.Enabled)
                    {
                        serviceProperties.Value.StaticWebsite.Enabled = true;
                        serviceProperties.Value.StaticWebsite.IndexDocument = "index.html";
                        serviceProperties.Value.StaticWebsite.ErrorDocument404Path = "404.html";

                        await blobClient.SetPropertiesAsync(serviceProperties.Value);
                    }
                }

                // Use config values if pre-configured, otherwise use form values
                var connectionStringToSave = config.StoragePreConfigured 
                    ? config.StorageConnectionString 
                    : StorageConnectionString;
                    
                var blobPublicUrlToSave = config.BlobPublicUrlPreConfigured 
                    ? config.BlobPublicUrl 
                    : BlobPublicUrl;

                await setupService.UpdateStorageConfigAsync(
                    SetupId,
                    connectionStringToSave,
                    blobPublicUrlToSave);

                await setupService.UpdateStepAsync(SetupId, 1);  // ✅ Changed back to 1
                return RedirectToPage("./Step2_AdminAccount");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to proceed: {ex.Message}";
                return Page();
            }
        }

        /// <summary>
        /// Infers storage type from connection string.
        /// </summary>
        /// <param name="connectionString">Connection string.</param>
        /// <returns>Storage type.</returns>
        private string InferStorageType(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return string.Empty;
            }

            if (connectionString.Contains("DefaultEndpointsProtocol=", StringComparison.OrdinalIgnoreCase) ||
                connectionString.Contains("AccountName=", StringComparison.OrdinalIgnoreCase))
            {
                return "AzureBlob";
            }
            else if (connectionString.Contains("Bucket=", StringComparison.OrdinalIgnoreCase) &&
                     connectionString.Contains("Region=", StringComparison.OrdinalIgnoreCase))
            {
                return "AmazonS3";
            }
            else if (connectionString.Contains("AccountId=", StringComparison.OrdinalIgnoreCase) &&
                     connectionString.Contains("Bucket=", StringComparison.OrdinalIgnoreCase))
            {
                return "CloudflareR2";
            }

            return string.Empty;
        }
    }
}
