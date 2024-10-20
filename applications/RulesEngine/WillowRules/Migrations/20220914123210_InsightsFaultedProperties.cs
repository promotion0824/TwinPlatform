using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;


#nullable disable

namespace WillowRules.Migrations
{
    public partial class InsightsFaultedProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UnitOfMeasure",
                table: "TimeSeries",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EarliestFaultedDate",
                table: "Insight",
                type: "datetimeoffset",
				nullable: false,
				defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "FaultedCount",
                table: "Insight",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFaulty",
                table: "Insight",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFaultedDate",
                table: "Insight",
                type: "datetimeoffset",
                nullable: false,
				defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [dbo].[Insight] SET [LastFaultedDate] = [LastUpdated], [EarliestFaultedDate] = [LastUpdated]"
			});

			migrationBuilder.CreateIndex(
                name: "IX_TimeSeries_AverageValue",
                table: "TimeSeries",
                column: "AverageValue");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSeries_EstimatedPeriod",
                table: "TimeSeries",
                column: "EstimatedPeriod");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSeries_MaxValue",
                table: "TimeSeries",
                column: "MaxValue");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSeries_MinValue",
                table: "TimeSeries",
                column: "MinValue");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSeries_TotalValuesProcessed",
                table: "TimeSeries",
                column: "TotalValuesProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSeries_UnitOfMeasure",
                table: "TimeSeries",
                column: "UnitOfMeasure");

            migrationBuilder.CreateIndex(
                name: "IX_Insight_LastFaultedDate",
                table: "Insight",
                column: "LastFaultedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeSeries_AverageValue",
                table: "TimeSeries");

            migrationBuilder.DropIndex(
                name: "IX_TimeSeries_EstimatedPeriod",
                table: "TimeSeries");

            migrationBuilder.DropIndex(
                name: "IX_TimeSeries_MaxValue",
                table: "TimeSeries");

            migrationBuilder.DropIndex(
                name: "IX_TimeSeries_MinValue",
                table: "TimeSeries");

            migrationBuilder.DropIndex(
                name: "IX_TimeSeries_TotalValuesProcessed",
                table: "TimeSeries");

            migrationBuilder.DropIndex(
                name: "IX_TimeSeries_UnitOfMeasure",
                table: "TimeSeries");

            migrationBuilder.DropIndex(
                name: "IX_Insight_LastFaultedDate",
                table: "Insight");

            migrationBuilder.DropColumn(
                name: "EarliestFaultedDate",
                table: "Insight");

            migrationBuilder.DropColumn(
                name: "FaultedCount",
                table: "Insight");

            migrationBuilder.DropColumn(
                name: "IsFaulty",
                table: "Insight");

            migrationBuilder.DropColumn(
                name: "LastFaultedDate",
                table: "Insight");

            migrationBuilder.AlterColumn<string>(
                name: "UnitOfMeasure",
                table: "TimeSeries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
