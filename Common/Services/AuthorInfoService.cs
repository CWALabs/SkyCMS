// <copyright file="AuthorInfoService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// EF Core backed implementation of <see cref="IAuthorInfoService"/> that stores author metadata
    /// in the <see cref="ApplicationDbContext.AuthorInfos"/> table. Provides simple caching opportunities
    /// via higher-level callers (this class itself does not cache results).
    /// </summary>
    public sealed class AuthorInfoService : IAuthorInfoService
    {
        private readonly ApplicationDbContext db;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorInfoService"/> class.
        /// </summary>
        /// <param name="db">Application database context.</param>
        public AuthorInfoService(ApplicationDbContext db) =>
            this.db = db ?? throw new ArgumentNullException(nameof(db));

        /// <inheritdoc/>
        public async Task<AuthorInfo> GetOrCreateAsync(Guid userId, Func<string> displayNameFactory = null, CancellationToken cancellationToken = default)
        {
            var key = userId.ToString();
            var existing = await db.AuthorInfos.FirstOrDefaultAsync(a => a.Id == key, cancellationToken);
            if (existing != null) return existing;

            // Attempt to infer display name from ASP.NET Identity Users table if available.
            var identity = await db.Users.FirstOrDefaultAsync(u => u.Id == key, cancellationToken);
            var fallbackName = identity?.UserName ?? identity?.Email ?? key;
            var name = displayNameFactory?.Invoke() ?? fallbackName;

            var author = new AuthorInfo
            {
                Id = key,
                AuthorName = name,
                AuthorDescription = string.Empty
            };

            db.AuthorInfos.Add(author);
            await db.SaveChangesAsync(cancellationToken);
            return author;
        }

        /// <inheritdoc/>
        public Task<AuthorInfo> FindAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var key = userId.ToString();
            return db.AuthorInfos.FirstOrDefaultAsync(a => a.Id == key, cancellationToken)!;
        }

        /// <inheritdoc/>
        public async Task<AuthorInfo> UpdateAsync(AuthorInfo author, CancellationToken cancellationToken = default)
        {
            if (author == null) throw new ArgumentNullException(nameof(author));

            db.AuthorInfos.Update(author);
            await db.SaveChangesAsync(cancellationToken);
            return author;
        }
    }
}