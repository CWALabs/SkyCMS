// <copyright file="BlogController.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Cms.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Cosmos.Common.Models.Blog;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Simple blog listing controller (read-only) for editor environment.
    /// </summary>
    [AllowAnonymous]
    public class BlogController : Controller
    {
        private const int DefaultPageSize = 10;
        private readonly ApplicationDbContext dbContext;
        private readonly ArticleLogic articleLogic;

        /// <summary>
        ///  Initializes a new instance of the <see cref="BlogController"/> class.
        /// </summary>
        /// <param name="dbContext">Database context.</param>
        /// <param name="articleLogic">Article logic.</param>
        public BlogController(ApplicationDbContext dbContext, ArticleLogic articleLogic)
        {
            this.dbContext = dbContext;
            this.articleLogic = articleLogic;
        }

        /// <summary>
        ///  Index action - blog listing with optional category filter.
        /// </summary>
        /// <param name="page">Page number.</param>
        /// <param name="category">Category filter.</param>
        /// <returns></returns>
        [HttpGet("/blog/{page?}")]
        public async Task<IActionResult> Index(int page = 1, string category = "")
        {
            if (page < 1)
            {
                page = 1;
            }

            var blogType = (int)ArticleType.BlogPost;

            var query = dbContext.Pages
                .Where(p => p.ArticleType == blogType && p.Published != null && p.Published <= DateTimeOffset.UtcNow);
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p => p.Category == category);
            }
            var total = await query.CountAsync();
            var posts = await query
                .OrderByDescending(p => p.Published)
                .Skip((page - 1) * DefaultPageSize)
                .Take(DefaultPageSize)
                .Select(p => new BlogListItem
                {
                    Id = p.Id,
                    ArticleNumber = p.ArticleNumber,
                    Title = p.Title,
                    UrlPath = p.UrlPath,
                    Published = p.Published,
                    BannerImage = p.BannerImage,
                    Introduction = p.Introduction,
                    Category = p.Category
                }).ToListAsync();

            var model = new BlogIndexViewModel
            {
                Posts = posts,
                Page = page,
                PageSize = DefaultPageSize,
                TotalPages = (int)Math.Ceiling(total / (double)DefaultPageSize),
                Category = category ?? string.Empty
            };
            return View(model);
        }

        /// <summary>
        ///  Post a blog article by slug.
        /// </summary>
        /// <param name="slug">Slug.</param>
        /// <returns>IActionResult.</returns>
        [HttpGet("/blog/post/{*slug}")]
        public async Task<IActionResult> Post(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return NotFound();
            }

            slug = slug.Trim('/');

            var blogType = (int)ArticleType.BlogPost;
            var page = await dbContext.Pages.FirstOrDefaultAsync(p => p.UrlPath == slug && p.ArticleType == blogType && p.Published != null);
            if (page == null)
            {
                return NotFound();
            }

            var model = await articleLogic.GetPublishedPageByUrl(slug, string.Empty);
            if (model == null)
            {
                return NotFound();
            }

            model.ArticleType = ArticleType.BlogPost; // ensure flag
            await articleLogic.EnrichBlogNavigation(model);
            return View("~/Views/Blog/Post.cshtml", model);
        }
    }
}
