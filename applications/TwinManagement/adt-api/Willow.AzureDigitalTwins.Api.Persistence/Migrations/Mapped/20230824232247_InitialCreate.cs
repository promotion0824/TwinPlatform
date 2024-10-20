using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Willow.AzureDigitalTwins.Api.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MappedEntries",
                columns: table => new
                {
                    MappedId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MappedModelId = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: false),
                    WillowModelId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ParentMappedId = table.Column<string>(type: "nvarchar(48)", maxLength: 48, nullable: true),
                    ParentWillowId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    WillowParentRel = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModelInformation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StatusNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    AuditInformation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TimeLastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappedEntries", x => x.MappedId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MappedEntries");
        }
    }
}
