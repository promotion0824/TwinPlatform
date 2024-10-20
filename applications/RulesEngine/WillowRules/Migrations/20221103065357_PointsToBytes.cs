using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
	public partial class PointsToBytes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			//clear first. This should update sql pages' data. Drop Column only updates metadata
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE TimeSeries SET Points = NULL"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE Actors SET TimedValues = NULL"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE Actors SET OutputValues = NULL"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE Insight SET Occurrences = NULL"
			});

			migrationBuilder.DropColumn(
			  name: "TimedValues",
			  table: "Actors");

			migrationBuilder.DropColumn(
			  name: "OutputValues",
			  table: "Actors");

			migrationBuilder.DropColumn(
			  name: "Points",
			  table: "TimeSeries");

			migrationBuilder.DropColumn(
			  name: "Occurrences",
			  table: "Insight");

			migrationBuilder.AddColumn<byte[]>(
				name: "TimedValues",
				table: "Actors",
				type: "varbinary(max)",
				nullable: true);

			migrationBuilder.AddColumn<byte[]>(
				name: "OutputValues",
				table: "Actors",
				type: "varbinary(max)",
				nullable: true);

			migrationBuilder.AddColumn<byte[]>(
				name: "Points",
				table: "TimeSeries",
				type: "varbinary(max)",
				nullable: true);

			migrationBuilder.AddColumn<byte[]>(
				name: "Occurrences",
				table: "Insight",
				type: "varbinary(max)",
				nullable: true);

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE TimeSeries SET Points = 0x"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE Actors SET TimedValues = 0x"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE Actors SET OutputValues = 0x"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE Insight SET Occurrences = 0x"
			});
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Points",
                table: "TimeSeries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TimedValues",
                table: "Actors",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(byte[]),
                oldType: "varbinary(max)",
                oldNullable: true);
        }
    }
}
