#nullable disable

namespace Willow.CommandAndControl.WebApi.Data.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class AddNewLocationInfoAndActivityLogsColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Detail",
            table: "ActivityLog",
            newName: "TwinId");

        migrationBuilder.AddColumn<string>(
            name: "IsCapabilityOf",
            table: "RequestedCommand",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "IsHostedBy",
            table: "RequestedCommand",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "Location",
            table: "RequestedCommand",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "SiteId",
            table: "RequestedCommand",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);

        migrationBuilder.AddColumn<string>(
            name: "ConnectorId",
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
            name: "SiteId",
            table: "ActivityLog",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: string.Empty);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsCapabilityOf",
            table: "RequestedCommand");

        migrationBuilder.DropColumn(
            name: "IsHostedBy",
            table: "RequestedCommand");

        migrationBuilder.DropColumn(
            name: "Location",
            table: "RequestedCommand");

        migrationBuilder.DropColumn(
            name: "SiteId",
            table: "RequestedCommand");

        migrationBuilder.DropColumn(
            name: "ConnectorId",
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
            name: "SiteId",
            table: "ActivityLog");

        migrationBuilder.RenameColumn(
            name: "TwinId",
            table: "ActivityLog",
            newName: "Detail");
    }
}
