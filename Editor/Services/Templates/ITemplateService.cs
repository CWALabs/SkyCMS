// <copyright file="ITemplateService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/CWALabs/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Templates
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;

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

        /// <summary>
        /// Gets all the versions for a page design template.
        /// </summary>
        /// <param name="key">Template ID.</param>
        /// <returns>A list of versions.</returns>
        Task<List<PageDesignVersion>> GetTemplateDesignVersionsAsync(string key);

        /// <summary>
        /// Gets the latest unpublished version of a template for editing.
        /// </summary>
        /// <param name="key">Template ID.</param>
        /// <returns>Page template design version.</returns>
        Task<PageDesignVersion> GetVersionForEdit(string key);

        /// <summary>
        /// Gets a specific page design (template) version.
        /// </summary>
        /// <param name="id">Template ID.</param>
        /// <returns>Page template design version.</returns>
        Task<PageDesignVersion> GetVersion(string id);

        /// <summary>
        /// Saves a page design version.
        /// </summary>
        /// <param name="model">Version model.</param>
        /// <returns>Task.</returns>
        Task Save(PageDesignVersion model);

        /// <summary>
        /// Saves and publishes a page design model.
        /// </summary>
        /// <param name="model">Version model.</param>
        /// <remarks>
        /// This method accomplishes the following:
        /// <list type="bullet">
        /// <item>Saves the design.</item>
        /// <item>Applies the design to pages that use it.</item>
        /// <item>Republishes those pages who use the new design.</item>
        /// </list>
        /// </remarks>
        /// <returns>Task.</returns>
        Task Publish(PageDesignVersion model);
    }
}
