namespace Sky.Cms.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
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
        private readonly ApplicationDbContext dbContext;
        private const int DefaultPageSize = 10;

        public BlogController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("/blog/{page?}")]
        public async Task<IActionResult> Index(int page = 1, string category = "")
        {
            if (page < 1) page = 1;
            var query = dbContext.Pages
                .Where(p => p.IsBlogPost && p.Published != null && p.Published <= DateTimeOffset.UtcNow);
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
    }
}
