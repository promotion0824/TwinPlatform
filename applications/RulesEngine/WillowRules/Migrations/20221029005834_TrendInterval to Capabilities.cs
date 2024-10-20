using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
	public partial class TrendIntervaltoCapabilities : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// migrationBuilder.DropColumn(
			//     name: "Cost",
			//     table: "Insight");

			migrationBuilder.AddColumn<string>(
				name: "DtId",
				table: "TimeSeries",
				type: "nvarchar(max)",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "TrendInterval",
				table: "TimeSeries",
				type: "int",
				nullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "DtId",
				table: "TimeSeries");

			migrationBuilder.DropColumn(
				name: "TrendInterval",
				table: "TimeSeries");

			migrationBuilder.AddColumn<double>(
				name: "Cost",
				table: "Insight",
				type: "float",
				nullable: false,
				defaultValue: 0.0);
		}
	}
}
