using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;


#nullable disable
namespace WillowRules.Migrations
{
	/// <inheritdoc />
    public partial class ValueOutOfRangeEstimaterColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ValueOutOfRangeEstimator",
                table: "TimeSeries",
                type: "nvarchar(1000)",
                nullable: true);

			//reset IsValueOutOfRange flags
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = @"update TimeSeries set [IsValueOutOfRange] = 0"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValueOutOfRangeEstimator",
                table: "TimeSeries");
        }
    }
}
