using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.AzureDigitalTwins.Api.Persistence.Migrations.Jobs
{
    /// <inheritdoc />
    public partial class InitialCreateJobsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JobEntries",
                columns: table => new
                {
                    JobId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ParentJobId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    JobType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OutputsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProgressCurrentCount = table.Column<int>(type: "int", nullable: true),
                    ProgressTotalCount = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    InputsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserMessage = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ProgressStatusMessage = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CustomData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceResourceUri = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    TargetResourceUri = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false),
                    JobSubtype = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    TimeCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TimeLastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobEntries", x => x.JobId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobEntries_JobType_Status_IsDeleted",
                table: "JobEntries",
                columns: new[] { "JobType", "Status", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobEntries");
        }
    }
}
