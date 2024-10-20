using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.AzureDigitalTwins.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMappedIdStringStatusEnumAuditNotReq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "MappedEntries",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(16)",
                oldMaxLength: 16,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuditInformation",
                table: "MappedEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.DropPrimaryKey("PK_MappedEntries", "MappedEntries");
            migrationBuilder.AlterColumn<string>(
                name: "MappedId",
                table: "MappedEntries",
                type: "nvarchar(48)",
                maxLength: 48,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
            migrationBuilder.AddPrimaryKey("PK_MappedEntries", "MappedEntries", "MappedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "MappedEntries",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AuditInformation",
                table: "MappedEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.DropPrimaryKey("PK_MappedEntries", "MappedEntries");
            migrationBuilder.AlterColumn<Guid>(
                name: "MappedId",
                table: "MappedEntries",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(48)",
                oldMaxLength: 48);
            migrationBuilder.AddPrimaryKey("PK_MappedEntries", "MappedEntries", "MappedId");
        }
    }
}
