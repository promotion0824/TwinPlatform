using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Willow.Infrastructure.Entities.Migrations
{
    /// <inheritdoc />
    public partial class ConnectorsRedo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingConnectorStatuses",
                schema: "wsup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingConnectorStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Buildings",
                schema: "wsup",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Building_CustomerInstance",
                        column: x => x.CustomerInstanceId,
                        principalSchema: "wsup",
                        principalTable: "CustomerInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConnectorStatuses",
                schema: "wsup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectorStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConnectorTypes",
                schema: "wsup",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direction = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectorTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Connectors",
                schema: "wsup",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConnectorStatusId = table.Column<int>(type: "int", nullable: false),
                    ConnectorTypeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Connector_ConnectorStatuses",
                        column: x => x.ConnectorStatusId,
                        principalSchema: "wsup",
                        principalTable: "ConnectorStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Connector_ConnectorTypes",
                        column: x => x.ConnectorTypeId,
                        principalSchema: "wsup",
                        principalTable: "ConnectorTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Connector_CustomerInstance",
                        column: x => x.CustomerInstanceId,
                        principalSchema: "wsup",
                        principalTable: "CustomerInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingConnectors",
                schema: "wsup",
                columns: table => new
                {
                    BuildingId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConnectorId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BuildingConnectorStatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingConnectors", x => new { x.BuildingId, x.ConnectorId });
                    table.ForeignKey(
                        name: "FK_BuildingConnector_Building",
                        column: x => x.BuildingId,
                        principalSchema: "wsup",
                        principalTable: "Buildings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BuildingConnector_BuildingConnectorStatuses",
                        column: x => x.BuildingConnectorStatusId,
                        principalSchema: "wsup",
                        principalTable: "BuildingConnectorStatuses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BuildingConnector_Connector",
                        column: x => x.ConnectorId,
                        principalSchema: "wsup",
                        principalTable: "Connectors",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                schema: "wsup",
                table: "BuildingConnectorStatuses",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Connector is in the process of being deployed to this building.", "Commissioning" },
                    { 2, "Building Connector is active.", "Active" },
                    { 3, "Connector is temporarily offline for this building.", "Offline" },
                    { 4, "Connector is now disabled for this building.", "Disabled" }
                });

            migrationBuilder.InsertData(
                schema: "wsup",
                table: "ConnectorStatuses",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Connector is in development.", "In Development" },
                    { 2, "Connector is active.", "Active" },
                    { 3, "Connector is no longer active.", "Inactive" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingConnectors_BuildingConnectorStatusId",
                schema: "wsup",
                table: "BuildingConnectors",
                column: "BuildingConnectorStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingConnectors_ConnectorId",
                schema: "wsup",
                table: "BuildingConnectors",
                column: "ConnectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_CustomerInstanceId",
                schema: "wsup",
                table: "Buildings",
                column: "CustomerInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Connectors_ConnectorStatusId",
                schema: "wsup",
                table: "Connectors",
                column: "ConnectorStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Connectors_ConnectorTypeId",
                schema: "wsup",
                table: "Connectors",
                column: "ConnectorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Connectors_CustomerInstanceId",
                schema: "wsup",
                table: "Connectors",
                column: "CustomerInstanceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingConnectors",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "Buildings",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "BuildingConnectorStatuses",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "Connectors",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "ConnectorStatuses",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "ConnectorTypes",
                schema: "wsup");
        }
    }
}
