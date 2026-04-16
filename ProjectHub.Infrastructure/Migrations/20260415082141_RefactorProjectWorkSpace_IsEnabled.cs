@using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorProjectWorkSpace_IsEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 注意：EnabledProjectIdsJson 列在 WorkSpaces 表中不再使用
            // 由于 SQLite 限制，我们保留该列但代码中不再使用它
            // 启用状态现在存储在 ProjectWorkSpaces 表的 IsEnabled 列中

            migrationBuilder.AddColumn<bool>(
                name: "IsEnabled",
                table: "ProjectWorkSpaces",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEnabled",
                table: "ProjectWorkSpaces");
        }
    }
}
