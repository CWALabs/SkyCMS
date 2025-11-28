namespace Sky.Editor.Features.Articles.Create
{
    using System;
    using Cosmos.Common.Data;
    using Cosmos.Common.Models;
    using Sky.Editor.Features.Shared;

    /// <summary>
    /// Command to create a new article.
    /// </summary>
    public sealed class CreateArticleCommand : ICommand<CommandResult<ArticleViewModel>>
    {
        public string Title { get; init; } = string.Empty;
        public Guid UserId { get; init; }
        public Guid? TemplateId { get; init; }
        public string BlogKey { get; init; } = "default";
        public ArticleType ArticleType { get; init; } = ArticleType.General;
    }
}