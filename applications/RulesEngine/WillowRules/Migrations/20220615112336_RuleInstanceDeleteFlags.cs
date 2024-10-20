using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class RuleInstanceDeleteFlags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastUpdated",
                table: "RuleInstance",
                type: "datetimeoffset",
                nullable: false,
				defaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_RuleInstance_LastUpdated",
                table: "RuleInstance",
                column: "LastUpdated");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RuleInstance_LastUpdated",
                table: "RuleInstance");

            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "RuleInstance");
        }
    }
}
