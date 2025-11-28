// <copyright file="BaseController.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using Cosmos.Common.Data;
    using Cosmos.Common.Models;
    using HtmlAgilityPack;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Base controller.
    /// </summary>
    public abstract class BaseController : Controller
    {
        private readonly UserManager<IdentityUser> baseUserManager;
        private readonly ApplicationDbContext dbContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        ///     Constructor.
        /// </summary>
        /// <param name="dbContext">Database context.</param>
        /// <param name="userManager">User manager.</param>
        internal BaseController(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            this.dbContext = dbContext;
            baseUserManager = userManager;
        }

        /// <summary>
        ///     Server-side validation of HTML.
        /// </summary>
        /// <param name="fieldName">Field name to validate.</param>
        /// <param name="inputHtml">HTML data to check.</param>
        /// <returns>HTML content.</returns>
        /// <remarks>
        ///     <para>
        ///         The purpose of this method is to validate HTML prior to be saved to the database.
        ///         It uses an instance of <see cref="HtmlAgilityPack.HtmlDocument" /> to check HTML formatting.
        ///     </para>
        /// </remarks>
        internal string BaseValidateHtml(string fieldName, string inputHtml)
        {
            if (!string.IsNullOrEmpty(inputHtml))
            {
                var contentHtmlDocument = new HtmlDocument();
                contentHtmlDocument.LoadHtml(HttpUtility.HtmlDecode(inputHtml));
                return contentHtmlDocument.ParsedText.Trim();
            }

            return string.Empty;
        }

        /// <summary>
        ///     Get Layout List Items.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        internal async Task<List<SelectListItem>> BaseGetLayoutListItems()
        {
            var layouts = await dbContext.Layouts.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.LayoutName
            }).ToListAsync();
            if (layouts != null)
            {
                return layouts;
            }

            var layoutViewModel = new LayoutViewModel();

            dbContext.Layouts.Add(layoutViewModel.GetLayout());
            await dbContext.SaveChangesAsync();

            return await dbContext.Layouts.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.LayoutName
            }).ToListAsync();
        }

        /// <summary>
        /// Gets the user ID of the currently logged in user.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task<string> GetUserId()
        {
            // Get the user's ID for logging.
            var user = await baseUserManager.GetUserAsync(User);
            return user.Id;
        }
    }
}