using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WorkFolder_UniqueNameAndParentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 删除旧的唯一索引
            migrationBuilder.DropIndex(
                name: "IX_WorkFolders_Name",
                table: "WorkFolders");

            // 创建新的复合唯一索引
            migrationBuilder.CreateIndex(
                name: "IX_WorkFolders_Name_ParentId",
                table: "WorkFolders",
                columns: new[] { "Name", "ParentId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkFolders_Name_ParentId",
                table: "WorkFolders");

            migrationBuilder.CreateIndex(
                name: "IX_WorkFolders_Name",
                table: "WorkFolders",
                column: "Name",
                unique: true);
        }
    }
}
