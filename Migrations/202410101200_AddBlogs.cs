using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddBlogs : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Blogs",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                BlogKey = table.Column<string>(maxLength: 64, nullable: false),
                Title = table.Column<string>(maxLength: 128, nullable: false),
                Description = table.Column<string>(maxLength: 512, nullable: true),
                HeroImage = table.Column<string>(nullable: true),
                IsDefault = table.Column<bool>(nullable: false),
                CreatedUtc = table.Column<DateTimeOffset>(nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Blogs", x => x.Id));

        migrationBuilder.CreateIndex("IX_Blogs_BlogKey", "Blogs", "BlogKey", unique: true);

        migrationBuilder.AddColumn<string>("BlogKey", "Articles", nullable: true);
        migrationBuilder.AddColumn<string>("BlogKey", "Pages", nullable: true);
        migrationBuilder.AddColumn<string>("BlogKey", "ArticleCatalog", nullable: true);

        // Backfill existing data to default
        migrationBuilder.Sql("UPDATE Articles SET BlogKey = 'default' WHERE BlogKey IS NULL;");
        migrationBuilder.Sql("UPDATE Pages SET BlogKey = 'default' WHERE BlogKey IS NULL;");
        migrationBuilder.Sql("UPDATE ArticleCatalog SET BlogKey = 'default' WHERE BlogKey IS NULL;");

        // Seed default blog
        migrationBuilder.Sql(@"INSERT INTO Blogs (Id,BlogKey,Title,Description,HeroImage,IsDefault,CreatedUtc)
                               VALUES (NEWID(),'default','Main Blog','Default blog','',1, SYSUTCDATETIME());");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Blogs");
        migrationBuilder.DropColumn("BlogKey", "Articles");
        migrationBuilder.DropColumn("BlogKey", "Pages");
        migrationBuilder.DropColumn("BlogKey", "ArticleCatalog");
    }
}