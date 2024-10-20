using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class Removeestimatorforcurrentvalue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentValueEstimator",
                table: "TimeSeries");

            migrationBuilder.DropColumn(
                name: "EstimatedCurrentValue",
                table: "TimeSeries");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentValueEstimator",
                table: "TimeSeries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "EstimatedCurrentValue",
                table: "TimeSeries",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
