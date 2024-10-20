using System;
using Authorization.Migrator.Migrations.Operations.Providers;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;

namespace Authorization.Migrator.Migrations.Operations.OperationExtensions;

/// <summary>
/// Class that generate sql statements for the requested database operation
/// </summary>
internal static class DatabaseOperation
{
	/// <summary>
	/// Method to generate Sql statement for the requested database object
	/// </summary>
	/// <param name="migrationBuilder">Instance of the migration builder</param>
	/// <param name="targetName">Name of the database object</param>
	/// <returns>Sql Operation build with the generated sql</returns>
	/// <exception cref="NotImplementedException">When the Active Provider is not implemented</exception>
	public static OperationBuilder<SqlOperation> GenerateSqlFor(this MigrationBuilder migrationBuilder, string targetName)
	{
		switch (migrationBuilder.ActiveProvider)
		{
			case "Microsoft.EntityFrameworkCore.SqlServer":
				return migrationBuilder
					.Sql(SqlServerSqlProvider.GetSql(targetName));
			default:
				throw new NotImplementedException();
		}
	}
}

