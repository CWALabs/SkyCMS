// <copyright file="RedirectService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Redirects
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Cosmos.Common.Data.Logic;
    using Microsoft.EntityFrameworkCore;
    using Sky.Editor.Infrastructure.Time;
    using Sky.Editor.Services.Slugs;

    /// <summary>
    /// Implements redirect management by storing redirects as special <see cref="Article"/> records (StatusCode = Redirect).
    /// </summary>
    public sealed class RedirectService : IRedirectService
    {
        private readonly ApplicationDbContext db;
        private readonly ISlugService slugs;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectService"/> class.
        /// </summary>
        /// <param name="db">EF Core database context.</param>
        /// <param name="slugs">Slug normalization service.</param>
        /// <param name="clock">Clock abstraction for consistent timestamps.</param>
        public RedirectService(ApplicationDbContext db, ISlugService slugs, IClock clock)
        {
            this.db = db;
            this.slugs = slugs;
            this.clock = clock;
        }

        /// <inheritdoc/>
        public async Task<Article> CreateOrUpdateRedirectAsync(string fromSlug, string toSlug, Guid userId)
        {
            fromSlug = slugs.Normalize(fromSlug);
            toSlug = slugs.Normalize(toSlug);

            // Do not redirect from root; silently ignore.
            if (fromSlug == "root") return null;

            var existing = await db.Articles
                .Where(a => a.UrlPath == fromSlug && a.StatusCode == (int)StatusCodeEnum.Redirect)
                .OrderByDescending(a => a.VersionNumber)
                .FirstOrDefaultAsync();

            if (existing != null)
            {
                existing.Content = $"Redirect to {toSlug}";
                existing.RedirectTarget = toSlug;
                if (!existing.Published.HasValue)
                {
                    existing.Published = clock.UtcNow;
                }
                existing.Updated = clock.UtcNow;
                await db.SaveChangesAsync();
                return existing;
            }

            // Allocate a new redirect article (new article number sequence entry).
            var maxArticleNumber = await db.ArticleNumbers.MaxAsync(m => m.LastNumber);
            var redirect = new Article
            {
                ArticleNumber = maxArticleNumber + 1,
                StatusCode = (int)StatusCodeEnum.Redirect,
                UrlPath = fromSlug,
                Title = fromSlug,
                Content = $"Redirect to {toSlug}",
                RedirectTarget = toSlug,
                Published = clock.UtcNow,
                Updated = clock.UtcNow,
                VersionNumber = 1,
                UserId = userId.ToString(),
                BannerImage = string.Empty
            };

            db.Articles.Add(redirect);
            db.ArticleNumbers.Add(new ArticleNumber { LastNumber = redirect.ArticleNumber });
            await db.SaveChangesAsync();
            return redirect;
        }
    }
}