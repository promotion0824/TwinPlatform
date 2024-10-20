using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class CommandSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CommandsGenerated",
                table: "RuleMetadata",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RuleTriggersBound",
                table: "RuleInstance",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RuleTriggers",
                table: "Rule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CommandId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CommandName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CommandType = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TwinId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TwinName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EquipmentName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EquipmentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    RuleId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RuleInstanceId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RuleName = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(100)", nullable: true),
                    IsTriggered = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastSyncDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Occurrences = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    PrimaryModelId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commands_EquipmentId",
                table: "Commands",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Commands_RuleId",
                table: "Commands",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Commands_RuleInstanceId",
                table: "Commands",
                column: "RuleInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Commands_TwinId",
                table: "Commands",
                column: "TwinId");

			//EF does not give null columns default config, we have to do it manually
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [Rule] SET RuleTriggers = '[]'"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [RuleInstance] SET RuleTriggersBound = '[]'"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropColumn(
                name: "CommandsGenerated",
                table: "RuleMetadata");

            migrationBuilder.DropColumn(
                name: "RuleTriggersBound",
                table: "RuleInstance");

            migrationBuilder.DropColumn(
                name: "RuleTriggers",
                table: "Rule");
        }
    }
}
