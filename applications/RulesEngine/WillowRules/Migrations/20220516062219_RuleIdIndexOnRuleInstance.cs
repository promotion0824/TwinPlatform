using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class RuleIdIndexOnRuleInstance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.AlterColumn<string>("RuleId", "RuleInstance", type: "nvarchar(450)");

			migrationBuilder.CreateIndex(
				  name: $"IX_RuleInstance_RuleId",
				  table: "RuleInstance",
				  column: "RuleId");

		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
