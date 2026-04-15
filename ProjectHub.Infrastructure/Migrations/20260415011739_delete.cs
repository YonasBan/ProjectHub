using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class delete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeleteedTime",
                table: "WorkSpaceWorkFolder",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeleteedTime",
                table: "WorkSpaceTags",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeleteedTime",
                table: "WorkSpaces",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeleteedTime",
                table: "WorkFolders",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeleteedTime",
                table: "Tags",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeleteedTime",
                table: "ProjectWorkSpaces",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeleteedTime",
                table: "ProjectWorkFolders",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeleteedTime",
                table: "ProjectTags",
                newName: "DeletedAt");

            migrationBuilder.RenameColumn(
                name: "DeleteedTime",
                table: "Projects",
                newName: "DeletedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "WorkSpaceWorkFolder",
                newName: "DeleteedTime");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "WorkSpaceTags",
                newName: "DeleteedTime");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "WorkSpaces",
                newName: "DeleteedTime");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "WorkFolders",
                newName: "DeleteedTime");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Tags",
                newName: "DeleteedTime");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "ProjectWorkSpaces",
                newName: "DeleteedTime");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "ProjectWorkFolders",
                newName: "DeleteedTime");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "ProjectTags",
                newName: "DeleteedTime");

            migrationBuilder.RenameColumn(
                name: "DeletedAt",
                table: "Projects",
                newName: "DeleteedTime");
        }
    }
}
