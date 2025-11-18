// <copyright file="DataProtectionDbContext.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Data
{
    using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Database context for data protection keys.
    /// </summary>
    public class DataProtectionDbContext : DbContext, IDataProtectionKeyContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataProtectionDbContext"/> class.
        /// </summary>
        /// <param name="options">DB context options.</param>
        public DataProtectionDbContext(DbContextOptions<DataProtectionDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the data protection keys.
        /// </summary>
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    }
}