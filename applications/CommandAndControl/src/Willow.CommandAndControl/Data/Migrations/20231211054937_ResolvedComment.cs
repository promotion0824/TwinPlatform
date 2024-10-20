#nullable disable

namespace Willow.CommandAndControl.WebApi.Data;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class ResolvedComment : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Comment",
            table: "ResolvedCommand",
            type: "nvarchar(255)",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Comment",
            table: "ResolvedCommand");
    }
}
