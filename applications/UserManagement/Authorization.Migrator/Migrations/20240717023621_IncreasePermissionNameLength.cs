using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class IncreasePermissionNameLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_Extension",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Name_Extension",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "Permissions");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Permissions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name_ApplicationId",
                table: "Permissions",
                columns: new[] { "Name", "ApplicationId" },
                unique: true,
                filter: "[ApplicationId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Permissions_Name_ApplicationId",
                table: "Permissions");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Permissions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "Permissions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Extension",
                table: "Permissions",
                column: "Extension");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Name_Extension",
                table: "Permissions",
                columns: new[] { "Name", "Extension" },
                unique: true);
        }
    }
}
