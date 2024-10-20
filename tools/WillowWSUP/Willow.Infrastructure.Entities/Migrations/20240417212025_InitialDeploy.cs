using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Willow.Infrastructure.Entities.Migrations
{
    /// <inheritdoc />
    public partial class InitialDeploy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "wsup");

            migrationBuilder.CreateTable(
                name: "ApplicationStatuses",
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
                    table.PrimaryKey("PK_ApplicationStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerInstanceApplicationStatuses",
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
                    table.PrimaryKey("PK_CustomerInstanceApplicationStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerInstanceStatuses",
                schema: "wsup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResourceGroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdxDatabaseUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdtInstanceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerInstanceStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerStatuses",
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
                    table.PrimaryKey("PK_CustomerStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Environments",
                schema: "wsup",
                columns: table => new
                {
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Environments", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                schema: "wsup",
                columns: table => new
                {
                    ShortName = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.ShortName);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                schema: "wsup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamLead = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "wsup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SalesId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerStatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customer_CustomerStatuses",
                        column: x => x.CustomerStatusId,
                        principalSchema: "wsup",
                        principalTable: "CustomerStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Stamps",
                schema: "wsup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnvironmentName = table.Column<string>(type: "nvarchar(50)", nullable: true),
                    RegionShortName = table.Column<string>(type: "nvarchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stamps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Stamps_Environments_EnvironmentName",
                        column: x => x.EnvironmentName,
                        principalSchema: "wsup",
                        principalTable: "Environments",
                        principalColumn: "Name");
                    table.ForeignKey(
                        name: "FK_Stamps_Regions_RegionShortName",
                        column: x => x.RegionShortName,
                        principalSchema: "wsup",
                        principalTable: "Regions",
                        principalColumn: "ShortName");
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                schema: "wsup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    ApplicationStatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Application_ApplicationStatuses",
                        column: x => x.ApplicationStatusId,
                        principalSchema: "wsup",
                        principalTable: "ApplicationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Application_Team",
                        column: x => x.TeamId,
                        principalSchema: "wsup",
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerInstances",
                schema: "wsup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ShortName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerInstanceStatusId = table.Column<int>(type: "int", nullable: false),
                    StampId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegionShortName = table.Column<string>(type: "nvarchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerInstance_Customer",
                        column: x => x.CustomerId,
                        principalSchema: "wsup",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerInstance_CustomerInstanceStatuses",
                        column: x => x.CustomerInstanceStatusId,
                        principalSchema: "wsup",
                        principalTable: "CustomerInstanceStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerInstance_Stamp",
                        column: x => x.StampId,
                        principalSchema: "wsup",
                        principalTable: "Stamps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerInstances_Regions_RegionShortName",
                        column: x => x.RegionShortName,
                        principalSchema: "wsup",
                        principalTable: "Regions",
                        principalColumn: "ShortName");
                });

            migrationBuilder.CreateTable(
                name: "CustomerInstanceApplications",
                schema: "wsup",
                columns: table => new
                {
                    CustomerInstanceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    CustomerInstanceApplicationStatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerInstanceApplications", x => new { x.CustomerInstanceId, x.ApplicationId });
                    table.ForeignKey(
                        name: "FK_CustomerInstanceApplication_Applications",
                        column: x => x.ApplicationId,
                        principalSchema: "wsup",
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerInstanceApplication_CustomerInstanceApplicationStatuses",
                        column: x => x.CustomerInstanceApplicationStatusId,
                        principalSchema: "wsup",
                        principalTable: "CustomerInstanceApplicationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerInstanceApplication_CustomerInstances",
                        column: x => x.CustomerInstanceId,
                        principalSchema: "wsup",
                        principalTable: "CustomerInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "wsup",
                table: "ApplicationStatuses",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Application is not active.", "Inactive" },
                    { 2, "Application is in Preview mode.", "Preview" },
                    { 3, "Application is active.", "Active" }
                });

            migrationBuilder.InsertData(
                schema: "wsup",
                table: "CustomerInstanceApplicationStatuses",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Application is not active.", "Inactive" },
                    { 2, "Application is active.", "Active" }
                });

            migrationBuilder.InsertData(
                schema: "wsup",
                table: "CustomerInstanceStatuses",
                columns: new[] { "Id", "AdtInstanceUrl", "AdxDatabaseUrl", "Description", "Name", "ResourceGroupName" },
                values: new object[,]
                {
                    { 1, null, null, "Customer instance is not yet active.", "Commissioning", null },
                    { 2, null, null, "Customer instance is active.", "Active", null },
                    { 3, null, null, "Customer instance is no longer active.", "Decommissioned", null }
                });

            migrationBuilder.InsertData(
                schema: "wsup",
                table: "CustomerStatuses",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Customer is not active.", "Inactive" },
                    { 2, "Customer is active.", "Active" }
                });

            migrationBuilder.InsertData(
                schema: "wsup",
                table: "Environments",
                columns: new[] { "Name", "Description" },
                values: new object[,]
                {
                    { "dev", "Development and Test Environment." },
                    { "prd", "Production Environment." },
                    { "sbx", "Sandbox environment for developer testing." }
                });

            migrationBuilder.InsertData(
                schema: "wsup",
                table: "Regions",
                columns: new[] { "ShortName", "DisplayName", "Name" },
                values: new object[,]
                {
                    { "aue", "Australia East", "australiaeast" },
                    { "eus", "US East", "eastus" },
                    { "eus2", "US East 2", "eastus2" },
                    { "weu", "West Europe", "westeurope" },
                    { "wus", "West US", "westus" },
                    { "wus2", "West US 2", "westus2" }
                });

            migrationBuilder.InsertData(
                schema: "wsup",
                table: "Teams",
                columns: new[] { "Id", "Description", "Name", "TeamLead" },
                values: new object[] { 999999, null, "Unknown", "Unknown" });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicationStatusId",
                schema: "wsup",
                table: "Applications",
                column: "ApplicationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_TeamId",
                schema: "wsup",
                table: "Applications",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInstanceApplications_ApplicationId",
                schema: "wsup",
                table: "CustomerInstanceApplications",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInstanceApplications_CustomerInstanceApplicationStatusId",
                schema: "wsup",
                table: "CustomerInstanceApplications",
                column: "CustomerInstanceApplicationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInstances_CustomerId",
                schema: "wsup",
                table: "CustomerInstances",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInstances_CustomerInstanceStatusId",
                schema: "wsup",
                table: "CustomerInstances",
                column: "CustomerInstanceStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInstances_RegionShortName",
                schema: "wsup",
                table: "CustomerInstances",
                column: "RegionShortName");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerInstances_StampId",
                schema: "wsup",
                table: "CustomerInstances",
                column: "StampId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerStatusId",
                schema: "wsup",
                table: "Customers",
                column: "CustomerStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Stamps_EnvironmentName_RegionShortName_Name",
                schema: "wsup",
                table: "Stamps",
                columns: new[] { "EnvironmentName", "RegionShortName", "Name" },
                unique: true,
                filter: "[EnvironmentName] IS NOT NULL AND [RegionShortName] IS NOT NULL AND [Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Stamps_RegionShortName",
                schema: "wsup",
                table: "Stamps",
                column: "RegionShortName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerInstanceApplications",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "Applications",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "CustomerInstanceApplicationStatuses",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "CustomerInstances",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "ApplicationStatuses",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "Teams",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "CustomerInstanceStatuses",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "Stamps",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "CustomerStatuses",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "Environments",
                schema: "wsup");

            migrationBuilder.DropTable(
                name: "Regions",
                schema: "wsup");
        }
    }
}
