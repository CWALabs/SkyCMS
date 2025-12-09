// <copyright file="SetupDbContext.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the MIT License (https://opensource.org/licenses/MIT)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Sky.Editor.Data
{
    using Cosmos.Common.Data;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// SQLite database context for storing setup wizard progress.
    /// This database is ephemeral and deleted after successful setup.
    /// </summary>
    public class SetupDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetupDbContext"/> class.
        /// </summary>
        /// <param name="options">Database context options.</param>
        public SetupDbContext(DbContextOptions<SetupDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the setup configuration records.
        /// </summary>
        public DbSet<SetupConfiguration> SetupConfigurations { get; set; }

        /// <summary>
        /// Configure the model.
        /// </summary>
        /// <param name="modelBuilder">Model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SetupConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TenantMode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PublisherUrl).IsRequired();
                entity.Property(e => e.AdminEmail).IsRequired();
                entity.Property(e => e.DatabaseConnectionString).IsRequired();
                entity.Property(e => e.StorageConnectionString).IsRequired();
            });
        }
    }
}