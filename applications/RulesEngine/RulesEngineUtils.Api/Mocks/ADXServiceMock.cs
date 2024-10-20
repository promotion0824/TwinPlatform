using System.Threading.Channels;
using Willow.Rules.Model;
using Willow.Rules.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace RulesEngineUtils.Api.Mocks;

public class ADXServiceMock : IADXService
{
	public Task<(bool hasTwinChanges, bool hasRelationshipChanges)> HasADTChanges(DateTime startDate)
	{
		throw new NotImplementedException();
	}

	public (Task producer, ChannelReader<RawData> reader) RunRawQueryPaged(DateTime earliest, DateTime latestStop, IEnumerable<IdFilter>? idFilters = null, IEnumerable<string>? ruleIds = null, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}
