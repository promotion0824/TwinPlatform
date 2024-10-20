using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class GlobalizationforRules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LanguageDescriptions",
                table: "Rule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LanguageNames",
                table: "Rule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LanguageRecommendations",
                table: "Rule",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguageDescriptions",
                table: "Rule");

            migrationBuilder.DropColumn(
                name: "LanguageNames",
                table: "Rule");

            migrationBuilder.DropColumn(
                name: "LanguageRecommendations",
                table: "Rule");
        }
    }
}
