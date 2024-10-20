using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class FilterExpressionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RuleFiltersBound",
                table: "RuleInstance",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Filters",
                table: "Rule",
                type: "nvarchar(max)",
                nullable: true);


			//EF does not give null columns default config, we have to do it manually
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [RuleInstance] SET RuleFiltersBound = '[]'"
			});


			//EF does not give null columns default config, we have to do it manually
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [Rule] SET Filters = '[]'"
			});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RuleFiltersBound",
                table: "RuleInstance");

            migrationBuilder.DropColumn(
                name: "Filters",
                table: "Rule");
        }
    }
}
