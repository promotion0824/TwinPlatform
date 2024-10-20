using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
	/// <inheritdoc />
	public partial class ColumnSizeIncrease : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "ExtendedData",
				table: "RuleExecutionRequest",
				type: "varchar(max)",
				nullable: false,
				oldClrType: typeof(byte[]),
				oldType: "varchar(1000)",
				oldNullable: false);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
