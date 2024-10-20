using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.Infrastructure.Entities.Migrations
{
    /// <inheritdoc />
    public partial class fixindex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingConnector_Building",
                schema: "wsup",
                table: "BuildingConnectors");

            migrationBuilder.DropForeignKey(
                name: "FK_BuildingConnector_Connector",
                schema: "wsup",
                table: "BuildingConnectors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Connectors",
                schema: "wsup",
                table: "Connectors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Buildings",
                schema: "wsup",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_BuildingConnectors_ConnectorId",
                schema: "wsup",
                table: "BuildingConnectors");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerInstanceId",
                schema: "wsup",
                table: "BuildingConnectors",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Connectors",
                schema: "wsup",
                table: "Connectors",
                columns: new[] { "Id", "CustomerInstanceId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Buildings",
                schema: "wsup",
                table: "Buildings",
                columns: new[] { "Id", "CustomerInstanceId" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingConnectors_BuildingId_CustomerInstanceId",
                schema: "wsup",
                table: "BuildingConnectors",
                columns: new[] { "BuildingId", "CustomerInstanceId" });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingConnectors_ConnectorId_CustomerInstanceId",
                schema: "wsup",
                table: "BuildingConnectors",
                columns: new[] { "ConnectorId", "CustomerInstanceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingConnector_Building",
                schema: "wsup",
                table: "BuildingConnectors",
                columns: new[] { "BuildingId", "CustomerInstanceId" },
                principalSchema: "wsup",
                principalTable: "Buildings",
                principalColumns: new[] { "Id", "CustomerInstanceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingConnector_Connector",
                schema: "wsup",
                table: "BuildingConnectors",
                columns: new[] { "ConnectorId", "CustomerInstanceId" },
                principalSchema: "wsup",
                principalTable: "Connectors",
                principalColumns: new[] { "Id", "CustomerInstanceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingConnector_Building",
                schema: "wsup",
                table: "BuildingConnectors");

            migrationBuilder.DropForeignKey(
                name: "FK_BuildingConnector_Connector",
                schema: "wsup",
                table: "BuildingConnectors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Connectors",
                schema: "wsup",
                table: "Connectors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Buildings",
                schema: "wsup",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_BuildingConnectors_BuildingId_CustomerInstanceId",
                schema: "wsup",
                table: "BuildingConnectors");

            migrationBuilder.DropIndex(
                name: "IX_BuildingConnectors_ConnectorId_CustomerInstanceId",
                schema: "wsup",
                table: "BuildingConnectors");

            migrationBuilder.DropColumn(
                name: "CustomerInstanceId",
                schema: "wsup",
                table: "BuildingConnectors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Connectors",
                schema: "wsup",
                table: "Connectors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Buildings",
                schema: "wsup",
                table: "Buildings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingConnectors_ConnectorId",
                schema: "wsup",
                table: "BuildingConnectors",
                column: "ConnectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingConnector_Building",
                schema: "wsup",
                table: "BuildingConnectors",
                column: "BuildingId",
                principalSchema: "wsup",
                principalTable: "Buildings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingConnector_Connector",
                schema: "wsup",
                table: "BuildingConnectors",
                column: "ConnectorId",
                principalSchema: "wsup",
                principalTable: "Connectors",
                principalColumn: "Id");
        }
    }
}
