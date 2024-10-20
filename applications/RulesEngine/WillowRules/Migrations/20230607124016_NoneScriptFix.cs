using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    /// <inheritdoc />
    public partial class NoneScriptFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = @"
					update TimeSeriesMapping set
					ConnectorId = null
					from TimeSeriesMapping x
					where ConnectorId = 'NONE'

					update TimeSeriesMapping set
					ExternalId = null
					from TimeSeriesMapping x
					where ExternalId = 'NONE'

					update TimeSeries set
					ConnectorId = null
					from TimeSeries x
					where ConnectorId = 'NONE'

					update TimeSeries set
					ExternalId = null
					from TimeSeries x
					where ExternalId = 'NONE'"
			});
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
