using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class TimeSeriesvalidationflagsandmodelId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOffline",
                table: "TimeSeries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPeriodOutOfRange",
                table: "TimeSeries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStuck",
                table: "TimeSeries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsValueOutOfRange",
                table: "TimeSeries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModelId",
                table: "TimeSeries",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOffline",
                table: "TimeSeries");

            migrationBuilder.DropColumn(
                name: "IsPeriodOutOfRange",
                table: "TimeSeries");

            migrationBuilder.DropColumn(
                name: "IsStuck",
                table: "TimeSeries");

            migrationBuilder.DropColumn(
                name: "IsValueOutOfRange",
                table: "TimeSeries");

            migrationBuilder.DropColumn(
                name: "ModelId",
                table: "TimeSeries");
        }
    }
}
