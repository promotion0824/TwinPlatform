using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.AzureDigitalTwins.Api.Persistence.Migrations.Jobs
{
    /// <inheritdoc />
    public partial class InitialUpdateJobsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorsJson",
                table: "JobEntries",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProcessingEndTime",
                table: "JobEntries",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ProcessingStartTime",
                table: "JobEntries",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorsJson",
                table: "JobEntries");

            migrationBuilder.DropColumn(
                name: "ProcessingEndTime",
                table: "JobEntries");

            migrationBuilder.DropColumn(
                name: "ProcessingStartTime",
                table: "JobEntries");
        }
    }
}
