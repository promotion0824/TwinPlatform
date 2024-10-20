using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;


#nullable disable
namespace WillowRules.Migrations
{
    public partial class AddStatusToRuleInstance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "RuleInstance",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RuleInstance_Status",
                table: "RuleInstance",
                column: "Status");

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [dbo].[RuleInstance] SET [Status] = (CASE WHEN [Valid] = 0 THEN 1 WHEN [Valid] = 1 THEN 0 END);"
			});

			migrationBuilder.DropColumn(
				name: "Valid",
				table: "RuleInstance");
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RuleInstance_Status",
                table: "RuleInstance");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "RuleInstance");

            migrationBuilder.AddColumn<bool>(
                name: "Valid",
                table: "RuleInstance",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
