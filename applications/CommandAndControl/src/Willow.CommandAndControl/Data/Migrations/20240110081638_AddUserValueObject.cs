#nullable disable

namespace Willow.CommandAndControl.WebApi.Data.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class AddUserValueObject : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "StatusUpdatedBy",
            table: "ResolvedCommand",
            newName: "StatusUpdatedBy_Name");

        migrationBuilder.RenameColumn(
            name: "StatusUpdatedBy",
            table: "RequestedCommand",
            newName: "StatusUpdatedBy_Name");

        migrationBuilder.RenameColumn(
            name: "UpdatedBy",
            table: "ActivityLog",
            newName: "UpdatedBy_Name");

        migrationBuilder.AddColumn<string>(
            name: "StatusUpdatedBy_Email",
            table: "ResolvedCommand",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "StatusUpdatedBy_Email",
            table: "RequestedCommand",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy_Email",
            table: "ActivityLog",
            type: "nvarchar(max)",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "StatusUpdatedBy_Email",
            table: "ResolvedCommand");

        migrationBuilder.DropColumn(
            name: "StatusUpdatedBy_Email",
            table: "RequestedCommand");

        migrationBuilder.DropColumn(
            name: "UpdatedBy_Email",
            table: "ActivityLog");

        migrationBuilder.RenameColumn(
            name: "StatusUpdatedBy_Name",
            table: "ResolvedCommand",
            newName: "StatusUpdatedBy");

        migrationBuilder.RenameColumn(
            name: "StatusUpdatedBy_Name",
            table: "RequestedCommand",
            newName: "StatusUpdatedBy");

        migrationBuilder.RenameColumn(
            name: "UpdatedBy_Name",
            table: "ActivityLog",
            newName: "UpdatedBy");
    }
}
