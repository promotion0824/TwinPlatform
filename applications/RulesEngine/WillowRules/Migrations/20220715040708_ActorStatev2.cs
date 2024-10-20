using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class ActorStatev2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeSeriesBuffer",
                table: "TimeSeriesBuffer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActorState",
                table: "ActorState");

            migrationBuilder.RenameTable(
                name: "TimeSeriesBuffer",
                newName: "TimeSeries");

            migrationBuilder.RenameTable(
                name: "ActorState",
                newName: "Actors");

            migrationBuilder.RenameIndex(
                name: "IX_ActorState_RuleId",
                table: "Actors",
                newName: "IX_Actors_RuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeSeries",
                table: "TimeSeries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Actors",
                table: "Actors",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeSeries",
                table: "TimeSeries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Actors",
                table: "Actors");

            migrationBuilder.RenameTable(
                name: "TimeSeries",
                newName: "TimeSeriesBuffer");

            migrationBuilder.RenameTable(
                name: "Actors",
                newName: "ActorState");

            migrationBuilder.RenameIndex(
                name: "IX_Actors_RuleId",
                table: "ActorState",
                newName: "IX_ActorState_RuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeSeriesBuffer",
                table: "TimeSeriesBuffer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActorState",
                table: "ActorState",
                column: "Id");
        }
    }
}
