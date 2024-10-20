using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Willow.Infrastructure.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddTeams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamLead",
                schema: "wsup",
                table: "Teams");

            migrationBuilder.UpdateData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 999999,
                column: "Description",
                value: "Team is Unknown.");

            migrationBuilder.InsertData(
                schema: "wsup",
                table: "Teams",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Cloud Operations", "CloudOps" },
                    { 2, "Activate Technology Team", "Activate Technology" },
                    { 3, "Advanced Analytics", "Advanced Analytics" },
                    { 4, "Connectors", "Connectors" },
                    { 5, "Core Services", "CoreServices" },
                    { 6, "Dashboards", "Dashboards" },
                    { 7, "Investa Experience", "InvestaExperience" },
                    { 8, "IoT Services", "IoTServices" },
                    { 9, "Search and Explore", "SearchAndExplore" },
                    { 10, "Security and Privacy", "Security" },
                    { 11, "Workflows", "Workflows" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.AddColumn<string>(
                name: "TeamLead",
                schema: "wsup",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "wsup",
                table: "Teams",
                keyColumn: "Id",
                keyValue: 999999,
                columns: new[] { "Description", "TeamLead" },
                values: new object[] { null, "Unknown" });
        }
    }
}
