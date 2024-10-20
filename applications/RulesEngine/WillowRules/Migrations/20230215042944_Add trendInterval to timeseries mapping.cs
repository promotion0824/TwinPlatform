using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class AddtrendIntervaltotimeseriesmapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrendInterval",
                table: "TimeSeriesMapping",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrendInterval",
                table: "TimeSeriesMapping");
        }
    }
}
