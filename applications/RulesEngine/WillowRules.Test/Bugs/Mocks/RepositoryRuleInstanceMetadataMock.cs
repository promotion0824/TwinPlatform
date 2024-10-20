using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryRuleInstanceMetadataMock : RepositoryBaseMock<RuleInstanceMetadata>, IRepositoryRuleInstanceMetadata
{
	public Task<int> DeleteMetadataByRuleId(string ruleId, CancellationToken cancellationToken)
	{
		return Task.FromResult(0);
	}

	public Task<int> DeleteOrphanMetadata(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(0);
	}

	public Task<RuleInstanceMetadata> GetOrAdd(string ruleInstanceId)
	{
		throw new System.NotImplementedException();
	}
}
