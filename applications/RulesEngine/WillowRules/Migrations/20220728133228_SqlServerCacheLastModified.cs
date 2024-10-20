using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class SqlServerCacheLastModified : Migration
    {
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<DateTimeOffset>(
				name: "LastUpdated",
				table: "Cache",
				type: "datetimeoffset",
				nullable: false,
				defaultValueSql: "GETUTCDATE()");

			migrationBuilder.CreateIndex(
				name: "IX_Cache_LastUpdated",
				table: "Cache",
				column: "LastUpdated");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
				name: "IX_Cache_LastUpdated",
				table: "Cache");

			migrationBuilder.DropColumn(
				name: "LastUpdated",
				table: "Cache");
		}
	}
}
