using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddClientAssignmentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    ApplicationClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Expression = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Condition = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientAssignments_ApplicationClients_ApplicationClientId",
                        column: x => x.ApplicationClientId,
                        principalTable: "ApplicationClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientAssignmentPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    ClientAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientAssignmentPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientAssignmentPermissions_ClientAssignments_ClientAssignmentId",
                        column: x => x.ClientAssignmentId,
                        principalTable: "ClientAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientAssignmentPermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssignmentPermissions_ClientAssignmentId",
                table: "ClientAssignmentPermissions",
                column: "ClientAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssignmentPermissions_PermissionId_ClientAssignmentId",
                table: "ClientAssignmentPermissions",
                columns: new[] { "PermissionId", "ClientAssignmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssignments_ApplicationClientId",
                table: "ClientAssignments",
                column: "ApplicationClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientAssignmentPermissions");

            migrationBuilder.DropTable(
                name: "ClientAssignments");
        }
    }
}
