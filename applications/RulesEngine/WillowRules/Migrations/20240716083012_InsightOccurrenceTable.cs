using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class InsightOccurrenceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Occurrences",
                table: "Insight");

            migrationBuilder.CreateTable(
                name: "InsightOccurrence",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(50)", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    IsFaulted = table.Column<bool>(type: "bit", nullable: false),
                    Started = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Ended = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InsightId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
				{
					table.PrimaryKey("PK_InsightOccurrence", x => x.Id);
					//faster writes without FK
					//table.ForeignKey(
					//    name: "FK_InsightOccurrence_Insight_InsightId",
					//    column: x => x.InsightId,
					//    principalTable: "Insight",
					//    principalColumn: "Id");
				});

            migrationBuilder.CreateIndex(
                name: "IX_InsightOccurrence_Ended",
                table: "InsightOccurrence",
                column: "Ended");

            migrationBuilder.CreateIndex(
                name: "IX_InsightOccurrence_InsightId",
                table: "InsightOccurrence",
                column: "InsightId");

			migrationBuilder.CreateIndex(
				 name: "IX_InsightOccurrence_InsightId_Ended",
				 table: "InsightOccurrence",
				 columns: new[] { "InsightId", "Ended" });
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InsightOccurrence");

            migrationBuilder.AddColumn<byte[]>(
                name: "Occurrences",
                table: "Insight",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
