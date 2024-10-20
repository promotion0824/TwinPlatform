using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class PointsColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Points",
                table: "Insight",
                type: "nvarchar(max)",
                nullable: true);

            //EF does not give null columns default config, we have to do it manually
            migrationBuilder.Operations.Add(new SqlOperation
            {
                Sql = "UPDATE [Insight] SET Points = '[]'"
            });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Points",
                table: "Insight");
        }
    }
}
