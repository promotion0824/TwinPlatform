using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace WillowRules.Services;

/// <summary>
/// Service to send data to ADX via Event Hub
/// https://willow.atlassian.net/wiki/spaces/INTEG/pages/1984823425/ADX+-+Customer+IoT+Environment+Setup
/// </summary>
/// <remarks>
/// Essentially just a channel that connects the processor to the background sender
/// </remarks>
public interface IEventHubService
{
	/// <summary>
	/// Gets the reader channel (used by the background service)
	/// </summary>
	ChannelReader<EventHubServiceDto> Reader { get; }

	/// <summary>
	/// Writes to the channel
	/// </summary>
	/// <param name="dt">The payload to write</param>
	/// <param name="waitOrSkipToken">An optional timeout token to skip writing to queue if it is already full</param>
	Task<bool> WriteAsync(EventHubServiceDto dt, CancellationToken? waitOrSkipToken = null);
}

/// <summary>
/// Service to send data to ADX via Event Hub
/// https://willow.atlassian.net/wiki/spaces/INTEG/pages/1984823425/ADX+-+Customer+IoT+Environment+Setup
/// </summary>
public class EventHubService : IEventHubService
{
	private readonly ILogger<EventHubService> logger;
	private readonly Channel<EventHubServiceDto> messageQueue;

	public const int MaxQueueCapacity = 100000;

	/// <summary>
	/// Creates a new <see cref="EventHubService" />
	/// </summary>
	public EventHubService(
		ILogger<EventHubService> logger)
	{
		this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this.messageQueue = Channel.CreateBounded<EventHubServiceDto>(new BoundedChannelOptions(MaxQueueCapacity)
		{
			//Increased performance
			SingleReader = true
		});

	}

	public ChannelReader<EventHubServiceDto> Reader => messageQueue.Reader;
	public ChannelWriter<EventHubServiceDto> Writer => messageQueue.Writer;

	public async Task<bool> WriteAsync(EventHubServiceDto dt, CancellationToken? waitOrSkipToken = null)
	{
		//optionally wait until space is avaible and cancel wait if token expires
		if (waitOrSkipToken is not null)
		{
			try
			{
				var spaceAvailable = await Writer.WaitToWriteAsync(waitOrSkipToken.Value);

				if (!spaceAvailable)
				{
					return false;
				}
			}
			catch(OperationCanceledException)
			{
				//we have waited to long for space to become available
				return false;
			}
		}

		await Writer.WriteAsync(dt);

		return true;
	}
}

/// <summary>
/// Dto for sending data to Event Hub
/// </summary>
[DebuggerDisplay("({SourceTimestamp},{ScalarValue})")]
public class EventHubServiceDto
{
	/// <summary>
	/// ConnectorId
	/// </summary>
	public string? ConnectorId { get; init; }

	/// <summary>
	/// TrendId - Currently Rule instance output trend ID
	/// </summary>
	public string? TrendId { get; init; }

    /// <summary>
	/// External Id - In future this is the only id that should be used and not TrendId
	/// </summary>
	public string? ExternalId { get; init; }

    /// <summary>
    /// SourceTimestamp - When instance was triggered
    /// </summary>
    public DateTime SourceTimestamp { get; init; }

	/// <summary>
	/// EnqueuedTimestamp - DateTime.UTCNow
	/// </summary>
	public DateTime EnqueuedTimestamp { get; init; }

	/// <summary>
	/// Value to write to ADX
	/// </summary>
	public object? ScalarValue { get; init; }
}
