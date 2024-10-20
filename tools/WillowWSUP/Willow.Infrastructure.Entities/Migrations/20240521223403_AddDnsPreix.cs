using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.Infrastructure.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddDnsPreix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                schema: "wsup",
                table: "CustomerInstances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DnsEnvSuffix",
                schema: "wsup",
                table: "CustomerInstances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullDomain",
                schema: "wsup",
                table: "CustomerInstances",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasPublicEndpoint",
                schema: "wsup",
                table: "Applications",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayName",
                schema: "wsup",
                table: "CustomerInstances");

            migrationBuilder.DropColumn(
                name: "DnsEnvSuffix",
                schema: "wsup",
                table: "CustomerInstances");

            migrationBuilder.DropColumn(
                name: "FullDomain",
                schema: "wsup",
                table: "CustomerInstances");

            migrationBuilder.DropColumn(
                name: "HasPublicEndpoint",
                schema: "wsup",
                table: "Applications");
        }
    }
}
