// <copyright file="PubController.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Controllers
{
    using Cosmos.BlobService;
    using Cosmos.Common.Data;
    using Cosmos.Publisher.Controllers;
    using Microsoft.AspNetCore.Mvc;
    using Sky.Editor.Data.Logic;

    /// <summary>
    /// Secure file access controller and proxy.
    /// </summary>
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class PubController : PubControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PubController"/> class.
        /// </summary>
        /// <param name="dbContext">Database context.</param>
        /// <param name="storageContext">Storage context.</param>
        /// <param name="options">Editor settings.</param>
        public PubController(ApplicationDbContext dbContext, StorageContext storageContext, IEditorSettings options)
            : base(dbContext, storageContext, options.CosmosRequiresAuthentication)
        {
        }
    }
}
