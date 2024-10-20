using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
	/// <inheritdoc />
	public partial class Lastgapneedstobestoredasticks : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<TimeSpan>(
				name: "LastGap",
				table: "TimeSeries",
				type: "bigint",
				nullable: false,
				defaultValue: new TimeSpan(0, 0, 0, 0));
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "LastGap",
				table: "TimeSeries");
		}
	}
}
