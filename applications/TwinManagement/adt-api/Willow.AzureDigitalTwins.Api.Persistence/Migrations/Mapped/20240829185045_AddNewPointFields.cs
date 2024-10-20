using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.AzureDigitalTwins.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPointFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataType",
                table: "MappedEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "MappedEntries",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataType",
                table: "MappedEntries");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "MappedEntries");
        }
    }
}
