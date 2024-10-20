using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.Infrastructure.Entities.Migrations
{
    /// <inheritdoc />
    public partial class fixindex2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BuildingConnectors",
                schema: "wsup",
                table: "BuildingConnectors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BuildingConnectors",
                schema: "wsup",
                table: "BuildingConnectors",
                columns: new[] { "CustomerInstanceId", "BuildingId", "ConnectorId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BuildingConnectors",
                schema: "wsup",
                table: "BuildingConnectors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BuildingConnectors",
                schema: "wsup",
                table: "BuildingConnectors",
                columns: new[] { "BuildingId", "ConnectorId" });
        }
    }
}
