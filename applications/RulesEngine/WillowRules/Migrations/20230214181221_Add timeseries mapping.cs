using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class Addtimeseriesmapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TimeSeriesMapping",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TrendId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ConnectorId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DtId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSeriesMapping", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeSeriesMapping_ConnectorId",
                table: "TimeSeriesMapping",
                column: "ConnectorId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSeriesMapping_ExternalId",
                table: "TimeSeriesMapping",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSeriesMapping_TrendId",
                table: "TimeSeriesMapping",
                column: "TrendId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimeSeriesMapping");
        }
    }
}
