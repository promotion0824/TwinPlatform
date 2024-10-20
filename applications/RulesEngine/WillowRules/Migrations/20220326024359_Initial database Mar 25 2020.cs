using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class InitialdatabaseMar252020 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ADTSummaries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AsOfDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CustomerEnvironmentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ADTInstanceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountTwins = table.Column<int>(type: "int", nullable: false),
                    CountCapabilities = table.Column<int>(type: "int", nullable: false),
                    CountRelationships = table.Column<int>(type: "int", nullable: false),
                    CountTwinsNotInGraph = table.Column<int>(type: "int", nullable: false),
                    CountModels = table.Column<int>(type: "int", nullable: false),
                    CountModelsInUse = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ADTSummaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalculatedPoints",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TrendId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValueExpression = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculatedPoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalVariable",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expression = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBuiltIn = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalVariable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Insight",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleRecomendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Locations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feeds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Occurrences = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleInstanceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PrimaryModelId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleTemplateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EquipmentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EquipmentUniqueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Cost = table.Column<double>(type: "float", nullable: false),
                    Comfort = table.Column<double>(type: "float", nullable: false),
                    Reliability = table.Column<double>(type: "float", nullable: false),
                    Invocations = table.Column<long>(type: "bigint", nullable: false),
                    CommandEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CommandInsightId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insight", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Progress",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Percentage = table.Column<double>(type: "float", nullable: false),
                    TwinCount = table.Column<long>(type: "bigint", nullable: false),
                    RelationshipCount = table.Column<long>(type: "bigint", nullable: false),
                    TotalTwinCount = table.Column<int>(type: "int", nullable: false),
                    TotalRelationshipCount = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Eta = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RuleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Speed = table.Column<double>(type: "float", nullable: false),
                    StartTimeSeriesTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CurrentTimeSeriesTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndTimeSeriesTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Progress", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rule",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recommendations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryModelId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Elements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rule_Rule_RuleId",
                        column: x => x.RuleId,
                        principalTable: "Rule",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RuleExecution",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerEnvironmentId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RuleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Generation = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Percentage = table.Column<double>(type: "float", nullable: false),
                    PercentageReported = table.Column<double>(type: "float", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CompletedEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TargetEndDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleExecution", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RuleInstance",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RuleId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleTemplate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrimaryModelId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PointEntityIds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleParametersBound = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Valid = table.Column<bool>(type: "bit", nullable: false),
                    Disabled = table.Column<bool>(type: "bit", nullable: false),
                    EquipmentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EquipmentUniqueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Locations = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Feeds = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutputTrendId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RuleCategory = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleInstance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RuleInstanceMetadata",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TriggerCount = table.Column<int>(type: "int", nullable: false),
                    LastTriggered = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleInstanceMetadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RuleMetadata",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ScanStarted = table.Column<bool>(type: "bit", nullable: false),
                    ScanComplete = table.Column<bool>(type: "bit", nullable: false),
                    ScanState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScanStateAsOf = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RuleInstanceCount = table.Column<int>(type: "int", nullable: false),
                    ValidInstanceCount = table.Column<int>(type: "int", nullable: false),
                    InsightsGenerated = table.Column<int>(type: "int", nullable: false),
                    ETag = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleMetadata", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Insight_Comfort",
                table: "Insight",
                column: "Comfort");

            migrationBuilder.CreateIndex(
                name: "IX_Insight_Cost",
                table: "Insight",
                column: "Cost");

            migrationBuilder.CreateIndex(
                name: "IX_Insight_EquipmentId",
                table: "Insight",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Insight_LastUpdated",
                table: "Insight",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_Insight_Reliability",
                table: "Insight",
                column: "Reliability");

            migrationBuilder.CreateIndex(
                name: "IX_Insight_RuleId",
                table: "Insight",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Progress_LastUpdated",
                table: "Progress",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_Rule_RuleId",
                table: "Rule",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleExecution_RuleId",
                table: "RuleExecution",
                column: "RuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleInstance_EquipmentId",
                table: "RuleInstance",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RuleInstance_PrimaryModelId",
                table: "RuleInstance",
                column: "PrimaryModelId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ADTSummaries");

            migrationBuilder.DropTable(
                name: "CalculatedPoints");

            migrationBuilder.DropTable(
                name: "GlobalVariable");

            migrationBuilder.DropTable(
                name: "Insight");

            migrationBuilder.DropTable(
                name: "Progress");

            migrationBuilder.DropTable(
                name: "Rule");

            migrationBuilder.DropTable(
                name: "RuleExecution");

            migrationBuilder.DropTable(
                name: "RuleInstance");

            migrationBuilder.DropTable(
                name: "RuleInstanceMetadata");

            migrationBuilder.DropTable(
                name: "RuleMetadata");
        }
    }
}
