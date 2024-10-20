using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class ActorTimedValueOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNonMonotonicAscending",
                table: "TimeSeries");

            migrationBuilder.DropColumn(
                name: "MonotonicAscendingEstimator",
                table: "TimeSeries");

            migrationBuilder.AddColumn<string>(
                name: "TimedValueOptions",
                table: "Actors",
                type: "nvarchar(4000)",
                nullable: true);

			migrationBuilder.AlterColumn<string>(
				name: "DtId",
				table: "TimeSeries",
				maxLength: 1024,
				nullable: true
			);

			migrationBuilder.CreateIndex(
			   name: "IX_TimeSeries_DtId",
			   table: "TimeSeries",
			   column: "DtId");

			//EF does not give null columns default config, we have to do it manually
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [Actors] SET TimedValueOptions = '{}'"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimedValueOptions",
                table: "Actors");

            migrationBuilder.AddColumn<bool>(
                name: "IsNonMonotonicAscending",
                table: "TimeSeries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MonotonicAscendingEstimator",
                table: "TimeSeries",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
