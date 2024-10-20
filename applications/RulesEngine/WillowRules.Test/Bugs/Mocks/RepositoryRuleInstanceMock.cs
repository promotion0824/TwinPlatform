using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryRuleInstancesMock : RepositoryBaseMock<RuleInstance>, IRepositoryRuleInstances
{
	public Task<int> DeleteInstancesBefore(DateTimeOffset date, string ruleId, bool isCalculatedPointsOnly = false, CancellationToken cancellationToken = default)
	{
		return Task.FromResult(0);
	}

	public Task<int> DeleteInstancesByRuleId(string ruleId, CancellationToken cancellationToken)
	{
		return Task.FromResult(0);
	}

	public Task<Batch<(RuleInstance ruleInstance, RuleInstanceMetadata metadata)>> GetAllCombined(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<RuleInstance, bool>>? whereExpression = null,
		int? page = null, int? take = null)
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<(RuleInstance ruleInstance, RuleInstanceMetadata metadata)> GetAllCombinedAsync(SortSpecificationDto[] sortSpecifications, FilterSpecificationDto[] filterSpecifications, Expression<Func<RuleInstance, bool>>? whereExpression = null, int? page = null, int? take = null)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<RuleInstance>> GetByLocation(string location)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<RuleInstance>> GetByTrendId(string trendId)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<RuleInstance>> GetFedBy(string twinId)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<RuleInstance>> GetFeeds(string twinId)
	{
		throw new NotImplementedException();
	}

	public Task<string> GetFirstNonUTCInstance()
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<(string ruleId, string equipmentId)> GetRuleInstanceInfoForTwinLookup()
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<(string id, string equipmentId, string equipmentName, RuleInstanceStatus status)>> GetRuleInstanceList(string ruleId)
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<RuleInstance> GetRuleInstancesWithoutExpressions(Expression<Func<RuleInstance, bool>>? whereExpression = null)
	{
		throw new NotImplementedException();
	}

	public Task<int> SetDisabled(string ruleInstanceId, bool disabled)
	{
		throw new NotImplementedException();
	}
}
