using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePermissionTableIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_Name",
                table: "Permissions");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name_Extension",
                table: "Permissions",
                columns: new[] { "Name", "Extension" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_Name_Extension",
                table: "Permissions");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name",
                table: "Permissions",
                column: "Name",
                unique: true);
        }
    }
}
