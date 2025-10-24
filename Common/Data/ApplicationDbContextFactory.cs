// <copyright file="ApplicationDbContextFactory.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/SkyCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Data
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    /// <summary>
    /// Design-time factory so EF Core can create the DbContext for migrations
    /// without ambiguity between multiple public constructors.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        /// <inheritdoc/>
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Use the same relational provider you generate migrations for.
            // Adjust the connection string as appropriate for your dev environment.
            optionsBuilder.UseSqlite("Data Source=app.db");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}