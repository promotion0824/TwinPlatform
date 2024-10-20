#nullable disable

namespace Willow.CommandAndControl.WebApi.Data.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class InitialSchema : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ActivityLog",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                Type = table.Column<int>(type: "int", nullable: false),
                Value = table.Column<double>(type: "float", nullable: false),
                RuleId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Detail = table.Column<string>(type: "nvarchar(max)", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ActivityLog", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "RequestedCommand",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CommandName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ConnectorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                TwinId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Value = table.Column<double>(type: "float", nullable: false),
                Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                EndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                RuleId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                StatusUpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RequestedCommand", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "ResolvedCommand",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Status = table.Column<int>(type: "int", nullable: false),
                RequestedCommandId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                EndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                CreatedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ResolvedCommand", x => x.Id);
                table.ForeignKey(
                    name: "FK_ResolvedCommand_RequestedCommand_RequestedCommandId",
                    column: x => x.RequestedCommandId,
                    principalTable: "RequestedCommand",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ResolvedCommand_RequestedCommandId",
            table: "ResolvedCommand",
            column: "RequestedCommandId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ActivityLog");

        migrationBuilder.DropTable(
            name: "ResolvedCommand");

        migrationBuilder.DropTable(
            name: "RequestedCommand");
    }
}
