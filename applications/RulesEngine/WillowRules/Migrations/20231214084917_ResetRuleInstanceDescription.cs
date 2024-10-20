using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class ResetRuleInstanceDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			//rule instnace description can only be used once expansion has run, for sites that hasn't run expansion (rule instnace Description = ""), it must fall back to rule description
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [RuleInstance] SET Description = ''"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
