using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.IoTService.Deployment.DataAccess.Migrations
{
    public partial class AddIsSynced : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "Modules",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "Modules");
        }
    }
}
