using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;


#nullable disable
namespace WillowRules.Migrations
{
	/// <inheritdoc />
    public partial class AddTwinLocations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TwinLocations",
                table: "RuleInstance",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwinLocations",
                table: "Insight",
                type: "nvarchar(max)",
                nullable: true);

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [Insight] SET [Insight].[Points] = [RuleInstance].[PointEntityIds] FROM [Insight] INNER JOIN [RuleInstance] ON [Insight].[Id] = [RuleInstance].[Id]"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TwinLocations",
                table: "RuleInstance");

            migrationBuilder.DropColumn(
                name: "TwinLocations",
                table: "Insight");
        }
    }
}
