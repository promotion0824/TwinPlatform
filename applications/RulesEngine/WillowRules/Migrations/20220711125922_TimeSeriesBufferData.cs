using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class TimeSeriesBufferData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActorState",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RuleId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RuleInstanceId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    LastFaulted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EarliestSeen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastChangedOutput = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Invocations = table.Column<long>(type: "bigint", nullable: false),
                    TimedValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutputValues = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActorState", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeSeriesBuffer",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EarliestSeen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastSeen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MaxTimeToKeep = table.Column<long>(type: "bigint", nullable: true),
                    MaxCountToKeep = table.Column<int>(type: "int", nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    TotalValuesProcessed = table.Column<long>(type: "bigint", nullable: false),
					EstimatedCurrentValue = table.Column<double>(type: "float", nullable: false),
                    EstimatedPeriod = table.Column<long>(type: "bigint", nullable: false),
                    AverageInBuffer = table.Column<double>(type: "float", nullable: false),
                    AverageValue = table.Column<double>(type: "float", nullable: false),
                    LastValueDouble = table.Column<double>(type: "float", nullable: true),
					LastValueBool = table.Column<bool>(type: "bit", nullable: true),
                    LastValueString = table.Column<string>(type: "nvarchar(1000)", nullable: true),
                    Points = table.Column<string>(type: "nvarchar(max)", nullable: true),
					CurrentValueEstimator = table.Column<string>(type: "nvarchar(max)", nullable: true),
					AveragePeriodEstimator = table.Column<string>(type: "nvarchar(max)", nullable: true),
					CompressionState = table.Column<string>(type: "nvarchar(max)", nullable: true),
					MaxValue = table.Column<double>(type: "float", nullable: true),
					MinValue = table.Column<double>(type: "float", nullable: true),
					TotalValue = table.Column<double>(type: "float", nullable: true),
				},
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSeriesBuffer", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActorState_RuleId",
                table: "ActorState",
                column: "RuleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActorState");

            migrationBuilder.DropTable(
                name: "TimeSeriesBuffer");
        }
    }
}
