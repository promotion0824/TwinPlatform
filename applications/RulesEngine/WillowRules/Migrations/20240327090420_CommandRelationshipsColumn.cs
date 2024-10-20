using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class CommandRelationshipsColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Relationships",
                table: "Commands",
                type: "nvarchar(max)",
                nullable: true);

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [Commands] SET Relationships = '[]'"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Relationships",
                table: "Commands");
        }
    }
}
