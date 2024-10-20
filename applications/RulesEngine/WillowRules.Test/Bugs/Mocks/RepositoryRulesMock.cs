using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryRulesMock : RepositoryBaseMock<Rule>, IRepositoryRules
{
	public Task<int> DeleteRuleById(string ruleId, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task<int> EnableSyncForRule(string ruleId, bool enabled)
	{
		throw new NotImplementedException();
	}

	public Task<Batch<(Rule rule, RuleMetadata metadata)>> GetAllCombined(
		SortSpecificationDto[] sortSpecifications,
		FilterSpecificationDto[] filterSpecifications,
		Expression<Func<Rule, bool>>? whereExpression = null,
		int? page = null, int? take = null)
	{
		throw new NotImplementedException();
	}

	public Task<List<string>> GetCategories()
	{
		throw new NotImplementedException();
	}

	public Task<List<string>> GetTags()
	{
		throw new NotImplementedException();
	}

	public Task<List<Rule>> MatchRuleReferences(string variableName)
	{
		throw new NotImplementedException();
	}

	public Task<int> SyncRuleWithADT(string ruleId, bool enabled)
	{
		throw new NotImplementedException();
	}
}
