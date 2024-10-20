using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class MonotonicEstimatorColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MonotonicAscendingEstimator",
                table: "TimeSeries",
                type: "nvarchar(1000)",
                nullable: true);

			//reset monotonic flags
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = @"
					update TimeSeries set [IsNonMonotonicAscending] = 0"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MonotonicAscendingEstimator",
                table: "TimeSeries");
        }
    }
}
