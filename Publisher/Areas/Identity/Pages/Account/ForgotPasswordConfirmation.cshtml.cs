// <copyright file="ForgotPasswordConfirmation.cshtml.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cosmos.Cms.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Forgot password confirmation page model.
    /// </summary>
    [AllowAnonymous]
    public class ForgotPasswordConfirmation : PageModel
    {
        /// <summary>
        /// On get method handler.
        /// </summary>
        public void OnGet()
        {
        }
    }
}
