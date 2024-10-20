using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryCommandMock : RepositoryBaseMock<Command>, IRepositoryCommand
{
	public Task<int> EnableSync(string commandId, bool enabled)
	{
		return Task.FromResult(0);
	}

	public Task<IEnumerable<(string id, bool enabled, bool isTriggered)>> GetCommandValues()
	{
		return Task.FromResult(Data.Select(v => (v.Id, v.Enabled, v.IsTriggered)));
	}

	public Task<int> RemoveAllCommandsForRule(string ruleId)
	{
		throw new NotImplementedException();
	}
}
