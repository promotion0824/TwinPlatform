using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class SqlServerCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			//details for this EF migration script can be found here:
			//https://dejanstojanovic.net/aspnet/2020/december/setting-up-sql-server-idistributedcache-with-migration-in-aspnet-core/
			migrationBuilder.CreateTable(
			   name: "Cache",
			   columns: table => new
			   {
				   Id = table.Column<string>(type: "nvarchar(449)", maxLength: 449, nullable: false),
				   Value = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
				   ExpiresAtTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
				   SlidingExpirationInSeconds = table.Column<long>(type: "bigint", nullable: true),
				   AbsoluteExpiration = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
			   },
			   constraints: table =>
			   {
				   table.PrimaryKey($"PK_Cache", x => x.Id);
			   });

			migrationBuilder.CreateIndex(
			   name: $"IX_Cache_ExpiresAtTime",
			   table: "Cache",
			   column: "ExpiresAtTime");
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.DropTable(
			   name: "Cache");
		}
    }
}
