using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryActorStateMock : RepositoryBaseMock<ActorState>, IRepositoryActorState
{
	public Task<int> DeleteActorsByRuleId(string ruleId, CancellationToken cancellationToken)
	{
		throw new System.NotImplementedException();
	}

	public Task<int> DeleteOrphanActors(CancellationToken cancellationToken = default)
	{
		return Task.FromResult(0);
	}

	public IAsyncEnumerable<ActorState> GetAllActors()
	{
		return Data.ToAsyncEnumerable();
	}

	public Task RemoveAll()
	{
		throw new System.NotImplementedException();
	}
}
