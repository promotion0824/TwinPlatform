using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class MetadataReviewColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comments",
                table: "RuleInstanceMetadata",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "RuleInstanceMetadata",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CapabilityCount",
                table: "RuleInstance",
                type: "int",
                nullable: false,
                defaultValue: 0);

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [RuleInstanceMetadata] SET Comments = '[]'"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comments",
                table: "RuleInstanceMetadata");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "RuleInstanceMetadata");

            migrationBuilder.DropColumn(
                name: "CapabilityCount",
                table: "RuleInstance");
        }
    }
}
