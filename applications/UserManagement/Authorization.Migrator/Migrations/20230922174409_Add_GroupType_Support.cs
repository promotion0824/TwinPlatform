using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Authorization.TwinPlatform.Persistence.Types;

#nullable disable

namespace Authorization.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class Add_GroupType_Support : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "GroupTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupTypes_Name",
                table: "GroupTypes",
                column: "Name",
                unique: true);

            var groupTypes = Enum.GetNames<GroupTypeNames>().ToDictionary(k => Guid.NewGuid(), v => v);

            foreach (var groupType in groupTypes)
            {
                migrationBuilder.Sql($"INSERT INTO GroupTypes (Id,Name) VALUES ('{groupType.Key}','{groupType.Value}')");
            }

            migrationBuilder.AddColumn<Guid>(
                name: "GroupTypeId",
                table: "Groups",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: groupTypes.First(f => f.Value == Enum.GetName<GroupTypeNames>(GroupTypeNames.Application)).Key);

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_GroupTypes_GroupTypeId",
                table: "Groups",
                column: "GroupTypeId",
                principalTable: "GroupTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_GroupTypeId",
                table: "Groups",
                column: "GroupTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_GroupTypes_GroupTypeId",
                table: "Groups");

            migrationBuilder.DropTable(
                name: "GroupTypes");

            migrationBuilder.DropIndex(
                name: "IX_Groups_GroupTypeId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "GroupTypeId",
                table: "Groups");
        }
    }
}
