using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
	public partial class Wipeactors : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "TRUNCATE TABLE Actors"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "TRUNCATE TABLE TimeSeries"
			});
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
