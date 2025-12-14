// <copyright file="ResetPasswordConfirmation.cshtml.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Areas.Identity.Pages.Account
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.RazorPages;

    /// <summary>
    /// Reset password confirmation page model.
    /// </summary>
    [AllowAnonymous]
    public class ResetPasswordConfirmationModel : PageModel
    {
        /// <summary>
        /// GET method handler.
        /// </summary>
        public void OnGet()
        {
            // Method intentionally left empty.
        }
    }
}
