using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class IsWillowStandardColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWillowStandard",
                table: "RuleInstance",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWillowStandard",
                table: "Rule",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWillowStandard",
                table: "Insight",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWillowStandard",
                table: "GlobalVariable",
                type: "bit",
                nullable: false,
                defaultValue: false);

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [Rule] SET IsWillowStandard = 1 WHERE Tags LIKE '%WillowStandard%'"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [GlobalVariable] SET IsWillowStandard = 1 WHERE Tags LIKE '%WillowStandard%'"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWillowStandard",
                table: "RuleInstance");

            migrationBuilder.DropColumn(
                name: "IsWillowStandard",
                table: "Rule");

            migrationBuilder.DropColumn(
                name: "IsWillowStandard",
                table: "Insight");

            migrationBuilder.DropColumn(
                name: "IsWillowStandard",
                table: "GlobalVariable");
        }
    }
}
