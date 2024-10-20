using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Services;

namespace Willow.ServiceBus;

/// <summary>
/// Tracks heart beat messages from processor
/// </summary>
/// <remarks>
/// Singleton across app
/// </remarks>
public interface IHeartBeatTracker : IMessageHandler
{
	DateTimeOffset LastSeen { get; }
}

/// <summary>
/// Internal state store that receives progress from Rule Execution engine
/// </summary>
/// <remarks>
/// Instantiated as a singleton to communicate between the background service and the web app
/// register it as an IMessageHandler also
/// </remarks>
public class HeartBeatTracker : BaseHandler<HeartBeatMessage>, IHeartBeatTracker
{
	private readonly ITelemetryCollector telemetryCollector;

	public DateTimeOffset LastSeen { get; set; } = DateTimeOffset.MinValue;

	public HeartBeatTracker(ITelemetryCollector telemetryCollector, ILogger<HeartBeatTracker> logger) : base(logger)
	{
		this.telemetryCollector = telemetryCollector ?? throw new ArgumentNullException(nameof(telemetryCollector));
	}

	public override Task<bool> Handle(HeartBeatMessage message, CancellationToken cancellationToken)
	{
		logger.LogDebug("Heartbeat from Processor");

		LastSeen = DateTimeOffset.Now;

		return Task.FromResult(true);
	}

	public override Task Initialize()
	{
		return Task.CompletedTask;
	}
}
