using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
	/// <inheritdoc />
	public partial class RuleTimseriesMappingTable : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "RuleTimeSeriesMapping",
				columns: table => new
				{
					Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
					TrendId = table.Column<string>(type: "nvarchar(450)", nullable: true),
					ExternalId = table.Column<string>(type: "nvarchar(450)", nullable: true),
					ConnectorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
					DtId = table.Column<string>(type: "nvarchar(450)", nullable: true),
					RuleId = table.Column<string>(type: "nvarchar(450)", nullable: true),
					RuleInstanceId = table.Column<string>(type: "nvarchar(450)", nullable: true),
					LastUpdate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_RuleTimeSeriesMapping", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_RuleTimeSeriesMapping_RuleId",
				table: "RuleTimeSeriesMapping",
				column: "RuleId");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "RuleTimeSeriesMapping");
		}
	}
}
