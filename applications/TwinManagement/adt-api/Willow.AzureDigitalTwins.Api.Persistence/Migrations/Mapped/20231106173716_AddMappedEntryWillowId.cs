using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.AzureDigitalTwins.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMappedEntryWillowId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WillowId",
                table: "MappedEntries",
                type: "nvarchar(48)",
                maxLength: 48,
                nullable: true);

            migrationBuilder.Sql("UPDATE MappedEntries SET WillowId = MappedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WillowId",
                table: "MappedEntries");
        }
    }
}
