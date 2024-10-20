using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddClientAssignmentConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClientAssignments_ApplicationClientId",
                table: "ClientAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssignments_ApplicationClientId_Expression",
                table: "ClientAssignments",
                columns: new[] { "ApplicationClientId", "Expression" },
                unique: true,
                filter: "[Expression] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ClientAssignments_ApplicationClientId_Expression",
                table: "ClientAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_ClientAssignments_ApplicationClientId",
                table: "ClientAssignments",
                column: "ApplicationClientId");
        }
    }
}
