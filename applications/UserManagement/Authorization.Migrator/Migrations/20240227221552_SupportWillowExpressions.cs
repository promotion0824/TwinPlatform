using Authorization.Migrator.Migrations.Operations.OperationExtensions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class SupportWillowExpressions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoleAssignments_RoleId_UserId_ResourceId",
                table: "RoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_GroupRoleAssignments_RoleId_GroupId_ResourceId",
                table: "GroupRoleAssignments");

            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "RoleAssignments");

            migrationBuilder.DropColumn(
                name: "ResourceId",
                table: "GroupRoleAssignments");

            migrationBuilder.AddColumn<string>(
                name: "Expression",
                table: "RoleAssignments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Expression",
                table: "GroupRoleAssignments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_RoleId_UserId_Expression",
                table: "RoleAssignments",
                columns: new[] { "RoleId", "UserId", "Expression" },
                unique: true,
                filter: "[Expression] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoleAssignments_RoleId_GroupId_Expression",
                table: "GroupRoleAssignments",
                columns: new[] { "RoleId", "GroupId", "Expression" },
                unique: true,
                filter: "[Expression] IS NOT NULL");

            migrationBuilder.GenerateSqlFor("GetRoleAssignmentsByUser_Down");
            migrationBuilder.GenerateSqlFor("GetRoleAssignmentsByUser_v1_Up");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoleAssignments_RoleId_UserId_Expression",
                table: "RoleAssignments");

            migrationBuilder.DropIndex(
                name: "IX_GroupRoleAssignments_RoleId_GroupId_Expression",
                table: "GroupRoleAssignments");

            migrationBuilder.DropColumn(
                name: "Expression",
                table: "RoleAssignments");

            migrationBuilder.DropColumn(
                name: "Expression",
                table: "GroupRoleAssignments");

            migrationBuilder.AddColumn<string>(
                name: "ResourceId",
                table: "RoleAssignments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResourceId",
                table: "GroupRoleAssignments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleAssignments_RoleId_UserId_ResourceId",
                table: "RoleAssignments",
                columns: new[] { "RoleId", "UserId", "ResourceId" },
                unique: true,
                filter: "[ResourceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoleAssignments_RoleId_GroupId_ResourceId",
                table: "GroupRoleAssignments",
                columns: new[] { "RoleId", "GroupId", "ResourceId" },
                unique: true,
                filter: "[ResourceId] IS NOT NULL");

            migrationBuilder.GenerateSqlFor("GetRoleAssignmentsByUser_v1_Down");
            migrationBuilder.GenerateSqlFor("GetRoleAssignmentsByUser_Up");
        }
    }
}
