// <copyright file="SaveArticleHandler.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Features.Articles.Save
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Cosmos.Common.Models;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Sky.Editor.Features.Shared;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.Catalog;
    using Sky.Editor.Services.CDN;
    using Sky.Editor.Services.Html;
    using Sky.Editor.Services.Publishing;
    using Sky.Editor.Services.Titles;

    /// <summary>
    /// Handles saving (updating) existing articles with full workflow coordination.
    /// </summary>
    public class SaveArticleHandler : ICommandHandler<SaveArticleCommand, CommandResult<ArticleUpdateResult>>
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IArticleHtmlService htmlService;
        private readonly ICatalogService catalogService;
        private readonly IPublishingService publishingService;
        private readonly ITitleChangeService titleChangeService;
        private readonly IClock clock;
        private readonly ILogger<SaveArticleHandler> logger;
        private readonly SaveArticleValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveArticleHandler"/> class.
        /// </summary>
        public SaveArticleHandler(
            ApplicationDbContext dbContext,
            IArticleHtmlService htmlService,
            ICatalogService catalogService,
            IPublishingService publishingService,
            ITitleChangeService titleChangeService,
            IClock clock,
            ILogger<SaveArticleHandler> logger)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.htmlService = htmlService ?? throw new ArgumentNullException(nameof(htmlService));
            this.catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
            this.publishingService = publishingService ?? throw new ArgumentNullException(nameof(publishingService));
            this.titleChangeService = titleChangeService ?? throw new ArgumentNullException(nameof(titleChangeService));
            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            validator = new SaveArticleValidator();
        }

        /// <summary>
        /// Handles the save article command.
        /// </summary>
        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
        public async Task<CommandResult<ArticleUpdateResult>> HandleAsync(
            SaveArticleCommand command,
            CancellationToken cancellationToken = default)
        {
            // Validate
            var validationErrors = validator.Validate(command);
            if (validationErrors.Any())
            {
                return CommandResult<ArticleUpdateResult>.Failure(validationErrors);
            }

            try
            {
                logger.LogInformation(
                    "Saving article {ArticleNumber} '{Title}' by user {UserId}",
                    command.ArticleNumber,
                    command.Title,
                    command.UserId);

                // Get latest version of the article
                var article = await dbContext.Articles
                    .Where(a => a.ArticleNumber == command.ArticleNumber)
                    .OrderByDescending(o => o.VersionNumber)
                    .FirstOrDefaultAsync(cancellationToken);

                if (article == null)
                {
                    logger.LogWarning("Article {ArticleNumber} not found", command.ArticleNumber);
                    return CommandResult<ArticleUpdateResult>.Failure($"Article {command.ArticleNumber} not found.");
                }

                // ✅ FIX: Capture BOTH old title AND old URL path BEFORE making changes
                var oldTitle = article.Title;
                var oldUrlPath = article.UrlPath;

                // Process HTML content
                var processedContent = htmlService.EnsureEditableMarkers(command.Content);
                htmlService.EnsureAngularBase(command.HeadJavaScript ?? string.Empty, command.UrlPath ?? article.UrlPath);

                // Update article properties
                article.Content = processedContent;
                article.Title = command.Title.Trim();  // ← This changes the title
                article.Updated = clock.UtcNow;
                article.HeaderJavaScript = command.HeadJavaScript ?? string.Empty;
                article.FooterJavaScript = command.FooterJavaScript ?? string.Empty;
                article.BannerImage = command.BannerImage ?? string.Empty;
                article.UserId = command.UserId.ToString();
                article.ArticleType = (int)command.ArticleType;
                article.Category = command.Category ?? string.Empty;
                article.Published = command.Published;

                if (!string.IsNullOrWhiteSpace(command.Introduction))
                {
                    article.Introduction = command.Introduction;
                }

                // Save with concurrency handling
                var saved = await SaveWithRetryAsync(article, cancellationToken);
                if (!saved)
                {
                    return CommandResult<ArticleUpdateResult>.Failure("Failed to save article due to concurrent modification.");
                }

                // ✅ FIX: Handle title change with BOTH old title and old URL path
                if (!oldTitle.Equals(article.Title))
                {
                    logger.LogInformation(
                        "Title changed from '{OldTitle}' to '{NewTitle}' for article {ArticleNumber}",
                        oldTitle,
                        article.Title,
                        article.ArticleNumber);

                    await titleChangeService.HandleTitleChangeAsync(article, oldTitle, oldUrlPath);
                }

                // Update catalog
                await catalogService.UpsertAsync(article, cancellationToken);

                // Publish if needed
                var cdnResults = new List<CdnResult>();
                if (article.Published.HasValue)
                {
                    cdnResults = await publishingService.PublishAsync(article, cancellationToken);
                }

                logger.LogInformation(
                    "Successfully saved article {ArticleNumber} version {VersionNumber}",
                    article.ArticleNumber,
                    article.VersionNumber);

                // Build result
                var viewModel = MapToViewModel(article, command);
                var result = new ArticleUpdateResult
                {
                    ServerSideSuccess = true,
                    Model = viewModel,
                    CdnResults = cdnResults
                };

                return CommandResult<ArticleUpdateResult>.Success(result);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error saving article {ArticleNumber} '{Title}'",
                    command.ArticleNumber,
                    command.Title);

                return CommandResult<ArticleUpdateResult>.Failure("An error occurred while saving the article.");
            }
        }

        /// <summary>
        /// Saves the article with retry logic for concurrency conflicts.
        /// </summary>
        private async Task<bool> SaveWithRetryAsync(Article article, CancellationToken cancellationToken)
        {
            for (int attempt = 0; attempt < 2; attempt++)
            {
                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    return true;
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (attempt == 1)
                    {
                        logger.LogError(ex, "Concurrency conflict saving article {ArticleNumber}", article.ArticleNumber);
                        throw;
                    }

                    logger.LogWarning("Concurrency conflict on attempt {Attempt}, retrying...", attempt + 1);
                    await dbContext.Entry(article).ReloadAsync(cancellationToken);
                }
            }

            return false;
        }

        /// <summary>
        /// Maps Article entity to ArticleViewModel.
        /// </summary>
        private static ArticleViewModel MapToViewModel(Article article, SaveArticleCommand command)
        {
            return new ArticleViewModel
            {
                Id = article.Id,
                ArticleNumber = article.ArticleNumber,
                Title = article.Title,
                Content = article.Content,
                UrlPath = article.UrlPath,
                HeadJavaScript = article.HeaderJavaScript,
                FooterJavaScript = article.FooterJavaScript,
                BannerImage = article.BannerImage,
                ArticleType = command.ArticleType,
                Category = article.Category,
                Introduction = article.Introduction,
                Published = article.Published,
                Updated = article.Updated,
                VersionNumber = article.VersionNumber,
                StatusCode = (StatusCodeEnum)article.StatusCode
            };
        }
    }
}