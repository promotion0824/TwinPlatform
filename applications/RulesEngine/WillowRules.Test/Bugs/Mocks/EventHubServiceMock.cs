using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using WillowRules.Services;

namespace WillowRules.Test.Bugs.Mocks;

public class EventHubServiceMock : IEventHubService
{
	public List<EventHubServiceDto> Output { get; } = new List<EventHubServiceDto>();

	public ChannelReader<EventHubServiceDto> Reader => throw new NotImplementedException();

	public Task<bool> WriteAsync(EventHubServiceDto dt, CancellationToken? waitOrSkipToken = null)
	{
		Output.Add(dt);
		return Task.FromResult(true);
	}
}
