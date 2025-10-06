// <copyright file="AuthorInfoService.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Services.Authors
{
    using System;
    using System.Threading.Tasks;
    using Cosmos.Common.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;

    internal sealed class AuthorInfoService : IAuthorInfoService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMemoryCache _cache;

        public AuthorInfoService(ApplicationDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<AuthorInfo> GetOrCreateAsync(Guid userId)
        {
            var key = userId.ToString();
            if (_cache.TryGetValue(key, out AuthorInfo cached))
                return cached;

            var existing = await _db.AuthorInfos.FirstOrDefaultAsync(a => a.Id == key);
            if (existing == null)
            {
                var identity = await _db.Users.FirstOrDefaultAsync(u => u.Id == key);
                if (identity == null) return null;

                existing = new AuthorInfo
                {
                    Id = key,
                    AuthorName = identity.UserName ?? identity.Email ?? key,
                    AuthorDescription = string.Empty
                };
                _db.AuthorInfos.Add(existing);
                await _db.SaveChangesAsync();
            }

            _cache.Set(key, existing, TimeSpan.FromMinutes(10));
            return existing;
        }
    }
}
