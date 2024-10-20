using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryGlobalVariableMock : RepositoryBaseMock<GlobalVariable>, IRepositoryGlobalVariable
{
	public Task<List<string>> GetTags()
	{
		throw new System.NotImplementedException();
	}

	public Task<List<GlobalVariable>> MatchGlobalVariableReferences(string variableName)
	{
		throw new System.NotImplementedException();
	}
}
