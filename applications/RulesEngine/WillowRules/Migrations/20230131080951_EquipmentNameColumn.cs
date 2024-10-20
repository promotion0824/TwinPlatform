using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class EquipmentNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EquipmentName",
                table: "RuleInstance",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentName",
                table: "Insight",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RuleInstance_EquipmentName",
                table: "RuleInstance",
                column: "EquipmentName");

            migrationBuilder.CreateIndex(
                name: "IX_Insight_EquipmentName",
                table: "Insight",
                column: "EquipmentName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RuleInstance_EquipmentName",
                table: "RuleInstance");

            migrationBuilder.DropIndex(
                name: "IX_Insight_EquipmentName",
                table: "Insight");

            migrationBuilder.DropColumn(
                name: "EquipmentName",
                table: "RuleInstance");

            migrationBuilder.DropColumn(
                name: "EquipmentName",
                table: "Insight");
        }
    }
}
