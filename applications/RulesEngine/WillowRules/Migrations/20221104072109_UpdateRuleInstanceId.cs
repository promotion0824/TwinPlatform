using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace WillowRules.Migrations
{
    public partial class UpdateRuleInstanceId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
		{
			//update all related rule instance ids
			//OutputTrendId null check is to ignore Calculated Point records
			migrationBuilder.Operations.Add(new SqlOperation
			{
				Sql = @"
					update Actors set
					Id = ri.EquipmentId + '_' + ri.RuleId,
					RuleInstanceId = ri.EquipmentId + '_' + ri.RuleId,
					RuleId = ri.RuleId
					from Actors x
					inner join RuleInstance ri on ri.Id = x.Id
					where ri.OutputTrendId is null

					update RuleInstanceMetadata set
					Id = ri.EquipmentId + '_' + ri.RuleId
					from RuleInstanceMetadata x
					inner join RuleInstance ri on ri.Id = x.Id
					where ri.OutputTrendId is null

					update Insight set
					Id = ri.EquipmentId + '_' + ri.RuleId,
					RuleInstanceId = ri.EquipmentId + '_' + ri.RuleId,
					RuleId = ri.RuleId,
					RuleName = ri.RuleName
					from Insight x
					inner join RuleInstance ri on ri.Id = x.Id
					where ri.OutputTrendId is null

					update InsightImpactScore set
					Id = ri.EquipmentId + '_' + ri.RuleId + '_' + FieldId,
					InsightId = ri.EquipmentId + '_' + ri.RuleId
					from InsightImpactScore x
					inner join RuleInstance ri on ri.Id = x.InsightId
					where ri.OutputTrendId is null

					update RuleInstance set
					Id = EquipmentId + '_' + RuleId
					from RuleInstance ri
					where ri.OutputTrendId is null"
			});
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
