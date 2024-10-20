using System;
using Microsoft.EntityFrameworkCore.Migrations;


#nullable disable
namespace WillowRules.Migrations
{
	public partial class EarliestExecutionDateColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EarliestExecutionDate",
                table: "RuleMetadata",
                type: "datetimeoffset",
                nullable: false,
				//default to max value so the next execution can start doing updates
                defaultValue: new DateTimeOffset(new DateTime(9999, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EarliestExecutionDate",
                table: "RuleMetadata");
        }
    }
}
