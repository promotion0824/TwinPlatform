using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.Infrastructure.Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCIA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerInstanceApplication_Applications",
                schema: "wsup",
                table: "CustomerInstanceApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerInstanceApplication_CustomerInstances",
                schema: "wsup",
                table: "CustomerInstanceApplications");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerInstanceApplication_Applications",
                schema: "wsup",
                table: "CustomerInstanceApplications",
                column: "ApplicationId",
                principalSchema: "wsup",
                principalTable: "Applications",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerInstanceApplication_CustomerInstances",
                schema: "wsup",
                table: "CustomerInstanceApplications",
                column: "CustomerInstanceId",
                principalSchema: "wsup",
                principalTable: "CustomerInstances",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerInstanceApplication_Applications",
                schema: "wsup",
                table: "CustomerInstanceApplications");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerInstanceApplication_CustomerInstances",
                schema: "wsup",
                table: "CustomerInstanceApplications");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerInstanceApplication_Applications",
                schema: "wsup",
                table: "CustomerInstanceApplications",
                column: "ApplicationId",
                principalSchema: "wsup",
                principalTable: "Applications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerInstanceApplication_CustomerInstances",
                schema: "wsup",
                table: "CustomerInstanceApplications",
                column: "CustomerInstanceId",
                principalSchema: "wsup",
                principalTable: "CustomerInstances",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
