namespace Sky.Editor.Features.Articles.Create
{
    using System;
    using Cosmos.Cms.Common;
    using Cosmos.Common.Data;
    using Cosmos.Common.Models;
    using Sky.Editor.Features.Shared;

    /// <summary>
    /// Command to create a new article.
    /// </summary>
    public sealed class CreateArticleCommand : ICommand<CommandResult<ArticleViewModel>>
    {
        /// <summary>
        /// Gets or sets the article title.
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the user ID of the article creator.
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Gets the template ID to use for the new article.
        /// </summary>
        public Guid? TemplateId { get; init; }

        /// <summary>
        /// Gets the blog key where the article will be created.
        /// </summary>
        public string BlogKey { get; init; } = "default";

        /// <summary>
        /// Gets the type of the article.
        /// </summary>
        public ArticleType ArticleType { get; init; } = ArticleType.General;
    }
}
