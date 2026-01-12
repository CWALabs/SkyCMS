// <copyright file="HomeController.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Cosmos.BlobService;
    using Cosmos.Common.Data;
    using Cosmos.Common.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Net.Http.Headers;
    using Sky.Cms.Models;
    using Sky.Cms.Services;
    using Sky.Editor.Data.Logic;
    using Sky.Editor.Services.EditorSettings;
    using Sky.Editor.Services.Setup;

    /// <summary>
    /// Home page controller.
    /// </summary>
    [Authorize]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]

    public class HomeController : Controller
    {
        private readonly ArticleEditLogic articleLogic;
        private readonly EditorSettings options;
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<IdentityUser> userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">ILogger to use.</param>
        /// <param name="options">Cosmos configuration.</param>
        /// <param name="dbContext"><see cref="ApplicationDbContext">Database context</see>.</param>
        /// <param name="articleLogic"><see cref="ArticleEditLogic">Article edit logic.</see>.</param>
        /// <param name="userManager">User manager.</param>
        /// <param name="signInManager">Sign in manager service.</param>
        /// <param name="storageContext"><see cref="StorageContext">File storage context</see>.</param>
        /// <param name="emailSender">Email service.</param>
        /// <param name="configuration">Website configuration.</param>
        /// <param name="services">Services provider.</param>
        public HomeController(
            ILogger<HomeController> logger,
            IEditorSettings options,
            ApplicationDbContext dbContext,
            ArticleEditLogic articleLogic,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IEmailSender emailSender,
            IConfiguration configuration,
            IServiceProvider services)
        {
            // This handles injection manually to make sure everything is setup.
            this.options = (EditorSettings)options;
            this.articleLogic = articleLogic;
            this.dbContext = dbContext;
            this.userManager = userManager;
        }

        /// <summary>
        /// Get edit list.
        /// </summary>
        /// <param name="target">Path to page.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IActionResult> EditList(string target)
        {
            // Decode and normalize the target URL
            if (!string.IsNullOrEmpty(target))
            {
                target = System.Net.WebUtility.UrlDecode(target);
                target = target.Trim().TrimStart('/').TrimEnd('/');
            }

            var article = await articleLogic.GetArticleByUrl(target);

            if (article == null)
            {
                return NotFound($"No article found for URL: {target}");
            }

            var data = await dbContext.Articles.OrderByDescending(o => o.VersionNumber)
                .Where(a => a.ArticleNumber == article.ArticleNumber).Select(s => new ArticleEditMenuItem
                {
                    Id = s.Id,
                    ArticleNumber = s.ArticleNumber,
                    Published = s.Published,
                    VersionNumber = s.VersionNumber,
                    UsesHtmlEditor = s.Content != null && (s.Content.ToLower().Contains(" editable=") || s.Content.ToLower().Contains(" data-ccms-ceid="))
                }).OrderByDescending(o => o.VersionNumber).Take(1).ToListAsync();

            return Json(data);
        }

        /// <summary>
        /// Gets the index page.
        /// </summary>
        /// <param name="lang">Language code.</param>
        /// <param name="mode">json or nothing.</param>
        /// <param name="layoutId">Layout ID when previewing a layout.</param>
        /// <param name="articleId">Article Id.</param>
        /// <param name="previewType">Preview type.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IActionResult> Index(string lang = "", string mode = "", Guid? layoutId = null, Guid? articleId = null, string previewType = "")
        {
            // Note: Setup check is handled by middleware (TenantSetupMiddleware for multi-tenant,
            // or Program.cs middleware for single-tenant) before this action is reached.
            // Ensure user is authenticated (middleware may bypass during setup)
            if (User.Identity?.IsAuthenticated == false)
            {
                Response.Cookies.Delete("CosmosAuthCookie");
                return Redirect("~/Identity/Account/Login");
            }

            // Make sure the user's claims identity has an account here.
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                Response.Cookies.Delete("CosmosAuthCookie");
                return Redirect("~/Identity/Account/Logout");
            }

            if (options.AllowSetup && (await dbContext.Users.CountAsync()) == 1 && !User.IsInRole("Administrators"))
            {
                await userManager.AddToRoleAsync(user, "Administrators");
            }

            // If yes, do NOT include headers that allow caching. 
            Response.Headers[HeaderNames.CacheControl] = "no-store";

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ArticleViewModel article;

            if (articleId.HasValue)
            {
                var userId = new Guid(user.Id);
                article = await articleLogic.GetArticleById(articleId.Value, EnumControllerName.Edit, userId);
            }
            else
            {
                var path = HttpContext.Request.Path.Value?.TrimStart('/') ?? string.Empty;
                article = await articleLogic.GetArticleByUrl(path);

                if (article == null)
                {
                    // See if a page is un-published, but does exist, let us edit it.
                    article = await articleLogic.GetArticleByUrl(HttpContext.Request.Path, publishedOnly: false);

                    // Create your own not found page for a graceful page for users.
                    article = await articleLogic.GetArticleByUrl("/not_found");

                    HttpContext.Response.StatusCode = 404;

                    if (article == null)
                    {
                        return NotFound();
                    }
                }
            }

            if (layoutId.HasValue)
            {
                ViewData["LayoutId"] = layoutId.Value.ToString();
                article.Layout = new LayoutViewModel(await dbContext.Layouts.FirstOrDefaultAsync(f => f.Id == layoutId));
            }

            var viewRenderingService = HttpContext.RequestServices.GetService(typeof(IViewRenderService)) as IViewRenderService;
            var renderedView = await viewRenderingService.RenderToStringAsync("~/Views/Home/index.cshtml", article);
            ViewData["RenderedView"] = renderedView;
            ViewData["CurrentPath"] = HttpContext.Request.Path.Value?.TrimStart('/') ?? string.Empty;

            // If no preview type, load the edit list.
            if (string.IsNullOrEmpty(previewType))
            {
                ViewData["LoadEditList"] = true; // Signal to load the edit list in the main menu.
                return View("Wrapper");
            }

            return View("~/Views/Home/Preview.cshtml");
        }

        /// <summary>
        ///     Gets an article by its ID (or row key).
        /// </summary>
        /// <param name="articleNumber">Article number.</param>
        /// <param name="versionNumber">Version number.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IActionResult> Preview(int articleNumber, int? versionNumber = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ViewData["EditModeOn"] = false;
            var article = await articleLogic.GetArticleByArticleNumber(articleNumber, versionNumber);

            // Home/Preview/154
            if (article != null)
            {
                article.ReadWriteMode = false;
                article.EditModeOn = false;

                return View("Preview", article);
            }

            return NotFound();
        }

        /// <summary>
        /// Gets the error page.
        /// </summary>
        /// <returns>Returns an <see cref="IActionResult"/>.</returns>
        public IActionResult Error()
        {
            ViewData["EditModeOn"] = false;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Gets the application validation for Microsoft.
        /// </summary>
        /// <returns>Returns an <see cref="FileContentResult"/> if successful.</returns>
        [AllowAnonymous]
        public IActionResult GetMicrosoftIdentityAssociation()
        {
            var model = new MicrosoftValidationObject();
            model.associatedApplications.Add(new AssociatedApplication() { applicationId = options.MicrosoftAppId });

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(model);

            return File(Encoding.UTF8.GetBytes(data), "application/json", fileDownloadName: "microsoft-identity-association.json");
        }

        /// <summary>
        /// Returns if a user has not been granted access yet.
        /// </summary>
        /// <returns>Returns an <see cref="IActionResult"/>.</returns>
        [Authorize]
        public IActionResult AccessPending()
        {
            var model = new ArticleViewModel
            {
                Id = Guid.NewGuid(),
                ArticleNumber = 0,
                UrlPath = null,
                VersionNumber = 0,
                Published = null,
                Title = "Access Pending",
                Content = null,
                Updated = default,
                HeadJavaScript = null,
                FooterJavaScript = null,
                Layout = null,
                ReadWriteMode = false,
                PreviewMode = false,
                EditModeOn = false
            };
            return View(model);
        }

        ///// <summary>
        ///// Ensures there is a Layout.
        ///// </summary>
        ///// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        //private async Task<bool> EnsureLayoutExists()
        //{
        //    return await dbContext.Layouts.CosmosAnyAsync();
        //}

        ///// <summary>
        ///// Ensures there is at least one article.
        ///// </summary>
        ///// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        //private async Task<bool> EnsureArticleExists()
        //{
        //    return await dbContext.Articles.CosmosAnyAsync();
        //}
    }
}
