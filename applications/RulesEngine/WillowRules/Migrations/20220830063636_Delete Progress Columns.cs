using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class DeleteProgressColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelationshipCount",
                table: "Progress");

            migrationBuilder.DropColumn(
                name: "TotalRelationshipCount",
                table: "Progress");

            migrationBuilder.DropColumn(
                name: "TotalTwinCount",
                table: "Progress");

            migrationBuilder.DropColumn(
                name: "TwinCount",
                table: "Progress");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RelationshipCount",
                table: "Progress",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "TotalRelationshipCount",
                table: "Progress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalTwinCount",
                table: "Progress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "TwinCount",
                table: "Progress",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
