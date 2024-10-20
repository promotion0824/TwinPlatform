#nullable disable

namespace Willow.CommandAndControl.WebApi.Data.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class AddActivityLogsForeignKeyRefenceToRequestedCommands : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ConnectorId",
            table: "ActivityLog");

        migrationBuilder.DropColumn(
            name: "ExternalId",
            table: "ActivityLog");

        migrationBuilder.DropColumn(
            name: "IsCapabilityOf",
            table: "ActivityLog");

        migrationBuilder.DropColumn(
            name: "IsHostedBy",
            table: "ActivityLog");

        migrationBuilder.DropColumn(
            name: "Location",
            table: "ActivityLog");

        migrationBuilder.DropColumn(
            name: "RuleId",
            table: "ActivityLog");

        migrationBuilder.DropColumn(
            name: "SiteId",
            table: "ActivityLog");

        migrationBuilder.DropColumn(
            name: "TwinId",
            table: "ActivityLog");

        migrationBuilder.DropColumn(
            name: "Value",
            table: "ActivityLog");

        migrationBuilder.AddColumn<Guid>(
            name: "RequestedCommandId",
            table: "ActivityLog",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateIndex(
            name: "IX_ActivityLog_RequestedCommandId",
            table: "ActivityLog",
            column: "RequestedCommandId");

        migrationBuilder.AddForeignKey(
            name: "FK_ActivityLog_RequestedCommand_RequestedCommandId",
            table: "ActivityLog",
            column: "RequestedCommandId",
            principalTable: "RequestedCommand",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_ActivityLog_RequestedCommand_RequestedCommandId",
            table: "ActivityLog");

        migrationBuilder.DropIndex(
            name: "IX_ActivityLog_RequestedCommandId",
            table: "ActivityLog");

        migrationBuilder.DropColumn(
            name: "RequestedCommandId",
            table: "ActivityLog");

        migrationBuilder.AddColumn<string>(
            name: "ConnectorId",
            table: "ActivityLog",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "ExternalId",
            table: "ActivityLog",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "IsCapabilityOf",
            table: "ActivityLog",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "IsHostedBy",
            table: "ActivityLog",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Location",
            table: "ActivityLog",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "RuleId",
            table: "ActivityLog",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "SiteId",
            table: "ActivityLog",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "TwinId",
            table: "ActivityLog",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<double>(
            name: "Value",
            table: "ActivityLog",
            type: "float",
            nullable: false,
            defaultValue: 0.0);
    }
}
