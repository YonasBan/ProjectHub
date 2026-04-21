using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CustomIconPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ColorTag = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    LastOpenedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LaunchCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalUsageDurationMs = table.Column<long>(type: "INTEGER", nullable: false),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    FavoritedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Deadline = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CustomerId = table.Column<long>(type: "INTEGER", nullable: true),
                    DefaultProgram = table.Column<string>(type: "TEXT", nullable: true),
                    LaunchArguments = table.Column<string>(type: "TEXT", nullable: true),
                    WebUrl = table.Column<string>(type: "TEXT", nullable: true),
                    CmdCommand = table.Column<string>(type: "TEXT", nullable: true),
                    LaunchType = table.Column<int>(type: "INTEGER", nullable: false),
                    RunAsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkFolders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IconPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ParentId = table.Column<long>(type: "INTEGER", nullable: true),
                    ProjectCount = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkSpaceCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkFolders_WorkFolders_ParentId",
                        column: x => x.ParentId,
                        principalTable: "WorkFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkSpaces",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IconPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false),
                    FavoritedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastOpenedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UseCustomLaunchOrder = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultLaunchIntervalSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    LaunchOrderJson = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSpaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<long>(type: "INTEGER", nullable: false),
                    TagId = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectTags_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectWorkFolders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<long>(type: "INTEGER", nullable: false),
                    WorkFolderId = table.Column<long>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectWorkFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectWorkFolders_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectWorkFolders_WorkFolders_WorkFolderId",
                        column: x => x.WorkFolderId,
                        principalTable: "WorkFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectWorkSpaces",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<long>(type: "INTEGER", nullable: false),
                    WorkSpaceId = table.Column<long>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectWorkSpaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectWorkSpaces_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectWorkSpaces_WorkSpaces_WorkSpaceId",
                        column: x => x.WorkSpaceId,
                        principalTable: "WorkSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkSpaceTags",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkSpaceId = table.Column<long>(type: "INTEGER", nullable: false),
                    TagId = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSpaceTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkSpaceTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkSpaceTags_WorkSpaces_WorkSpaceId",
                        column: x => x.WorkSpaceId,
                        principalTable: "WorkSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkSpaceWorkFolders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkSpaceId = table.Column<long>(type: "INTEGER", nullable: false),
                    WorkFolderId = table.Column<long>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSpaceWorkFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkSpaceWorkFolders_WorkFolders_WorkFolderId",
                        column: x => x.WorkFolderId,
                        principalTable: "WorkFolders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkSpaceWorkFolders_WorkSpaces_WorkSpaceId",
                        column: x => x.WorkSpaceId,
                        principalTable: "WorkSpaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_FavoritedAt",
                table: "Projects",
                column: "FavoritedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsFavorite",
                table: "Projects",
                column: "IsFavorite");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_LastOpenedAt",
                table: "Projects",
                column: "LastOpenedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Path",
                table: "Projects",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTags_ProjectId",
                table: "ProjectTags",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTags_ProjectId_TagId",
                table: "ProjectTags",
                columns: new[] { "ProjectId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectTags_TagId",
                table: "ProjectTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectWorkFolders_ProjectId_WorkFolderId",
                table: "ProjectWorkFolders",
                columns: new[] { "ProjectId", "WorkFolderId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectWorkFolders_WorkFolderId",
                table: "ProjectWorkFolders",
                column: "WorkFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectWorkSpaces_ProjectId_WorkSpaceId",
                table: "ProjectWorkSpaces",
                columns: new[] { "ProjectId", "WorkSpaceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectWorkSpaces_WorkSpaceId",
                table: "ProjectWorkSpaces",
                column: "WorkSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_SortOrder",
                table: "Tags",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFolders_Name_ParentId",
                table: "WorkFolders",
                columns: new[] { "Name", "ParentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkFolders_ParentId",
                table: "WorkFolders",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaces_FavoritedAt",
                table: "WorkSpaces",
                column: "FavoritedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaces_IsFavorite",
                table: "WorkSpaces",
                column: "IsFavorite");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaces_LastOpenedAt",
                table: "WorkSpaces",
                column: "LastOpenedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaces_Name",
                table: "WorkSpaces",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaceTags_TagId",
                table: "WorkSpaceTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaceTags_WorkSpaceId",
                table: "WorkSpaceTags",
                column: "WorkSpaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaceTags_WorkSpaceId_TagId",
                table: "WorkSpaceTags",
                columns: new[] { "WorkSpaceId", "TagId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaceWorkFolders_WorkFolderId",
                table: "WorkSpaceWorkFolders",
                column: "WorkFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkSpaceWorkFolders_WorkSpaceId_WorkFolderId",
                table: "WorkSpaceWorkFolders",
                columns: new[] { "WorkSpaceId", "WorkFolderId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectTags");

            migrationBuilder.DropTable(
                name: "ProjectWorkFolders");

            migrationBuilder.DropTable(
                name: "ProjectWorkSpaces");

            migrationBuilder.DropTable(
                name: "WorkSpaceTags");

            migrationBuilder.DropTable(
                name: "WorkSpaceWorkFolders");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "WorkFolders");

            migrationBuilder.DropTable(
                name: "WorkSpaces");
        }
    }
}
