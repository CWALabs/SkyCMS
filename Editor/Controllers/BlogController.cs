// <copyright file="BlogController.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

// UPDATED: sync with enhanced Blog entity & view models (Title, Description, HeroImage, IsDefault, SortOrder)
// Added mapping & update of UpdatedUtc when editing a blog stream.
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Cosmos.Common.Data;
using Sky.Editor.Models.Blogs;
using Sky.Editor.Data.Logic;

namespace Sky.Editor.Controllers
{
    [Authorize]
    [Route("editor/blogs")]
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ArticleEditLogic articleLogic;
        private const int BlogPostArticleType = 2;

        public BlogController(ApplicationDbContext db, ArticleEditLogic articleLogic)
        {
            this.db = db;
            this.articleLogic = articleLogic;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var blogs = await db.Blogs
                .OrderBy(b => b.SortOrder)
                .ThenBy(b => b.BlogKey)
                .Select(b => new BlogStreamViewModel
                {
                    Id = b.Id,
                    BlogKey = b.BlogKey,
                    Title = b.Title,
                    Description = b.Description,
                    HeroImage = b.HeroImage,
                    IsDefault = b.IsDefault,
                    SortOrder = b.SortOrder
                })
                .ToListAsync();
            return View("Index", blogs);
        }

        [HttpGet("create")]
        public IActionResult Create() =>
            View("Create", new BlogStreamViewModel { SortOrder = 0 });

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogStreamViewModel model)
        {
            if (!ModelState.IsValid) return View("Create", model);

            var exists = await db.Blogs.AnyAsync(b => b.BlogKey == model.BlogKey);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.BlogKey), "Blog key already exists.");
                return View("Create", model);
            }

            if (model.IsDefault)
            {
                // Unset any previous default
                var oldDefaults = await db.Blogs.Where(b => b.IsDefault).ToListAsync();
                foreach (var d in oldDefaults) d.IsDefault = false;
            }

            db.Blogs.Add(new Blog
            {
                BlogKey = model.BlogKey,
                Title = model.Title,
                Description = model.Description,
                HeroImage = model.HeroImage ?? string.Empty,
                IsDefault = model.IsDefault,
                SortOrder = model.SortOrder
            });
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id:guid}/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var blog = await db.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return NotFound();

            return View("Edit", new BlogStreamViewModel
            {
                Id = blog.Id,
                BlogKey = blog.BlogKey,
                Title = blog.Title,
                Description = blog.Description,
                HeroImage = blog.HeroImage,
                IsDefault = blog.IsDefault,
                SortOrder = blog.SortOrder
            });
        }

        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BlogStreamViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View("Edit", model);

            var blog = await db.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return NotFound();

            var duplicateKey = await db.Blogs.AnyAsync(b => b.BlogKey == model.BlogKey && b.Id != id);
            if (duplicateKey)
            {
                ModelState.AddModelError(nameof(model.BlogKey), "Another blog with this key exists.");
                return View("Edit", model);
            }

            if (model.IsDefault && !blog.IsDefault)
            {
                var oldDefaults = await db.Blogs.Where(b => b.IsDefault && b.Id != blog.Id).ToListAsync();
                foreach (var d in oldDefaults) d.IsDefault = false;
            }

            blog.BlogKey = model.BlogKey;
            blog.Title = model.Title;
            blog.Description = model.Description;
            blog.HeroImage = model.HeroImage ?? string.Empty;
            blog.IsDefault = model.IsDefault;
            blog.SortOrder = model.SortOrder;
            blog.UpdatedUtc = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id:guid}/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var blog = await db.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return NotFound();
            return View("Delete", new BlogStreamViewModel
            {
                Id = blog.Id,
                BlogKey = blog.BlogKey,
                Title = blog.Title
            });
        }

        [HttpPost("{id:guid}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDelete(Guid id, bool reassign = true)
        {
            var blog = await db.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return NotFound();

            if (blog.IsDefault)
            {
                ModelState.AddModelError(string.Empty, "Cannot delete the default blog. Make another default first.");
                return View("Delete", new BlogStreamViewModel { Id = blog.Id, BlogKey = blog.BlogKey, Title = blog.Title });
            }

            var hasArticles = await db.Articles.AnyAsync(a => a.BlogKey == blog.BlogKey);
            if (hasArticles && reassign)
            {
                var fallback = await db.Blogs.FirstOrDefaultAsync(b => b.IsDefault && b.Id != blog.Id)
                               ?? await db.Blogs.FirstOrDefaultAsync(b => b.Id != blog.Id);

                if (fallback == null)
                {
                    ModelState.AddModelError(string.Empty, "No fallback blog stream available for reassignment.");
                    return View("Delete", new BlogStreamViewModel { Id = blog.Id, BlogKey = blog.BlogKey, Title = blog.Title });
                }

                var affected = await db.Articles.Where(a => a.BlogKey == blog.BlogKey).ToListAsync();
                foreach (var a in affected) a.BlogKey = fallback.BlogKey;
            }
            else if (hasArticles && !reassign)
            {
                ModelState.AddModelError(string.Empty, "Blog contains articles. Reassign or delete them first.");
                return View("Delete", new BlogStreamViewModel { Id = blog.Id, BlogKey = blog.BlogKey, Title = blog.Title });
            }

            db.Blogs.Remove(blog);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{blogKey}/entries")]
        public async Task<IActionResult> Entries(string blogKey)
        {
            if (string.IsNullOrWhiteSpace(blogKey)) return BadRequest();

            var blog = await db.Blogs.FirstOrDefaultAsync(b => b.BlogKey == blogKey);
            if (blog == null) return NotFound();

            var entries = await db.ArticleCatalog
                .Where(c => c.BlogKey == blogKey)
                .OrderByDescending(c => c.Published ?? c.Updated)
                .Select(c => new BlogEntryListItem
                {
                    BlogKey = c.BlogKey,
                    ArticleNumber = c.ArticleNumber,
                    Title = c.Title,
                    Published = c.Published,
                    Updated = c.Updated,
                    UrlPath = c.UrlPath,
                    Introduction = c.Introduction,
                    BannerImage = c.BannerImage
                })
                .ToListAsync();

            var vm = new BlogEntriesListViewModel
            {
                BlogKey = blog.BlogKey,
                BlogTitle = blog.Title,
                BlogDescription = blog.Description,
                HeroImage = blog.HeroImage,
                Entries = entries
            };
            return View("Entries", vm);
        }

        [HttpGet("{blogKey}/entries/create")]
        public async Task<IActionResult> CreateEntry(string blogKey)
        {
            var blogExists = await db.Blogs.AnyAsync(b => b.BlogKey == blogKey);
            if (!blogExists) return NotFound();

            return View("CreateEntry", new BlogEntryEditViewModel
            {
                BlogKey = blogKey,
                PublishNow = true
            });
        }

        [HttpPost("{blogKey}/entries/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEntry(string blogKey, BlogEntryEditViewModel model)
        {
            if (blogKey != model.BlogKey) return BadRequest();
            var blogExists = await db.Blogs.AnyAsync(b => b.BlogKey == blogKey);
            if (!blogExists) return NotFound();

            if (!ModelState.IsValid) return View("CreateEntry", model);

            var userId = Guid.Parse(User?.Claims?.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value ?? Guid.Empty.ToString());
            var articleVm = await articleLogic.CreateArticle(model.Title, userId, null, blogKey);

            var entity = await db.Articles
                .OrderByDescending(a => a.VersionNumber)
                .FirstOrDefaultAsync(a => a.ArticleNumber == articleVm.ArticleNumber);

            entity.ArticleType = BlogPostArticleType;
            entity.Introduction = model.Introduction ?? string.Empty;
            entity.Content = model.Content ?? entity.Content;
            entity.BannerImage = model.BannerImage ?? string.Empty;

            if (model.PublishNow && entity.Published == null)
            {
                entity.Published = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync();
            await articleLogic.PublishArticle(entity.Id, entity.Published ?? DateTimeOffset.UtcNow);

            return RedirectToAction(nameof(Entries), new { blogKey });
        }

        [HttpGet("{blogKey}/entries/{articleNumber:int}/edit")]
        public async Task<IActionResult> EditEntry(string blogKey, int articleNumber)
        {
            var article = await db.Articles
                .Where(a => a.ArticleNumber == articleNumber)
                .OrderByDescending(a => a.VersionNumber)
                .FirstOrDefaultAsync();

            if (article == null || article.BlogKey != blogKey) return NotFound();

            var vm = new BlogEntryEditViewModel
            {
                BlogKey = blogKey,
                ArticleNumber = article.ArticleNumber,
                Id = article.Id,
                Title = article.Title,
                Introduction = article.Introduction,
                Content = article.Content,
                BannerImage = article.BannerImage,
                PublishNow = article.Published != null
            };
            return View("EditEntry", vm);
        }

        [HttpPost("{blogKey}/entries/{articleNumber:int}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEntry(string blogKey, int articleNumber, BlogEntryEditViewModel model)
        {
            if (model.ArticleNumber != articleNumber || model.BlogKey != blogKey) return BadRequest();
            if (!ModelState.IsValid) return View("EditEntry", model);

            var article = await db.Articles
                .Where(a => a.ArticleNumber == articleNumber)
                .OrderByDescending(a => a.VersionNumber)
                .FirstOrDefaultAsync();
            if (article == null || article.BlogKey != blogKey) return NotFound();

            var userId = Guid.Parse(User?.Claims?.FirstOrDefault(c => c.Type.EndsWith("nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value ?? Guid.Empty.ToString());

            var articleVm = await articleLogic.GetArticleByArticleNumber(articleNumber, article.VersionNumber);
            articleVm.Title = model.Title;
            articleVm.Introduction = model.Introduction;
            articleVm.Content = model.Content;
            articleVm.BannerImage = model.BannerImage;

            await articleLogic.SaveArticle(articleVm, userId);

            if (model.PublishNow)
            {
                if (article.Published == null)
                {
                    await articleLogic.PublishArticle(article.Id, DateTimeOffset.UtcNow);
                }
                else
                {
                    await articleLogic.PublishArticle(article.Id, article.Published);
                }
            }

            return RedirectToAction(nameof(Entries), new { blogKey });
        }

        [HttpGet("{blogKey}/entries/{articleNumber:int}/delete")]
        public async Task<IActionResult> DeleteEntry(string blogKey, int articleNumber)
        {
            var catalog = await db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == articleNumber);
            if (catalog == null || catalog.BlogKey != blogKey) return NotFound();

            var vm = new BlogEntryListItem
            {
                BlogKey = catalog.BlogKey,
                ArticleNumber = catalog.ArticleNumber,
                Title = catalog.Title,
                Published = catalog.Published,
                Updated = catalog.Updated,
                UrlPath = catalog.UrlPath,
                Introduction = catalog.Introduction,
                BannerImage = catalog.BannerImage
            };
            return View("DeleteEntry", vm);
        }

        [HttpPost("{blogKey}/entries/{articleNumber:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteEntry(string blogKey, int articleNumber)
        {
            var catalog = await db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == articleNumber);
            if (catalog == null || catalog.BlogKey != blogKey) return NotFound();

            await articleLogic.DeleteArticle(articleNumber);
            return RedirectToAction(nameof(Entries), new { blogKey });
        }

        [HttpGet("{blogKey}/preview")]
        [AllowAnonymous]
        public async Task<IActionResult> GenericBlogPage(string blogKey)
        {
            var blog = await db.Blogs.FirstOrDefaultAsync(b => b.BlogKey == blogKey);
            if (blog == null) return NotFound();

            var posts = await db.ArticleCatalog
                .Where(c => c.BlogKey == blogKey)
                .OrderByDescending(c => c.Published ?? c.Updated)
                .Take(25)
                .ToListAsync();

            ViewData["BlogTitle"] = blog.Title;
            ViewData["BlogDescription"] = blog.Description;
            ViewData["HeroImage"] = blog.HeroImage;

            return View("GenericBlog", posts);
        }
    }
}
