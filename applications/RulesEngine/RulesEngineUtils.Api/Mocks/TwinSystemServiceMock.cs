using Abodit.Mutable;
using Willow.Rules.Model;
using Willow.Rules.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace RulesEngineUtils.Api.Mocks;

public class TwinSystemServiceMock : ITwinSystemService
{
	public Task<Graph<BasicDigitalTwinPoco, WillowRelation>> GetTwinSystemGraph(string[] twinIds)
	{
		throw new NotImplementedException();
	}
}
