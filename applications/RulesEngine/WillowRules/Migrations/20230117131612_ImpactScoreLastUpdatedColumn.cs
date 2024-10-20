using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class ImpactScoreLastUpdatedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdated",
                table: "InsightImpactScore",
                type: "datetimeoffset",
                nullable: false,
				defaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_InsightImpactScore_LastUpdated",
                table: "InsightImpactScore",
                column: "LastUpdated");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InsightImpactScore_LastUpdated",
                table: "InsightImpactScore");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "InsightImpactScore");
        }
    }
}
