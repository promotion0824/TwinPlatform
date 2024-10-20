using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryCalculatedPointMock : RepositoryBaseMock<CalculatedPoint>, IRepositoryCalculatedPoint
{
	public Task<int> DeleteBefore(DateTimeOffset date, CalculatedPointSource source = 0, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(0);
	}

	public Task<int> ScheduleDeleteCalculatedPointsByRuleId(string ruleId, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task<Batch<(CalculatedPoint calculatedPoint, RuleInstance instance, RuleInstanceMetadata metadata, ActorState actor)>> GetAllCombined(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<CalculatedPoint, bool>>? whereExpression = null,
		int? page = null, int? take = null)
	{
		throw new NotImplementedException();
	}

	public Task<(CalculatedPoint calculatedPoint, RuleInstance? instance, RuleInstanceMetadata? metadata, ActorState? actor)?> GetOneCombined(string id)
	{
		throw new NotImplementedException();
	}
}
