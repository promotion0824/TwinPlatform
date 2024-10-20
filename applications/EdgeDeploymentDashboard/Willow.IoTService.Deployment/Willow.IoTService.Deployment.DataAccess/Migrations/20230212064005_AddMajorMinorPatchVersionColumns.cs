using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.IoTService.Deployment.DataAccess.Migrations
{
    public partial class AddMajorMinorPatchVersionColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ModuleTypeVersions_Version",
                table: "ModuleTypeVersions");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ModuleTypeVersions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "Major",
                table: "ModuleTypeVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Minor",
                table: "ModuleTypeVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Patch",
                table: "ModuleTypeVersions",
                type: "int",
                nullable: true);

            if (migrationBuilder.IsSqlServer())
            {
                migrationBuilder.Sql(@"
                    Update ModuleTypeVersions
                    SET 
                       Major = CAST(SUBSTRING(version, 1, CHARINDEX('.', version) - 1) AS INT),
                       Minor = CAST(SUBSTRING(version, CHARINDEX('.', version) + 1, CHARINDEX('.', version, CHARINDEX('.', version) + 1) - CHARINDEX('.', version) - 1) AS INT),
                       Patch = CAST(SUBSTRING(version, CHARINDEX('.', version, CHARINDEX('.', version) + 1) + 1, LEN(version)) AS INT)
                ");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Major",
                table: "ModuleTypeVersions");

            migrationBuilder.DropColumn(
                name: "Minor",
                table: "ModuleTypeVersions");

            migrationBuilder.DropColumn(
                name: "Patch",
                table: "ModuleTypeVersions");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                table: "ModuleTypeVersions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleTypeVersions_Version",
                table: "ModuleTypeVersions",
                column: "Version");
        }
    }
}
