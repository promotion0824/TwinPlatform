using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryInsightMock : RepositoryBaseMock<Insight>, IRepositoryInsight
{
	private RepositoryRuleInstancesMock repositoryRuleInstances;

	public RepositoryInsightMock(RepositoryRuleInstancesMock repositoryRuleInstances)
	{
		this.repositoryRuleInstances = repositoryRuleInstances ?? throw new ArgumentNullException(nameof(repositoryRuleInstances));
	}

	public Task<int> DeleteOldImpactScores()
	{
		return Task.FromResult(0);
	}

	public Task<IEnumerable<Insight>> GetByLocation(string location)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<(string id, bool enabled, Guid commandId, InsightStatus status, DateTimeOffset lastSyncDate)>> GetCommandValues()
	{
		return Task.FromResult(Data.Select(v => (v.Id, v.CommandEnabled, v.CommandInsightId, v.Status, v.LastSyncDateUTC)));
	}

	public Task<IEnumerable<(string equipmentId, int count)>> GetEquipmentIds()
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<Insight>> GetFedBy(string twinId)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<Insight>> GetFeeds(string twinId)
	{
		throw new NotImplementedException();
	}

	public Task<List<(string FieldId, string Name, string[] Units, int Count)>> GetImpactScoreNames()
	{
		throw new NotImplementedException();
	}

	public Task<List<(string FieldId, string Name, string[] Units, int Count)>> GetImpactScoreNames(string ruleId)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<Insight>> GetInsightsForRule(string ruleId)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<(Guid siteId, int count)>> GetSiteIds()
	{
		throw new NotImplementedException();
	}

	public Task RemoveAll()
	{
		throw new NotImplementedException();
	}

	public Task<int> RemoveAllInsightsForRule(string ruleId)
	{
		throw new NotImplementedException();
	}

	public Task<int> SetCommandInsightId(string insightId, Guid commandInsightId, InsightStatus status)
	{
		throw new NotImplementedException();
	}

	public Task<int> SyncRuleInsightsWithCommand(string ruleId, bool enabled)
	{
		throw new NotImplementedException();
	}

	public Task<int> SyncWithCommand(string insightId, bool enabled)
	{
		throw new NotImplementedException();
	}

	public IAsyncEnumerable<Insight> GetOrphanedInsights()
	{
		//this logic is copied from the real repo
		var result = (from insight in Data
					  join ruleInstance in repositoryRuleInstances.Data
						  on insight.Id equals ruleInstance.Id into insights
					  from ruleInstance in insights.DefaultIfEmpty() //This line indicates a left join
					  where (ruleInstance == null) || ruleInstance.Status.HasFlag(RuleInstanceStatus.FilterApplied)//delete filtered instances immediately
					  select insight);

		return result.ToList().ToAsyncEnumerable();
	}

	public Task DisableAllInsights()
	{
		throw new NotImplementedException();
	}

	public Task<int> CheckValidInsights()
	{
		return Task.FromResult(0);
	}

	public Task<int> GetInsightCountForRule(string ruleId)
	{
		return Task.FromResult(Data.Count(v => v.RuleId == ruleId));
	}

	public Task<int> DeleteOccurrencesBefore(DateTimeOffset date)
	{
		return Task.FromResult(0);
	}

	public Task<int> DeleteOccurrences(int maxCount)
	{
		return Task.FromResult(0);
	}

	public Task DeleteOccurrences(string ruleId = "")
	{
		return Task.CompletedTask;
	}
}
