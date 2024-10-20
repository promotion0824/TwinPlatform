using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class RuleInstanceTagsColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "RuleInstanceMetadata",
                type: "nvarchar(max)",
                nullable: true);

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [RuleInstanceMetadata] SET Tags = '[]'"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                table: "RuleInstanceMetadata");
        }
    }
}
