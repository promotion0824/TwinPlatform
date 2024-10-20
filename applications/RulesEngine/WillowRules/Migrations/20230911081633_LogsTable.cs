using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class LogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
					Message = table.Column<string>(type: "varchar(4000)", nullable: true),
					Level = table.Column<string>(type: "varchar(100)", nullable: true),
					TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
					Exception = table.Column<string>(type: "nvarchar(max)", nullable: true),
					LogEvent = table.Column<string>(type: "nvarchar(max)", nullable: true),
					ProgressId = table.Column<string>(type: "varchar(450)", nullable: true),
					CorrelationId = table.Column<string>(type: "varchar(450)", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateIndex(
                name: "IX_Logs_ProgressId",
                table: "Logs",
                column: "ProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_TimeStamp",
                table: "Logs",
                column: "TimeStamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Logs");
        }
    }
}
