using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
	public partial class ImpactScoreTable : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
				name: "RuleImpactScoresBound",
				table: "RuleInstance",
				type: "nvarchar(max)",
				nullable: true);

			//EF does not give null columns default config, we have to do it manually
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [RuleInstance] SET RuleImpactScoresBound = '[]'"
			});

			migrationBuilder.AddColumn<string>(
				name: "ImpactScores",
				table: "Rule",
				type: "nvarchar(max)",
				nullable: true);

			//EF does not give null columns default config, we have to do it manually
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "UPDATE [Rule] SET ImpactScores = '[]'"
			});

			migrationBuilder.CreateTable(
				name: "InsightImpactScore",
				columns: table => new
				{
					Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
					InsightId = table.Column<string>(type: "nvarchar(450)", nullable: true),
					Unit = table.Column<string>(type: "nvarchar(50)", nullable: true),
					Name = table.Column<string>(type: "nvarchar(450)", nullable: true),
					FieldId = table.Column<string>(type: "varchar(450)", nullable: true),
					Score = table.Column<double>(type: "float", nullable: false),
					BaseScore = table.Column<double>(type: "float", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_InsightImpactScore", x => x.Id);
					//table.ForeignKey(
					//    name: "FK_InsightImpactScore_Insight_InsightId",
					//    column: x => x.InsightId,
					//    principalTable: "Insight",
					//    principalColumn: "Id");
				});

			migrationBuilder.CreateIndex(
				name: "IX_InsightImpactScore_InsightId",
				table: "InsightImpactScore",
				column: "InsightId");

			migrationBuilder.CreateIndex(
				name: "IX_InsightImpactScore_Name",
				table: "InsightImpactScore",
				column: "Name");

			migrationBuilder.CreateIndex(
				name: "IX_InsightImpactScore_Score",
				table: "InsightImpactScore",
				column: "Score");

			migrationBuilder.CreateIndex(
				name: "IX_InsightImpactScore_BaseScore",
				table: "InsightImpactScore",
				column: "BaseScore");

			//sql data migrate here
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "INSERT INTO InsightImpactScore(Id, InsightId, Name, FieldId, Score, BaseScore) SELECT Id + '_cost_impact', Id, 'Cost impact', 'cost_impact', Cost, Cost FROM Insight"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "INSERT INTO InsightImpactScore(Id, InsightId, Name, FieldId, Score, BaseScore) SELECT Id + '_comfort_impact', Id, 'Comfort impact', 'comfort_impact', Comfort, Comfort FROM Insight"
			});

			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = "INSERT INTO InsightImpactScore(Id, InsightId, Name, FieldId, Score, BaseScore) SELECT Id + '_reliability_impact', Id, 'Reliability impact', 'reliability_impact', Reliability, Reliability FROM Insight"
			});

			migrationBuilder.DropIndex(
				name: "IX_Insight_Comfort",
				table: "Insight");

			migrationBuilder.DropIndex(
				name: "IX_Insight_Cost",
				table: "Insight");

			migrationBuilder.DropIndex(
				name: "IX_Insight_Reliability",
				table: "Insight");

			migrationBuilder.DropColumn(
				name: "Comfort",
				table: "Insight");

			migrationBuilder.DropColumn(
				name: "Cost",
				table: "Insight");

			migrationBuilder.DropColumn(
				name: "Reliability",
				table: "Insight");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "InsightImpactScore");

			migrationBuilder.DropColumn(
				name: "RuleImpactScoresBound",
				table: "RuleInstance");

			migrationBuilder.DropColumn(
				name: "ImpactScores",
				table: "Rule");

			migrationBuilder.AddColumn<double>(
				name: "Comfort",
				table: "Insight",
				type: "float",
				nullable: false,
				defaultValue: 0.0);

			migrationBuilder.AddColumn<double>(
				name: "Cost",
				table: "Insight",
				type: "float",
				nullable: false,
				defaultValue: 0.0);

			migrationBuilder.AddColumn<double>(
				name: "Reliability",
				table: "Insight",
				type: "float",
				nullable: false,
				defaultValue: 0.0);

			migrationBuilder.CreateIndex(
				name: "IX_Insight_Comfort",
				table: "Insight",
				column: "Comfort");

			migrationBuilder.CreateIndex(
				name: "IX_Insight_Cost",
				table: "Insight",
				column: "Cost");

			migrationBuilder.CreateIndex(
				name: "IX_Insight_Reliability",
				table: "Insight",
				column: "Reliability");
		}
	}
}
