using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cosmos.Common.Data
{
    /// <summary>
    /// Design-time factory so EF Core can create the DbContext for migrations
    /// without ambiguity between multiple public constructors.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
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