// <copyright file="20251005213605_InitialCreate.cs" company="Moonrise Software, LLC">
// Copyright (c) Moonrise Software, LLC. All rights reserved.
// Licensed under the GNU Public License, Version 3.0 (https://www.gnu.org/licenses/gpl-3.0.html)
// See https://github.com/MoonriseSoftwareCalifornia/CosmosCMS
// for more information concerning the license and the contributors participating to this project.
// </copyright>

namespace Cosmos.Common.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;
    using MySql.EntityFrameworkCore.Metadata;

    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticleCatalog",
                columns: table => new
                {
                    ArticleNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AuthorInfo = table.Column<string>(type: "longtext", nullable: true),
                    BannerImage = table.Column<string>(type: "longtext", nullable: true),
                    Title = table.Column<string>(type: "longtext", nullable: true),
                    Introduction = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    Published = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    UrlPath = table.Column<string>(type: "varchar(255)", nullable: true),
                    TemplateId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: true)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleCatalog", x => x.ArticleNumber);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticleLocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ConnectionId = table.Column<string>(type: "longtext", nullable: true),
                    UserEmail = table.Column<string>(type: "longtext", nullable: true),
                    ArticleId = table.Column<Guid>(type: "char(36)", nullable: false),
                    LockSetDateTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    EditorType = table.Column<string>(type: "longtext", nullable: true),
                    FilePath = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleLocks", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticleLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    IdentityUserId = table.Column<string>(type: "longtext", nullable: true),
                    ArticleId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ArticleTitle = table.Column<string>(type: "longtext", nullable: true),
                    ActivityNotes = table.Column<string>(type: "longtext", nullable: true),
                    DateTimeStamp = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleLogs", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticleNumbers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    SetDateTime = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticleNumbers", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ArticleNumber = table.Column<int>(type: "int", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    UrlPath = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Published = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    Title = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    HeaderJavaScript = table.Column<string>(type: "longtext", nullable: true),
                    FooterJavaScript = table.Column<string>(type: "longtext", nullable: true),
                    BannerImage = table.Column<string>(type: "longtext", nullable: false),
                    UserId = table.Column<string>(type: "longtext", nullable: true),
                    TemplateId = table.Column<Guid>(type: "char(36)", nullable: true),
                    IsBlogPost = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Category = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    Introduction = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true),
                    RedirectTarget = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: true)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuthorInfos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false),
                    AuthorName = table.Column<string>(type: "longtext", nullable: true),
                    AuthorDescription = table.Column<string>(type: "longtext", nullable: true),
                    TwitterHandle = table.Column<string>(type: "longtext", nullable: true),
                    InstagramUrl = table.Column<string>(type: "longtext", nullable: true),
                    Website = table.Column<string>(type: "longtext", nullable: true),
                    EmailAddress = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorInfos", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    FirstName = table.Column<string>(type: "longtext", nullable: true),
                    LastName = table.Column<string>(type: "longtext", nullable: true),
                    Email = table.Column<string>(type: "longtext", nullable: false),
                    Phone = table.Column<string>(type: "longtext", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FriendlyName = table.Column<string>(type: "longtext", nullable: true),
                    Xml = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Layouts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: true),
                    CommunityLayoutId = table.Column<string>(type: "longtext", nullable: true),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LayoutName = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true),
                    Head = table.Column<string>(type: "longtext", nullable: true),
                    BodyHtmlAttributes = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true),
                    HtmlHeader = table.Column<string>(type: "longtext", nullable: true),
                    FooterHtmlContent = table.Column<string>(type: "longtext", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    Published = table.Column<DateTimeOffset>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layouts", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Metrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ConnectionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    BlobStorageBytes = table.Column<long>(type: "bigint", nullable: false),
                    BlobStorageEgressBytes = table.Column<long>(type: "bigint", nullable: false),
                    BlobStorageIngressBytes = table.Column<long>(type: "bigint", nullable: false),
                    DatabaseDataUsageBytes = table.Column<long>(type: "bigint", nullable: false),
                    DatabaseIndexUsageBytes = table.Column<long>(type: "bigint", nullable: false),
                    DatabaseRuUsage = table.Column<long>(type: "bigint", nullable: false),
                    FrontDoorRequestBytes = table.Column<long>(type: "bigint", nullable: false),
                    FrontDoorResponseBytes = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Metrics", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    ArticleNumber = table.Column<int>(type: "int", nullable: false),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    UrlPath = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    ParentUrlPath = table.Column<string>(type: "longtext", nullable: true),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    Published = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    Expires = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    Title = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: true),
                    Updated = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    BannerImage = table.Column<string>(type: "longtext", nullable: false),
                    HeaderJavaScript = table.Column<string>(type: "longtext", nullable: true),
                    FooterJavaScript = table.Column<string>(type: "longtext", nullable: true),
                    AuthorInfo = table.Column<string>(type: "longtext", nullable: true),
                    TemplateId = table.Column<Guid>(type: "char(36)", nullable: true),
                    IsBlogPost = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Category = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    Introduction = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "longblob", rowVersion: true, nullable: true)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Group = table.Column<string>(type: "longtext", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false),
                    Value = table.Column<string>(type: "longtext", nullable: false),
                    IsRequired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    LayoutId = table.Column<Guid>(type: "char(36)", nullable: true),
                    CommunityLayoutId = table.Column<string>(type: "longtext", nullable: true),
                    Title = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    Content = table.Column<string>(type: "longtext", nullable: true),
                    PageType = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TotpTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<string>(type: "longtext", nullable: true),
                    Email = table.Column<string>(type: "longtext", nullable: true),
                    Token = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TotpTokens", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArticlePermission",
                columns: table => new
                {
                    ArticleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    IdentityObjectId = table.Column<string>(type: "longtext", nullable: true),
                    Permission = table.Column<string>(type: "longtext", nullable: true),
                    IsRoleObject = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CatalogEntryArticleNumber = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticlePermission", x => x.ArticleId);
                    table.ForeignKey(
                        name: "FK_ArticlePermission_ArticleCatalog_CatalogEntryArticleNumber",
                        column: x => x.CatalogEntryArticleNumber,
                        principalTable: "ArticleCatalog",
                        principalColumn: "ArticleNumber");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false),
                    Value = table.Column<string>(type: "longtext", nullable: true)
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ArticleCatalog_UrlPath",
                table: "ArticleCatalog",
                column: "UrlPath");

            migrationBuilder.CreateIndex(
                name: "IX_ArticlePermission_CatalogEntryArticleNumber",
                table: "ArticlePermission",
                column: "CatalogEntryArticleNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_ArticleNumber",
                table: "Articles",
                column: "ArticleNumber");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pages_UrlPath",
                table: "Pages",
                column: "UrlPath");
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
                name: "ArticleCatalog");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
