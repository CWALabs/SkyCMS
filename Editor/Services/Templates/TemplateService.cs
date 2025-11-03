// <copyright file="TemplateService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Templates
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Implementation of the template service.
    /// </summary>
    public class TemplateService : ITemplateService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<TemplateService> _logger;
        private readonly ApplicationDbContext dbContext;
        private List<PageTemplate>? _cachedTemplates;
        private readonly SemaphoreSlim _lock = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateService"/> class.
        /// </summary>
        /// <param name="environment">The web hosting environment.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="dbContext">The database context.</param>
        public TemplateService(IWebHostEnvironment environment, ILogger<TemplateService> logger, ApplicationDbContext dbContext)
        {
            _environment = environment;
            _logger = logger;
            this.dbContext = dbContext;
        }

        /// <inheritdoc/>
        public async Task EnsureDefaultTemplatesExistAsync()
        {
            var allTemplates = await GetAllTemplatesAsync();
            var defaultLayout = await dbContext.Layouts.FirstOrDefaultAsync(l => l.IsDefault == true);
            var layoutId = defaultLayout?.Id;

            foreach (var template in allTemplates)
            {
                var dbTemplate = await dbContext.Templates.FirstOrDefaultAsync(t => t.LayoutId == layoutId && t.Title == template.Name);
                if (dbTemplate == null)
                {
                    var html = await LoadTemplateContentAsync(template.FilePath);
                    dbTemplate = new Template
                    {
                        Id = Guid.NewGuid(),
                        Title = template.Name,
                        Description = template.Description,
                        PageType = template.Key,
                        Content = html,
                        LayoutId = layoutId,
                        CommunityLayoutId = defaultLayout?.CommunityLayoutId
                    };
                    dbContext.Templates.Add(dbTemplate);
                }
            }
            await dbContext.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<List<PageTemplate>> GetAllTemplatesAsync()
        {
            if (_cachedTemplates != null)
            {
                return _cachedTemplates;
            }

            await _lock.WaitAsync();
            try
            {
                if (_cachedTemplates != null)
                {
                    return _cachedTemplates;
                }

                _cachedTemplates = GetStandardTemplates();
                return _cachedTemplates;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<List<PageTemplate>> GetTemplatesByCategoryAsync(string category)
        {
            var allTemplates = await GetAllTemplatesAsync();
            return allTemplates
                .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <inheritdoc/>
        public async Task<PageTemplate> GetTemplateByKeyAsync(string key)
        {
            var allTemplates = await GetAllTemplatesAsync();
            var template = allTemplates.FirstOrDefault(t => t.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

            if (template != null && string.IsNullOrEmpty(template.Content))
            {
                template.Content = await LoadTemplateContentAsync(template.FilePath);
            }

            return template;
        }

        /// <inheritdoc/>
        public async Task<string> GetTemplateContentAsync(string key)
        {
            var template = await GetTemplateByKeyAsync(key);
            return template?.Content;
        }

        /// <inheritdoc/>
        public async Task<List<PageTemplate>> SearchTemplatesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetAllTemplatesAsync();
            }

            var allTemplates = await GetAllTemplatesAsync();
            var lowerSearch = searchTerm.ToLower();

            return allTemplates
                .Where(t =>
                    t.Name.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase) ||
                    t.Description.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase) ||
                    t.Tags.Any(tag => tag.Contains(lowerSearch, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        private async Task<string> LoadTemplateContentAsync(string filePath)
        {
            try
            {
                // Try embedded resource first
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"{assembly.GetName().Name}.Templates.{filePath.Replace('/', '.').Replace('\\', '.')}";

                await using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    using var reader = new StreamReader(stream);
                    return await reader.ReadToEndAsync();
                }

                // Fallback to file system
                var physicalPath = Path.Combine(_environment.ContentRootPath, "Templates", filePath);
                if (File.Exists(physicalPath))
                {
                    return await File.ReadAllTextAsync(physicalPath);
                }

                _logger.LogWarning("Template file not found: {FilePath}", filePath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading template: {FilePath}", filePath);
                return null;
            }
        }

        private List<PageTemplate> GetStandardTemplates()
        {
            return new List<PageTemplate>
            {
                new PageTemplate
                {
                    Key = "blog-stream",
                    Name = "Blog Stream",
                    Description = "Standard blog stream layout with featured image, author info, and comment section.",
                    Category = "Blog",
                    FilePath = "PageTemplates/blog-stream.html",
                    ThumbnailPath = "/images/templates/blog-stream-thumb.png",
                    Tags = new List<string> { "blog", "article", "post", "content" }
                },
                new PageTemplate
                {
                    Key = "blog-post",
                    Name = "Blog Post",
                    Description = "Standard blog post layout with featured image, author info, and comment section.",
                    Category = "Blog",
                    FilePath = "PageTemplates/blog-post.html",
                    ThumbnailPath = "/images/templates/blog-post-thumb.png",
                    Tags = new List<string> { "blog", "article", "post", "content" }
                },
                // TODO: Add more templates as needed
                //new PageTemplate
                //{
                //    Key = "landing-page",
                //    Name = "Landing Page",
                //    Description = "Conversion-focused landing page with hero section, features, and call-to-action.",
                //    Category = "Marketing",
                //    FilePath = "PageTemplates/landing-page.html",
                //    ThumbnailPath = "/images/templates/landing-page-thumb.png",
                //    Tags = new List<string> { "landing", "marketing", "conversion", "cta" }
                //},
                //new PageTemplate
                //{
                //    Key = "about-page",
                //    Name = "About Us",
                //    Description = "Professional about page with team section, company history, and values.",
                //    Category = "Corporate",
                //    FilePath = "PageTemplates/about-page.html",
                //    ThumbnailPath = "/images/templates/about-page-thumb.png",
                //    Tags = new List<string> { "about", "team", "company", "corporate" }
                //},
                //new PageTemplate
                //{
                //    Key = "contact-page",
                //    Name = "Contact Form",
                //    Description = "Contact page with form, location map, and contact details.",
                //    Category = "General",
                //    FilePath = "PageTemplates/contact-page.html",
                //    ThumbnailPath = "/images/templates/contact-page-thumb.png",
                //    Tags = new List<string> { "contact", "form", "support" },
                //    RequiresConfiguration = true
                //},
                //new PageTemplate
                //{
                //    Key = "product-showcase",
                //    Name = "Product Showcase",
                //    Description = "E-commerce style product display with image gallery and specifications.",
                //    Category = "E-commerce",
                //    FilePath = "PageTemplates/product-showcase.html",
                //    ThumbnailPath = "/images/templates/product-thumb.png",
                //    Tags = new List<string> { "product", "ecommerce", "showcase", "gallery" }
                //},
                //new PageTemplate
                //{
                //    Key = "blank",
                //    Name = "Blank Page",
                //    Description = "Empty page to start from scratch.",
                //    Category = "General",
                //    FilePath = "PageTemplates/blank.html",
                //    ThumbnailPath = "/images/templates/blank-thumb.png",
                //    Tags = new List<string> { "blank", "empty", "custom" }
                //}
            };
        }
    }
}