using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryRuleMetadataMock : RepositoryBaseMock<RuleMetadata>, IRepositoryRuleMetadata
{
	public Task<int> DeleteMetadataByRuleId(string ruleId, CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task<RuleMetadata> GetOrAdd(string ruleId, string? user = null)
	{
		var existing = Data.Find(v => v.Id == ruleId);

		if (existing is null)
		{
			existing = new RuleMetadata(ruleId, user)
			{
				Id = ruleId,
				ScanComplete = false,
				ScanStarted = false,
				ScanState = ScanState.Unknown,
				ScanStateAsOf = DateTimeOffset.Now,
				RuleInstanceCount = 0,
				ValidInstanceCount = 0,
				InsightsGenerated = 0,
				ETag = "METADATA ETAG NOT SET YET"
			};
		}

		return Task.FromResult(existing);
	}

	public Task<RuleMetadata> RuleUpdated(string ruleId, string user)
	{
		return GetOrAdd(ruleId, user);
	}
}
