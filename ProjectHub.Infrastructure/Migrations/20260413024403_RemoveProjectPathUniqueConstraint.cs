using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectHub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProjectPathUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_Path",
                table: "Projects");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Path",
                table: "Projects",
                column: "Path");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_Path",
                table: "Projects");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Path",
                table: "Projects",
                column: "Path",
                unique: true);
        }
    }
}
