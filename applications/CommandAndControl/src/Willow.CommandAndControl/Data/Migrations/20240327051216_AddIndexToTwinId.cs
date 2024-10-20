#nullable disable

namespace Willow.CommandAndControl.Data.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class AddIndexToTwinId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "TwinId",
            table: "RequestedCommand",
            type: "nvarchar(450)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.CreateIndex(
            name: "IX_RequestedCommand_TwinId",
            table: "RequestedCommand",
            column: "TwinId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_RequestedCommand_TwinId",
            table: "RequestedCommand");

        migrationBuilder.AlterColumn<string>(
            name: "TwinId",
            table: "RequestedCommand",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(450)");
    }
}
