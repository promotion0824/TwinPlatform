using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.IoTService.Deployment.DataAccess.Migrations
{
    public partial class RemoveVersionColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "ModuleTypeVersions");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Version",
                table: "ModuleTypeVersions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
