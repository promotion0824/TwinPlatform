using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.IoTService.Deployment.DataAccess.Migrations
{
    public partial class AddStatusMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusMessage",
                table: "Deployments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusMessage",
                table: "Deployments");
        }
    }
}
