using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.AzureDigitalTwins.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingIdFieldAndUpdateConnectorIdField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ConnectorId",
                table: "MappedEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingId",
                table: "MappedEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "MappedEntries");

            migrationBuilder.AlterColumn<string>(
                name: "ConnectorId",
                table: "MappedEntries",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}
