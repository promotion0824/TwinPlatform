using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class ActorColumnCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Invocations",
                table: "Actors");

            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "Actors");

            migrationBuilder.DropColumn(
                name: "LastFaulted",
                table: "Actors");

            migrationBuilder.DropColumn(
                name: "RuleInstanceId",
                table: "Actors");

            migrationBuilder.DropColumn(
                name: "TimedValueOptions",
                table: "Actors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Invocations",
                table: "Actors",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "Actors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastFaulted",
                table: "Actors",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "RuleInstanceId",
                table: "Actors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimedValueOptions",
                table: "Actors",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
