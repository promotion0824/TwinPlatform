using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeseriesLatency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Latency",
                table: "TimeSeries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "LatencyEstimator",
                table: "TimeSeries",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latency",
                table: "TimeSeries");

            migrationBuilder.DropColumn(
                name: "LatencyEstimator",
                table: "TimeSeries");
        }
    }
}
