using System;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryRuleExecutionsMock : RepositoryBaseMock<RuleExecution>, IRepositoryRuleExecutions
{
	public Task<RuleExecution> GetOneByRuleId(string ruleId)
	{
		throw new System.NotImplementedException();
	}

	public string IdGen(string customerEnvironmentId, string ruleId)
	{
		throw new System.NotImplementedException();
	}

	public Task<RuleExecution> MergeWorkItem(RuleExecutionRequest ruleExecutionRequest)
	{
		var startDate = (DateTimeOffset)ruleExecutionRequest.StartDate!.Value.ToUniversalTime();
		var endDate = (DateTimeOffset)ruleExecutionRequest.TargetEndDate!.Value.ToUniversalTime();

		var ruleExecution = new RuleExecution
		{
			Id = ruleExecutionRequest.CorrelationId,
			CustomerEnvironmentId = "mock",
			RuleId = ruleExecutionRequest.RuleId,
			StartDate = startDate,
			TargetEndDate = endDate,
			Percentage = 0.0,
			Generation = Guid.NewGuid(),
			PercentageReported = 0.0,
			CompletedEndDate = startDate
		};

		return Task.FromResult(ruleExecution);
	}
}
