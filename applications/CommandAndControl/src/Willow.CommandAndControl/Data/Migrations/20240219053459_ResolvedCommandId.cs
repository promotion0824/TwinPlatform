#nullable disable

namespace Willow.CommandAndControl.Data;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class ResolvedCommandId : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "ResolvedCommandId",
            table: "ActivityLog",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_ActivityLog_ResolvedCommand_ResolvedCommandId",
            table: "ActivityLog",
            column: "ResolvedCommandId",
            principalTable: "ResolvedCommand",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ActivityLog_ResolvedCommand_ResolvedCommandId",
            table: "ActivityLog");

        migrationBuilder.DropColumn(
            name: "ResolvedCommandId",
            table: "ActivityLog");
    }
}
