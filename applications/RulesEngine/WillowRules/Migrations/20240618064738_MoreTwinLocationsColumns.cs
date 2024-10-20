using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class MoreTwinLocationsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TwinLocations",
                table: "TimeSeriesMapping",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwinLocations",
                table: "TimeSeries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwinLocations",
                table: "CalculatedPoints",
                type: "nvarchar(max)",
                nullable: true);


			//EF does not give null columns default config, we have to do it manually
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [TimeSeriesMapping] SET TwinLocations = '[]'"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [TimeSeries] SET TwinLocations = '[]'"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [CalculatedPoints] SET TwinLocations = '[]'"
			});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwinLocations",
                table: "TimeSeriesMapping");

            migrationBuilder.DropColumn(
                name: "TwinLocations",
                table: "TimeSeries");

            migrationBuilder.DropColumn(
                name: "TwinLocations",
                table: "CalculatedPoints");
        }
    }
}
