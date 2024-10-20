using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class ADTSummaryExtensionDataColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtensionData",
                table: "ADTSummaries",
                type: "nvarchar(max)",
                nullable: true);

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [ADTSummaries] SET ExtensionData = '{}'"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtensionData",
                table: "ADTSummaries");
        }
    }
}
