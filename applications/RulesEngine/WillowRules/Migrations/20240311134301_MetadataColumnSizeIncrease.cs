using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class MetadataColumnSizeIncrease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.AlterColumn<string>(
				name: "ExtensionData",
				table: "RuleMetadata",
				type: "varchar(max)",
				nullable: false,
				oldClrType: typeof(byte[]),
				oldType: "varchar(4000)",
				oldNullable: false);
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
