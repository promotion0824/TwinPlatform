#nullable disable
namespace Willow.CommandAndControl.Data.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class ReceivedDate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "ReceivedDate",
            table: "RequestedCommand",
            type: "datetimeoffset",
            nullable: false,
            defaultValue: DateTimeOffset.UtcNow);

        // For older test data only.
        migrationBuilder.Sql("UPDATE RequestedCommand SET ReceivedDate = CreatedDate");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ReceivedDate",
            table: "RequestedCommand");
    }
}
