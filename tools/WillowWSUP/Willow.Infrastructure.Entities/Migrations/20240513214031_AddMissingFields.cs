using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.Infrastructure.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResourceGroupName",
                schema: "wsup",
                table: "Stamps",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeploymentPhase",
                schema: "wsup",
                table: "CustomerInstances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullCustomerInstanceName",
                schema: "wsup",
                table: "CustomerInstances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LifecycleState",
                schema: "wsup",
                table: "CustomerInstances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HealthEndpointPath",
                schema: "wsup",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                schema: "wsup",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                schema: "wsup",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResourceGroupName",
                schema: "wsup",
                table: "Stamps");

            migrationBuilder.DropColumn(
                name: "DeploymentPhase",
                schema: "wsup",
                table: "CustomerInstances");

            migrationBuilder.DropColumn(
                name: "FullCustomerInstanceName",
                schema: "wsup",
                table: "CustomerInstances");

            migrationBuilder.DropColumn(
                name: "LifecycleState",
                schema: "wsup",
                table: "CustomerInstances");

            migrationBuilder.DropColumn(
                name: "HealthEndpointPath",
                schema: "wsup",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "Path",
                schema: "wsup",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "RoleName",
                schema: "wsup",
                table: "Applications");
        }
    }
}
