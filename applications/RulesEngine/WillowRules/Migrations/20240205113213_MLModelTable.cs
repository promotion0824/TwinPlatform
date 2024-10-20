using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class MLModelTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MLModels",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(450)", nullable: false),
                    ModelName = table.Column<string>(type: "varchar(450)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(4000)", nullable: true),
                    ModelVersion = table.Column<string>(type: "varchar(100)", nullable: true),
                    FullName = table.Column<string>(type: "varchar(1000)", nullable: true),
                    ModelData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ExtensionData = table.Column<string>(type: "nvarchar(4000)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MLModels", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MLModels");
        }
    }
}
