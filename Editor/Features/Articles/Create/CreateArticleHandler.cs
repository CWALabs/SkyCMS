namespace Sky.Editor.Features.Articles.Create
{
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Cosmos.Common.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Sky.Editor.Features.Shared;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.Catalog;
    using Sky.Editor.Services.Html;
    using Sky.Editor.Services.Publishing;
    using Sky.Editor.Services.Templates;
    using Sky.Editor.Services.Titles;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles the creation of new articles.
    /// </summary>
    public class CreateArticleHandler : ICommandHandler<CreateArticleCommand, CommandResult<ArticleViewModel>>
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IArticleHtmlService htmlService;
        private readonly ICatalogService catalogService;
        private readonly IPublishingService publishingService;
        private readonly ITitleChangeService titleChangeService;
        private readonly ITemplateService templateService;
        private readonly IClock clock;
        private readonly ILogger<CreateArticleHandler> logger;
        private readonly CreateArticleValidator validator;

        public CreateArticleHandler(
            ApplicationDbContext dbContext,
            IArticleHtmlService htmlService,
            ICatalogService catalogService,
            IPublishingService publishingService,
            ITitleChangeService titleChangeService,
            ITemplateService templateService,
            IClock clock,
            ILogger<CreateArticleHandler> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.htmlService = htmlService ?? throw new ArgumentNullException(nameof(htmlService));
            this.catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
            this.publishingService = publishingService ?? throw new ArgumentNullException(nameof(publishingService));
            this.titleChangeService = titleChangeService ?? throw new ArgumentNullException(nameof(titleChangeService));
            this.templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            validator = new CreateArticleValidator();
        }

        public async Task<CommandResult<ArticleViewModel>> HandleAsync(
            CreateArticleCommand command,
            CancellationToken cancellationToken = default)
        {
            // Validate
            var validationErrors = validator.Validate(command);
            if (validationErrors.Any())
            {
                return CommandResult<ArticleViewModel>.Failure(validationErrors);
            }

            try
            {
                logger.LogInformation(
                    "Creating article '{Title}' for user {UserId}",
                    command.Title,
                    command.UserId);

                var isFirstArticle = await dbContext.Articles.CountAsync(cancellationToken) == 0;
                var defaultTemplate = await GetTemplateContentAsync(command.TemplateId, cancellationToken);

                var nextArticleNumber = await GetNextArticleNumberAsync(isFirstArticle, cancellationToken);

                var title = command.Title.Trim('/');
                var now = clock.UtcNow;

                var article = new Article
                {
                    BlogKey = command.BlogKey,
                    ArticleNumber = nextArticleNumber,
                    ArticleType = (int)command.ArticleType,
                    Content = htmlService.EnsureEditableMarkers(defaultTemplate),
                    StatusCode = (int)StatusCodeEnum.Active,
                    Title = title,
                    Updated = now,
                    VersionNumber = 1,
                    Published = isFirstArticle ? now : null,
                    UserId = command.UserId.ToString(),
                    TemplateId = command.TemplateId,
                    BannerImage = string.Empty
                };

                // Generate URL path
                article.UrlPath = isFirstArticle ? "root" : titleChangeService.BuildArticleUrl(article);

                dbContext.Articles.Add(article);
                dbContext.ArticleNumbers.Add(new ArticleNumber { LastNumber = nextArticleNumber });

                await dbContext.SaveChangesAsync(cancellationToken);

                // Update catalog
                await catalogService.UpsertAsync(article, cancellationToken);

                // Auto-publish first article
                if (isFirstArticle)
                {
                    await publishingService.PublishAsync(article);
                }

                logger.LogInformation(
                    "Successfully created article {ArticleNumber} with title '{Title}'",
                    article.ArticleNumber,
                    article.Title);

                // Build view model (simplified - you'll need ArticleLogic or similar)
                var viewModel = new ArticleViewModel
                {
                    Id = article.Id,
                    ArticleNumber = article.ArticleNumber,
                    Title = article.Title,
                    Content = article.Content,
                    UrlPath = article.UrlPath,
                    Published = article.Published,
                    Updated = article.Updated,
                    VersionNumber = article.VersionNumber,
                    StatusCode = (StatusCodeEnum)article.StatusCode,
                    ArticleType = command.ArticleType,
                    BannerImage = article.BannerImage
                };

                return CommandResult<ArticleViewModel>.Success(viewModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating article '{Title}'", command.Title);
                return CommandResult<ArticleViewModel>.Failure("An error occurred while creating the article.");
            }
        }

        private async Task<string> GetTemplateContentAsync(Guid? templateId, CancellationToken cancellationToken)
        {
            if (!templateId.HasValue)
            {
                return GetDefaultLoremIpsumContent();
            }

            var template = await dbContext.Templates
                .FirstOrDefaultAsync(f => f.Id == templateId.Value, cancellationToken);

            if (template == null)
            {
                return GetDefaultLoremIpsumContent();
            }

            var content = htmlService.EnsureEditableMarkers(template.Content);
            if (!content.Equals(template.Content))
            {
                template.Content = content;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return template.Content;
        }

        private async Task<int> GetNextArticleNumberAsync(bool isFirstArticle, CancellationToken cancellationToken)
        {
            if (isFirstArticle)
            {
                return 1;
            }

            return await dbContext.ArticleNumbers.MaxAsync(m => m.LastNumber, cancellationToken) + 1;
        }

        private static string GetDefaultLoremIpsumContent() =>
            "<div style='width: 100%;padding-left: 20px;padding-right: 20px;margin-left: auto;margin-right: auto;'>" +
            "<div contenteditable='true'><h1>Why Lorem Ipsum?</h1><p>" +
            LoremIpsum.WhyLoremIpsum + "</p></div></div>";
    }
}