// <copyright file="Step4_Publisher.cshtml.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Areas.Setup.Pages
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using Sky.Editor.Services.Layouts;
    using Sky.Editor.Services.Setup;

    /// <summary>
    /// Setup wizard step 3: Publisher configuration.
    /// </summary>
    public class Step3_Publisher : PageModel
    {
        private readonly ISetupService setupService;
        private readonly ILayoutImportService layoutImportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="Step3_Publisher"/> class.
        /// </summary>
        /// <param name="setupService">Setup service.</param>
        /// <param name="layoutImportService">Layout import service.</param>
        public Step3_Publisher(ISetupService setupService, ILayoutImportService layoutImportService)
        {
            this.setupService = setupService;
            this.layoutImportService = layoutImportService;
        }

        /// <summary>
        /// Gets or sets the setup session ID.
        /// </summary>
        [BindProperty]
        public Guid SetupId { get; set; }

        /// <summary>
        /// Gets or sets the publisher URL.
        /// </summary>
        [BindProperty]
        [Required(ErrorMessage = "Website URL is required")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        [Display(Name = "Website URL")]
        public string PublisherUrl { get; set; }

        /// <summary>
        /// Gets or sets the website title.
        /// </summary>
        [BindProperty]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Website title is required")]
        [Display(Name = "Website Title")]
        public string WebsiteTitle { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether static web pages are enabled.
        /// </summary>
        [BindProperty]
        [Display(Name = "Static Website Mode")]
        public bool StaticWebPages { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether authentication is required.
        /// </summary>
        [BindProperty]
        [Display(Name = "Require Authentication")]
        public bool CosmosRequiresAuthentication { get; set; } = false;

        /// <summary>
        /// Gets or sets the allowed file types.
        /// </summary>
        [BindProperty]
        [Required(ErrorMessage = "Allowed file types are required")]
        [Display(Name = "Allowed File Types")]
        public string AllowedFileTypes { get; set; } = ".js,.css,.htm,.html,.mov,.webm,.avi,.mp4,.mpeg,.ts,.svg,.json";

        /// <summary>
        /// Gets or sets the Microsoft App ID.
        /// </summary>
        [BindProperty]
        [Display(Name = "Microsoft Application ID")]
        public string MicrosoftAppId { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the site design ID.
        /// </summary>
        [BindProperty]
        public string SiteDesignId { get; set; }

        /// <summary>
        /// Handles GET requests.
        /// </summary>
        /// <returns>Page result.</returns>
        public async Task<IActionResult> OnGetAsync()
        {
            var config = await setupService.GetCurrentSetupAsync();
            if (config == null)
            {
                return RedirectToPage("./Index");
            }

            await GetSiteDesignOptions();

            SetupId = config.Id;
            SiteDesignId = config.SiteDesignId;
            PublisherUrl = config.PublisherUrl;
            StaticWebPages = config.StaticWebPages;
            CosmosRequiresAuthentication = config.CosmosRequiresAuthentication;
            MicrosoftAppId = config.MicrosoftAppId;
            WebsiteTitle = config.WebsiteTitle;

            return Page();
        }

        /// <summary>
        /// Handles POST requests.
        /// </summary>
        /// <returns>Redirect to next step.</returns>
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await GetSiteDesignOptions();
                return Page();
            }

            try
            {
                // Remove trailing slash from URL
                PublisherUrl = PublisherUrl.TrimEnd('/');

                await setupService.UpdatePublisherConfigAsync(
                    SetupId, 
                    PublisherUrl, 
                    true, 
                    CosmosRequiresAuthentication, 
                    AllowedFileTypes,
                    MicrosoftAppId,
                    SiteDesignId,
                    WebsiteTitle);
                
                await setupService.UpdateStepAsync(SetupId, 3);

                return RedirectToPage("./Step4_Email");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save publisher configuration: {ex.Message}";
                await GetSiteDesignOptions();
                return Page();
            }
        }

        private async Task GetSiteDesignOptions()
        {
            var catalog = await layoutImportService.GetCommunityCatalogAsync();
            ViewData["AvailableLayouts"] = catalog.LayoutCatalog.Select(layout => new SiteDesignOption()
            {
                Id = layout.Id,
                Title = layout.Name,
                Description = layout.Description
            }).ToList();
        }

        /// <summary>
        /// Site design option for layout selection.
        /// </summary>
        public class SiteDesignOption
        {
            /// <summary>
            /// Gets or sets the layout ID.
            /// </summary>
            public string Id { get; internal set; }

            /// <summary>
            /// Gets or sets the layout title.
            /// </summary>
            public string Title { get; internal set; }

            /// <summary>
            /// Gets or sets the layout description.
            /// </summary>
            public string Description { get; internal set; }
        }
    }
}
