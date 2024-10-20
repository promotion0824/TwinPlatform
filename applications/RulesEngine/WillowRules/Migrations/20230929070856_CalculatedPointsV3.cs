using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WillowRules.Migrations
{
	/// <inheritdoc />
	public partial class CalculatedPointsV3 : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "OutputExternalId",
				table: "RuleInstance",
				type: "nvarchar(512)",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "RelatedModelId",
				table: "RuleInstance",
				type: "nvarchar(512)",
				nullable: true);

			migrationBuilder.AddColumn<bool>(
				name: "ADTEnabled",
				table: "Rule",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<string>(
				name: "RelatedModelId",
				table: "Rule",
				type: "nvarchar(512)",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "ActionRequired",
				table: "CalculatedPoints",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<int>(
				name: "ActionStatus",
				table: "CalculatedPoints",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<string>(
				name: "ConnectorID",
				table: "CalculatedPoints",
				type: "nvarchar(512)",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "ExternalId",
				table: "CalculatedPoints",
				type: "nvarchar(512)",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "IsCapabilityOf",
				table: "CalculatedPoints",
				type: "nvarchar(512)",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "RuleId",
				table: "CalculatedPoints",
				type: "nvarchar(512)",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "Source",
				table: "CalculatedPoints",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<string>(
				name: "TimeZone",
				table: "CalculatedPoints",
				type: "nvarchar(255)",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "TrendInterval",
				table: "CalculatedPoints",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<int>(
				name: "Type",
				table: "CalculatedPoints",
				type: "int",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AlterColumn<string>(
				name: "PrimaryModelId",
				table: "Rule",
				maxLength: 512,
				nullable: true
			);

			migrationBuilder.AlterColumn<string>(
				name: "TemplateId",
				table: "Rule",
				maxLength: 512,
				nullable: true
			);

			migrationBuilder.AlterColumn<string>(
				name: "ModelId",
				table: "CalculatedPoints",
				maxLength: 512,
				nullable: true
			);

			migrationBuilder.CreateIndex(
				name: "IX_Rule_TemplateId",
				table: "Rule",
				column: "TemplateId");

			migrationBuilder.CreateIndex(
				name: "IX_CalculatedPoints_RuleId",
				table: "CalculatedPoints",
				column: "RuleId");

			migrationBuilder.CreateIndex(
				name: "IX_CalculatedPoints_ModelId",
				table: "CalculatedPoints",
				column: "ModelId");

			migrationBuilder.CreateIndex(
				name: "IX_CalculatedPoints_IsCapabilityOf",
				table: "CalculatedPoints",
				column: "IsCapabilityOf");

			migrationBuilder.CreateIndex(
				name: "IX_CalculatedPoints_ActionRequired",
				table: "CalculatedPoints",
				column: "ActionRequired");

			migrationBuilder.CreateIndex(
				name: "IX_CalculatedPoints_ActionStatus",
				table: "CalculatedPoints",
				column: "ActionStatus");

			migrationBuilder.CreateIndex(
				name: "IX_CalculatedPoints_Source",
				table: "CalculatedPoints",
				column: "Source");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "OutputExternalId",
				table: "RuleInstance");

			migrationBuilder.DropColumn(
				name: "RelatedModelId",
				table: "RuleInstance");

			migrationBuilder.DropColumn(
				name: "ADTEnabled",
				table: "Rule");

			migrationBuilder.DropColumn(
				name: "RelatedModelId",
				table: "Rule");

			migrationBuilder.DropColumn(
				name: "ActionRequired",
				table: "CalculatedPoints");

			migrationBuilder.DropColumn(
				name: "ActionStatus",
				table: "CalculatedPoints");

			migrationBuilder.DropColumn(
				name: "ConnectorID",
				table: "CalculatedPoints");

			migrationBuilder.DropColumn(
				name: "ExternalId",
				table: "CalculatedPoints");

			migrationBuilder.DropColumn(
				name: "IsCapabilityOf",
				table: "CalculatedPoints");

			migrationBuilder.DropColumn(
				name: "RuleId",
				table: "CalculatedPoints");

			migrationBuilder.DropColumn(
				name: "Source",
				table: "CalculatedPoints");

			migrationBuilder.DropColumn(
				name: "TimeZone",
				table: "CalculatedPoints");

			migrationBuilder.DropColumn(
				name: "TrendInterval",
				table: "CalculatedPoints");

			migrationBuilder.DropColumn(
				name: "Type",
				table: "CalculatedPoints");

			migrationBuilder.AddColumn<bool>(
				name: "IsNonMonotonicAscending",
				table: "TimeSeries",
				type: "bit",
				nullable: false,
				defaultValue: false);

			migrationBuilder.AddColumn<string>(
				name: "MonotonicAscendingEstimator",
				table: "TimeSeries",
				type: "nvarchar(max)",
				nullable: true);
		}
	}
}
