using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationId",
                table: "Permissions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "newsequentialid()"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SupportClientAuthentication = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                });

            // Populate Applications Table based on the existing permissions
            migrationBuilder.Sql("insert into Applications(Name,Description,SupportClientAuthentication) select Extension,'',0 from Permissions group by Extension");

            // Update Permission table Application Id based on the inserted applications
            migrationBuilder.Sql("UPDATE p SET p.ApplicationId = a.Id FROM Permissions p JOIN Applications a ON a.Name = p.Extension");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_ApplicationId",
                table: "Permissions",
                column: "ApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Permissions_Applications_ApplicationId",
                table: "Permissions",
                column: "ApplicationId",
                principalTable: "Applications",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permissions_Applications_ApplicationId",
                table: "Permissions");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_ApplicationId",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "Permissions");
        }
    }
}
