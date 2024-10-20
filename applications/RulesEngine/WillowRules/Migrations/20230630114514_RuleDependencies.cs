using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class RuleDependencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RuleDependenciesBound",
                table: "RuleInstance",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RuleDependencyCount",
                table: "RuleInstance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Dependencies",
                table: "Rule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dependencies",
                table: "Insight",
                type: "nvarchar(max)",
                nullable: true);

			//EF does not give null columns default config, we have to do it manually
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [RuleInstance] SET RuleDependenciesBound = '[]'"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [Rule] SET Dependencies = '[]'"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [Insight] SET Dependencies = '[]'"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RuleDependenciesBound",
                table: "RuleInstance");

            migrationBuilder.DropColumn(
                name: "RuleDependencyCount",
                table: "RuleInstance");

            migrationBuilder.DropColumn(
                name: "Dependencies",
                table: "Rule");

            migrationBuilder.DropColumn(
                name: "Dependencies",
                table: "Insight");
        }
    }
}
