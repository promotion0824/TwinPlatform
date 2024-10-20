using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.Infrastructure.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddAppDisplayName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "wsup",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "wsup",
                table: "Applications");
        }
    }
}
