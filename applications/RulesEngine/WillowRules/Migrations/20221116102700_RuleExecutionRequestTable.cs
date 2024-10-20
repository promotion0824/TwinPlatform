using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class RuleExecutionRequestTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FailedReason",
                table: "Progress",
                type: "varchar(2000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Progress",
                type: "varchar(50)",
                nullable: false,
                defaultValue: "Completed");

            migrationBuilder.CreateTable(
                name: "RuleExecutionRequest",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerEnvironmentId = table.Column<string>(type: "varchar(1000)", nullable: false),
                    ProgressId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Command = table.Column<string>(type: "varchar(100)", nullable: false),
                    RequestedBy = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    RequestedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExtendedData = table.Column<string>(type: "varchar(1000)", nullable: false),
					Requested = table.Column<bool>(type: "bit", nullable: false)
				},
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuleExecutionRequest", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RuleExecutionRequest");

            migrationBuilder.DropColumn(
                name: "FailedReason",
                table: "Progress");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Progress");
        }
    }
}
