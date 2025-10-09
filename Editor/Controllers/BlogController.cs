// <copyright file="BlogController.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Controllers
{
    // UPDATED: sync with enhanced Blog entity & view models (Title, Description, HeroImage, IsDefault, SortOrder)
    // Added mapping & update of UpdatedUtc when editing a blog stream.
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Sky.Editor.Data.Logic;
    using Sky.Editor.Models.Blogs;
    using Sky.Editor.Services.Slugs; // if you place ISlugService elsewhere adjust

    /// <summary>
    /// Editor-facing controller for managing blog streams (multi-blog support) and their entries (blog posts).
    /// </summary>
    /// <remarks>
    /// Responsibilities:
    /// <list type="bullet">
    ///   <item>Create, list, edit, and delete blog streams (<c>Blog</c> records).</item>
    ///   <item>Enforce uniqueness and validation of <c>BlogKey</c> values (route-safe identifiers).</item>
    ///   <item>Maintain a single default blog stream (used as reassignment target).</item>
    ///   <item>Create, edit, publish (immediate), and delete blog post entries via <see cref="ArticleEditLogic"/>.</item>
    ///   <item>Provide JSON listing endpoint for client-side selection widgets.</item>
    ///   <item>Provide an anonymous preview (<see cref="GenericBlogPage"/>) for a specific blog.</item>
    /// </list>
    /// Security:
    /// All actions require authentication via <see cref="AuthorizeAttribute"/> except the preview endpoint which allows anonymous access.
    /// </remarks>
    [Authorize]
    [Route("editor/blogs")]
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ArticleEditLogic articleLogic;
        private readonly ISlugService slugService; // NEW

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogController"/> class.
        /// </summary>
        /// <param name="db">Application database context.</param>
        /// <param name="articleLogic">Article editing / publishing logic service.</param>
        /// <param name="slugService">Slug normalization and uniqueness helper.</param>
        public BlogController(
            ApplicationDbContext db,
            ArticleEditLogic articleLogic,
            ISlugService slugService) // NEW
        {
            this.db = db;
            this.articleLogic = articleLogic;
            this.slugService = slugService;
        }

        /// <summary>
        /// Generates a unique blog key (slug) from a supplied title.
        /// </summary>
        /// <param name="title">Source title text.</param>
        /// <returns>Unique route-safe slug (lowercase, max length 64) not currently in use.</returns>
        private async Task<string> GenerateUniqueBlogKeyAsync(string title)
        {
            // Reuse existing slug normalizer. Fall back if service returns empty.
            var baseSlug = slugService.Normalize(title) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(baseSlug))
                baseSlug = "blog";

            // Trim to max length (64) before uniqueness suffixing
            const int max = 64;
            if (baseSlug.Length > max)
                baseSlug = baseSlug[..max];

            var candidate = baseSlug;
            var i = 2;
            while (await db.Blogs.AnyAsync(b => b.BlogKey == candidate))
            {
                var suffix = "-" + i;
                var cut = Math.Min(baseSlug.Length, max - suffix.Length);
                candidate = baseSlug[..cut] + suffix;
                i++;
            }
            return candidate;
        }

        /// <summary>
        /// Lists all blog streams ordered by sort order then key.
        /// </summary>
        /// <returns>Index view containing a list of <see cref="BlogStreamViewModel"/>.</returns>
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

        /// <summary>
        /// Displays the create blog stream form.
        /// </summary>
        /// <returns>Create view with default model.</returns>
        [HttpGet("create")]
        public IActionResult Create() =>
            View("Create", new BlogStreamViewModel { SortOrder = 0 });

        /// <summary>
        /// Handles blog stream creation.
        /// </summary>
        /// <param name="model">Submitted blog stream view model.</param>
        /// <returns>Redirect to <see cref="Index"/> on success; same view with validation errors otherwise.</returns>
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogStreamViewModel model)
        {
            if (!ModelState.IsValid) return View("Create", model);

            // Auto-generate if user left it blank
            if (string.IsNullOrWhiteSpace(model.BlogKey))
            {
                model.BlogKey = await GenerateUniqueBlogKeyAsync(model.Title ?? "blog");
            }

            var exists = await db.Blogs.AnyAsync(b => b.BlogKey == model.BlogKey);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.BlogKey), "Blog key already exists.");
                return View("Create", model);
            }

            var articleExists = await db.Articles.AnyAsync(a => a.UrlPath.StartsWith(model.BlogKey));
            if (articleExists)
            {
                ModelState.AddModelError(nameof(model.BlogKey), "Blog key conflicts with existing page on this website.");
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

        /// <summary>
        /// Displays edit form for a specified blog stream.
        /// </summary>
        /// <param name="id">Blog identifier (GUID).</param>
        /// <returns>Edit view or 404 if not found.</returns>
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

        /// <summary>
        /// Processes blog stream edits.
        /// </summary>
        /// <param name="id">Route blog identifier.</param>
        /// <param name="model">Edited blog view model.</param>
        /// <returns>Redirect to <see cref="Index"/> on success; edit view with errors otherwise.</returns>
        [HttpPost("{id:guid}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, BlogStreamViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View("Edit", model);

            var blog = await db.Blogs.FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return NotFound();

            // Do NOT silently regenerate BlogKey on title change (stability principle)
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

            blog.BlogKey = model.BlogKey; // user-chosen or previously generated
            blog.Title = model.Title;
            blog.Description = model.Description;
            blog.HeroImage = model.HeroImage ?? string.Empty;
            blog.IsDefault = model.IsDefault;
            blog.SortOrder = model.SortOrder;
            blog.UpdatedUtc = DateTimeOffset.UtcNow;

            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays confirmation page for blog deletion.
        /// </summary>
        /// <param name="id">Blog identifier.</param>
        /// <returns>Delete confirmation view or 404.</returns>
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

        /// <summary>
        /// Performs deletion of a blog stream, optionally reassigning articles to a fallback.
        /// </summary>
        /// <param name="id">Blog identifier.</param>
        /// <param name="reassign">If true, articles are reassigned to default/fallback blog; if false, deletion blocked when articles exist.</param>
        /// <returns>Redirect to <see cref="Index"/> or view with errors.</returns>
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

        /// <summary>
        /// Lists entries (articles) for a specific blog stream.
        /// </summary>
        /// <param name="blogKey">Unique blog key.</param>
        /// <returns>Entries view with listing model or 400/404 on invalid key.</returns>
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

        /// <summary>
        /// Displays create entry form for a given blog.
        /// </summary>
        /// <param name="blogKey">Blog key.</param>
        /// <returns>Create entry view or 404 if blog not found.</returns>
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

        /// <summary>
        /// Handles creation of a new blog entry (article). Automatically publishes if requested.
        /// </summary>
        /// <param name="blogKey">Blog key (must match model).</param>
        /// <param name="model">Entry edit model.</param>
        /// <returns>Redirect to entries list on success; same form on validation errors.</returns>
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

            entity.ArticleType = (int)ArticleType.BlogPost;
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

        /// <summary>
        /// Displays edit form for an existing blog entry (latest version).
        /// </summary>
        /// <param name="blogKey">Blog key.</param>
        /// <param name="articleNumber">Logical article number.</param>
        /// <returns>Edit entry view or 404.</returns>
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

        /// <summary>
        /// Processes edits to a blog entry. May trigger publish if requested.
        /// </summary>
        /// <param name="blogKey">Blog key.</param>
        /// <param name="articleNumber">Article number.</param>
        /// <param name="model">Edited entry model.</param>
        /// <returns>Redirect to entries list on success; same view with errors otherwise.</returns>
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

        /// <summary>
        /// Displays delete confirmation for a blog entry.
        /// </summary>
        /// <param name="blogKey">Blog key.</param>
        /// <param name="articleNumber">Article number.</param>
        /// <returns>Delete entry view or 404.</returns>
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

        /// <summary>
        /// Deletes a blog entry (article) via logic layer.
        /// </summary>
        /// <param name="blogKey">Blog key.</param>
        /// <param name="articleNumber">Article number.</param>
        /// <returns>Redirect to entries listing.</returns>
        [HttpPost("{blogKey}/entries/{articleNumber:int}/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDeleteEntry(string blogKey, int articleNumber)
        {
            var catalog = await db.ArticleCatalog.FirstOrDefaultAsync(c => c.ArticleNumber == articleNumber);
            if (catalog == null || catalog.BlogKey != blogKey) return NotFound();

            await articleLogic.DeleteArticle(articleNumber);
            return RedirectToAction(nameof(Entries), new { blogKey });
        }

        /// <summary>
        /// Anonymous preview page (simplified listing) for a specific blog, returning recent posts.
        /// </summary>
        /// <param name="blogKey">Blog key.</param>
        /// <returns>Preview view with recent posts; 404 if blog not found.</returns>
        [HttpGet("{blogKey}/preview")]
        [AllowAnonymous]
        public async Task<IActionResult> GenericBlogPage(string blogKey)
        {
            var blog = await db.Blogs.FirstOrDefaultAsync(b => b.BlogKey == blogKey);
            if (blog == null)
            {
                return NotFound();
            }

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

        /// <summary>
        /// Returns JSON list of all blog streams (for client-side UI).
        /// </summary>
        /// <returns>JSON array of <see cref="BlogStreamViewModel"/>.</returns>
        [HttpGet("GetBlogs")]
        public async Task<IActionResult> GetBlogs()
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
            return Json(blogs);
        }
    }
}
