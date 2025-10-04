namespace Sky.Cms.Controllers
{
    using System;
    using System.Linq;
    using System.ServiceModel.Syndication;
    using System.Threading.Tasks;
    using System.Xml;
    using Cosmos.Common.Data;
    using Cosmos.Common.Models.Blog;
    using Cosmos.Common.Data.Logic;
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
        private readonly ArticleLogic articleLogic;
        private const int DefaultPageSize = 10;

        public BlogController(ApplicationDbContext dbContext, ArticleLogic articleLogic)
        {
            this.dbContext = dbContext;
            this.articleLogic = articleLogic;
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

        [HttpGet("/blog/post/{*slug}")]
        public async Task<IActionResult> Post(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return NotFound();
            slug = slug.Trim('/');
            var page = await dbContext.Pages.FirstOrDefaultAsync(p => p.UrlPath == slug && p.IsBlogPost && p.Published != null);
            if (page == null) return NotFound();
            var model = await articleLogic.GetPublishedPageByUrl(slug, "");
            if (model == null) return NotFound();
            model.IsBlogPost = true; // ensure flag
            await articleLogic.EnrichBlogNavigation(model);
            return View("~/Views/Blog/Post.cshtml", model);
        }

        [HttpGet("/blog/rss")] 
        public async Task<IActionResult> Rss()
        {
            var baseUrl = Request.Scheme + "://" + Request.Host.Value.TrimEnd('/');
            var items = await dbContext.Pages
                .Where(p => p.IsBlogPost && p.Published != null && p.Published <= DateTimeOffset.UtcNow)
                .OrderByDescending(p => p.Published)
                .Take(20)
                .Select(p => new { p.Title, p.UrlPath, p.Published, p.Introduction, p.BannerImage })
                .ToListAsync();

            var feed = new SyndicationFeed("Blog", "Latest blog posts", new Uri(baseUrl + "/blog"));
            var feedItems = items.Select(p =>
                new SyndicationItem(
                    p.Title,
                    p.Introduction ?? string.Empty,
                    new Uri(baseUrl + "/" + p.UrlPath.TrimStart('/')),
                    p.UrlPath,
                    p.Published ?? DateTimeOffset.UtcNow)
            ).ToList();
            feed.Items = feedItems;
            Response.ContentType = "application/rss+xml";
            using var sw = new System.IO.StringWriter();
            using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true }))
            {
                var rssFormatter = new Rss20FeedFormatter(feed);
                rssFormatter.WriteTo(xmlWriter);
            }
            return Content(sw.ToString(), "application/rss+xml");
        }
    }
}
