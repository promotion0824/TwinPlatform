using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.Infrastructure.Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomerInstance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResourceGroupName",
                schema: "wsup",
                table: "Stamps");

            migrationBuilder.AddColumn<string>(
                name: "AzureDataExplorerInstance",
                schema: "wsup",
                table: "CustomerInstances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AzureDigitalTwinsInstance",
                schema: "wsup",
                table: "CustomerInstances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResourceGroupName",
                schema: "wsup",
                table: "CustomerInstances",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AzureDataExplorerInstance",
                schema: "wsup",
                table: "CustomerInstances");

            migrationBuilder.DropColumn(
                name: "AzureDigitalTwinsInstance",
                schema: "wsup",
                table: "CustomerInstances");

            migrationBuilder.DropColumn(
                name: "ResourceGroupName",
                schema: "wsup",
                table: "CustomerInstances");

            migrationBuilder.AddColumn<string>(
                name: "ResourceGroupName",
                schema: "wsup",
                table: "Stamps",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
