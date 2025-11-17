// <copyright file="TemplateService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Templates
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Service for managing and retrieving page templates.
    /// </summary>
    public interface ITemplateService
    {
        /// <summary>
        /// Ensures that default templates exist for the current default layout.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task EnsureDefaultTemplatesExistAsync();

        /// <summary>
        /// Gets all available templates.
        /// </summary>
        /// <returns>A page template list.</returns>
        Task<List<PageTemplate>> GetAllTemplatesAsync();

        /// <summary>
        /// Gets templates by category.
        /// </summary>
        /// <param name="category">The category to filter templates.</param>
        /// <returns>A page template list.</returns>
        Task<List<PageTemplate>> GetTemplatesByCategoryAsync(string category);

        /// <summary>
        /// Gets a specific template by its key.
        /// </summary>
        /// <param name="key">The unique key of the template.</param>
        /// <returns>A page template.</returns>
        Task<PageTemplate> GetTemplateByKeyAsync(string key);

        /// <summary>
        /// Gets the HTML content of a template.
        /// </summary>
        /// <param name="key">The unique key of the template.</param>
        /// <returns>HTML contents of a template.</returns>
        Task<string> GetTemplateContentAsync(string key);

        /// <summary>
        /// Searches templates by name or tags.
        /// </summary>
        /// <param name="searchTerm">The search term to filter templates.</param>
        /// <returns>A page template list.</returns>
        Task<List<PageTemplate>> SearchTemplatesAsync(string searchTerm);
    }
}