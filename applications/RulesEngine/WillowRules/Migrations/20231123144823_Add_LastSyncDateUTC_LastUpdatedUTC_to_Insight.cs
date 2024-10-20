using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class Add_LastSyncDateUTC_LastUpdatedUTC_to_Insight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastSyncDateUTC",
                table: "Insight",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdatedUTC",
                table: "Insight",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

			migrationBuilder.CreateIndex(
				name: "IX_Insight_CommandEnabled",
				table: "Insight",
				column: "CommandEnabled");

			migrationBuilder.CreateIndex(
				name: "IX_Insight_LastUpdatedUTC",
				table: "Insight",
				column: "LastUpdatedUTC");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.DropIndex(
				name: "IX_Insight_CommandEnabled",
				table: "Insight");

			migrationBuilder.DropIndex(
				name: "IX_Insight_LastUpdatedUTC",
				table: "Insight");

			migrationBuilder.DropColumn(
                name: "LastSyncDateUTC",
                table: "Insight");

            migrationBuilder.DropColumn(
                name: "LastUpdatedUTC",
                table: "Insight");
        }
    }
}
