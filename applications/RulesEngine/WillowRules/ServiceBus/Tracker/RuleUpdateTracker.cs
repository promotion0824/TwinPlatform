using Willow.Rules.Model;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Willow.Rules.Repository;
using System.Threading;
using Willow.Rules.Logging;
using System;

namespace Willow.ServiceBus;

/// <summary>
/// Tracks rule updates (metadata)
/// </summary>
/// <remarks>
/// Singleton across app. Only responsible for incrementing epoch now
/// </remarks>
public interface IRuleUpdateTracker : IMessageHandler
{
}

/// <summary>
/// Tracks rule updates (metadata)
/// </summary>
/// <remarks>
/// Singleton across app. Only responsible for incrementing epoch now
/// </remarks>
public class RuleUpdateTracker : BaseHandler<RuleUpdatedMessage>, IRuleUpdateTracker
{
	private readonly IEpochTracker epochTracker;
	private readonly ILogger throttledLogger;

	/// <summary>
	/// Creates a new <see cref="RuleUpdateTracker" />
	/// </summary>
	public RuleUpdateTracker(
		IEpochTracker epochTracker,
		ILogger<RuleUpdateTracker> logger) : base(logger)
	{
		this.epochTracker = epochTracker ?? throw new System.ArgumentNullException(nameof(epochTracker));
		throttledLogger = logger.Throttle(TimeSpan.FromSeconds(15));
	}

	public override Task<bool> Handle(RuleUpdatedMessage rer, CancellationToken cancellationToken)
	{
		throttledLogger.LogInformation("Rule update from service bus {entityId}", rer.EntityId);
		epochTracker.InvalidateCache();
		return Task.FromResult(true);
	}

	public override Task Initialize()
	{
		return Task.CompletedTask;
	}
}
