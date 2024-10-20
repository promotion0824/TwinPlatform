using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class RequestedByColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestedBy",
                table: "Progress",
                type: "nvarchar(100)",
                nullable: true);
        }

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "RequestedBy",
				table: "Progress");
		}
    }
}
