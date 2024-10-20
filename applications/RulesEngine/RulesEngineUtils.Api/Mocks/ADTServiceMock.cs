using Willow.Rules.Services;
using Willow.Rules.Sources;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace RulesEngineUtils.Api.Mocks;

public class ADTServiceMock : IADTService
{
	public ADTInstance[] AdtInstances => new ADTInstance[0];
}
