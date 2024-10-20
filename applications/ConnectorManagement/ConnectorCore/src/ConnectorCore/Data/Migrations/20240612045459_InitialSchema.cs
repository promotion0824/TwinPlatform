#nullable disable

namespace ConnectorCore.Data.Migrations
{
    using System;
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConnectorType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ConnectorConfigurationSchemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceMetadataSchemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PointMetadataSchemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScanConfigurationSchemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectorType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Schema",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Connector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ClientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConnectorTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ErrorThreshold = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    IsLoggingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RegistrationKey = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LastImport = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "sysutcdatetime()"),
                    ConnectionType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: true, defaultValue: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connector", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Connector_ConnectorType_ConnectorTypeId",
                        column: x => x.ConnectorTypeId,
                        principalTable: "ConnectorType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SchemaColumn",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SchemaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: string.Empty),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchemaColumn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchemaColumn_Schema_SchemaId",
                        column: x => x.SchemaId,
                        principalTable: "Schema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PointCount = table.Column<int>(type: "int", nullable: false),
                    ErrorCount = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    Errors = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Logs_Connector_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "Connector",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Scan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DevicesToScan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorCount = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Configuration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scan", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Scan_Connector_ConnectorId",
                        column: x => x.ConnectorId,
                        principalTable: "Connector",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "Idx_Connector_ConnectorTypeId",
                table: "Connector",
                column: "ConnectorTypeId");

            migrationBuilder.CreateIndex(
                name: "Idx_Connector_SiteId",
                table: "Connector",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "Idx_ConnectorType_ConnectorConfigurationSchemaId",
                table: "ConnectorType",
                column: "ConnectorConfigurationSchemaId");

            migrationBuilder.CreateIndex(
                name: "Idx_ConnectorType_DeviceMetadataSchemaId",
                table: "ConnectorType",
                column: "DeviceMetadataSchemaId");

            migrationBuilder.CreateIndex(
                name: "Idx_ConnectorType_PointMetadataSchemaId",
                table: "ConnectorType",
                column: "PointMetadataSchemaId");

            migrationBuilder.CreateIndex(
                name: "Idx_ConnectorType_ScanConfigurationSchemaId",
                table: "ConnectorType",
                column: "ScanConfigurationSchemaId");

            migrationBuilder.CreateIndex(
                name: "Idx_Logs_ConnectorId",
                table: "Logs",
                column: "ConnectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_EndTime",
                table: "Logs",
                column: "EndTime")
                .Annotation("SqlServer:Include", new[] { "ConnectorId", "PointCount", "ErrorCount" });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_StartTime",
                table: "Logs",
                column: "StartTime")
                .Annotation("SqlServer:Include", new[] { "ConnectorId", "Source" });

            migrationBuilder.CreateIndex(
                name: "Idx_Scan_ConnectorId",
                table: "Scan",
                column: "ConnectorId");

            migrationBuilder.CreateIndex(
                name: "Idx_Schema_ClientId",
                table: "Schema",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_SchemaColumn_SchemaId",
                table: "SchemaColumn",
                column: "SchemaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");

            migrationBuilder.DropTable(
                name: "Scan");

            migrationBuilder.DropTable(
                name: "SchemaColumn");

            migrationBuilder.DropTable(
                name: "Connector");

            migrationBuilder.DropTable(
                name: "Schema");

            migrationBuilder.DropTable(
                name: "ConnectorType");
        }
    }
}
