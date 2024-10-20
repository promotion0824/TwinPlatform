using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class SummaryTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SystemSummary",
                table: "ADTSummaries",
                type: "varchar(4000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SystemSummary",
                table: "ADTSummaries");
        }
    }
}
