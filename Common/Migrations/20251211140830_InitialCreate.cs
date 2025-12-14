using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cosmos.Common.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticleCatalog",
                columns: table => new
                {
                    ArticleNumber = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AuthorInfo = table.Column<string>(type: "TEXT", nullable: true),
                    BannerImage = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Introduction = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    Updated = table.Column<long>(type: "INTEGER", nullable: false),
                    Published = table.Column<long>(type: "INTEGER", nullable: true),
                    UrlPath = table.Column<string>(type: "TEXT", maxLength: 1999, nullable: true),
                    TemplateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<long>(type: "INTEGER", nullable: true),
                    BlogKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleCatalog", x => x.ArticleNumber);
                });

            migrationBuilder.CreateTable(
                name: "ArticleLocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConnectionId = table.Column<string>(type: "TEXT", nullable: true),
                    UserEmail = table.Column<string>(type: "TEXT", nullable: true),
                    ArticleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LockSetDateTime = table.Column<long>(type: "INTEGER", nullable: false),
                    EditorType = table.Column<string>(type: "TEXT", nullable: true),
                    FilePath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleLocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IdentityUserId = table.Column<string>(type: "TEXT", nullable: true),
                    ArticleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArticleTitle = table.Column<string>(type: "TEXT", nullable: true),
                    ActivityNotes = table.Column<string>(type: "TEXT", nullable: true),
                    DateTimeStamp = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticleNumbers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    SetDateTime = table.Column<long>(type: "INTEGER", nullable: false),
                    LastNumber = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleNumbers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArticleNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    UrlPath = table.Column<string>(type: "TEXT", maxLength: 1999, nullable: true),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Published = table.Column<long>(type: "INTEGER", nullable: true),
                    Expires = table.Column<long>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 254, nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Updated = table.Column<long>(type: "INTEGER", nullable: false),
                    HeaderJavaScript = table.Column<string>(type: "TEXT", nullable: true),
                    FooterJavaScript = table.Column<string>(type: "TEXT", nullable: true),
                    BannerImage = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    TemplateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ArticleType = table.Column<int>(type: "INTEGER", nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Introduction = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    RedirectTarget = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    RowVersion = table.Column<long>(type: "INTEGER", nullable: true),
                    BlogKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<long>(type: "INTEGER", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthorInfos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorName = table.Column<string>(type: "TEXT", nullable: true),
                    AuthorDescription = table.Column<string>(type: "TEXT", nullable: true),
                    TwitterHandle = table.Column<string>(type: "TEXT", nullable: true),
                    InstagramUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Website = table.Column<string>(type: "TEXT", nullable: true),
                    EmailAddress = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: false),
                    Created = table.Column<long>(type: "INTEGER", nullable: false),
                    Updated = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FriendlyName = table.Column<string>(type: "TEXT", nullable: true),
                    Xml = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Layouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: true),
                    CommunityLayoutId = table.Column<string>(type: "TEXT", nullable: true),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    LayoutName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Head = table.Column<string>(type: "TEXT", nullable: true),
                    BodyHtmlAttributes = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    HtmlHeader = table.Column<string>(type: "TEXT", nullable: true),
                    FooterHtmlContent = table.Column<string>(type: "TEXT", nullable: true),
                    LastModified = table.Column<long>(type: "INTEGER", nullable: true),
                    Published = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Metrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeStamp = table.Column<long>(type: "INTEGER", nullable: false),
                    BlobStorageBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    BlobStorageEgressBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    BlobStorageIngressBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    DatabaseDataUsageBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    DatabaseIndexUsageBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    DatabaseRuUsage = table.Column<long>(type: "INTEGER", nullable: false),
                    FrontDoorRequestBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    FrontDoorResponseBytes = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArticleNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    UrlPath = table.Column<string>(type: "TEXT", maxLength: 1999, nullable: true),
                    ParentUrlPath = table.Column<string>(type: "TEXT", maxLength: 1999, nullable: true),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Published = table.Column<long>(type: "INTEGER", nullable: true),
                    Expires = table.Column<long>(type: "INTEGER", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 254, nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Updated = table.Column<long>(type: "INTEGER", nullable: false),
                    BannerImage = table.Column<string>(type: "TEXT", nullable: false),
                    HeaderJavaScript = table.Column<string>(type: "TEXT", nullable: true),
                    FooterJavaScript = table.Column<string>(type: "TEXT", nullable: true),
                    AuthorInfo = table.Column<string>(type: "TEXT", nullable: true),
                    TemplateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    Introduction = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    RowVersion = table.Column<long>(type: "INTEGER", nullable: true),
                    ArticleType = table.Column<int>(type: "INTEGER", nullable: true),
                    BlogKey = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Group = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LayoutId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CommunityLayoutId = table.Column<string>(type: "TEXT", nullable: true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    PageType = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TotpTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Token = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<long>(type: "INTEGER", nullable: false),
                    ExpiresAt = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TotpTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WebsiteAuthor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "TEXT", nullable: true),
                    EmailAddress = table.Column<string>(type: "TEXT", nullable: true),
                    Path = table.Column<string>(type: "TEXT", nullable: true),
                    TemplateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    TemplateName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebsiteAuthor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArticlePermission",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdentityObjectId = table.Column<string>(type: "TEXT", nullable: true),
                    Permission = table.Column<string>(type: "TEXT", nullable: true),
                    IsRoleObject = table.Column<bool>(type: "INTEGER", nullable: false),
                    CatalogEntryArticleNumber = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticlePermission", x => x.ArticleId);
                    table.ForeignKey(
                        name: "FK_ArticlePermission_ArticleCatalog_CatalogEntryArticleNumber",
                        column: x => x.CatalogEntryArticleNumber,
                        principalTable: "ArticleCatalog",
                        principalColumn: "ArticleNumber");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleCatalog_ArticleNumber",
                table: "ArticleCatalog",
                column: "ArticleNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleLocks_Id",
                table: "ArticleLocks",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleLogs_Id",
                table: "ArticleLogs",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticleNumbers_Id",
                table: "ArticleNumbers",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticlePermission_ArticleId",
                table: "ArticlePermission",
                column: "ArticleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArticlePermission_CatalogEntryArticleNumber",
                table: "ArticlePermission",
                column: "CatalogEntryArticleNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_Id",
                table: "Articles",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_Id",
                table: "AspNetRoleClaims",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_Id",
                table: "AspNetRoles",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_Id",
                table: "AspNetUserClaims",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_LoginProvider_ProviderKey",
                table: "AspNetUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId_RoleId",
                table: "AspNetUserRoles",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Id",
                table: "AspNetUsers",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserTokens_UserId_LoginProvider_Name",
                table: "AspNetUserTokens",
                columns: new[] { "UserId", "LoginProvider", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorInfos_Id",
                table: "AuthorInfos",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_Id",
                table: "Contacts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Layouts_Id",
                table: "Layouts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Metrics_Id",
                table: "Metrics",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pages_Id",
                table: "Pages",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Id",
                table: "Settings",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Templates_Id",
                table: "Templates",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TotpTokens_Id",
                table: "TotpTokens",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WebsiteAuthor_Id",
                table: "WebsiteAuthor",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticleLocks");

            migrationBuilder.DropTable(
                name: "ArticleLogs");

            migrationBuilder.DropTable(
                name: "ArticleNumbers");

            migrationBuilder.DropTable(
                name: "ArticlePermission");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuthorInfos");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "Layouts");

            migrationBuilder.DropTable(
                name: "Metrics");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "TotpTokens");

            migrationBuilder.DropTable(
                name: "WebsiteAuthor");

            migrationBuilder.DropTable(
                name: "ArticleCatalog");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
