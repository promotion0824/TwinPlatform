using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class StatusUpdateScript : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			//convert to Flag (bit) enums
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = @"
					  update [RuleInstance]
					  set Status = 4
					  where Status = 2
					  
					  update [RuleInstance]
					  set Status = 2
					  where Status = 1

					  update [RuleInstance]
					  set Status = 1
					  where Status = 0"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
