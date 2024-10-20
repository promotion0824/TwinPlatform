using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;
using Willow.ServiceBus;

namespace RulesEngine.Processor.Services;

/// <summary>
/// A health check for ADX
/// </summary>
public class HealthCheckADX : HealthCheckBase<MessageConsumer>
{
	/// <summary>
	/// Failing calls
	/// </summary>
	public static readonly HealthCheckResult ConnectionFailed = HealthCheckResult.Degraded("Failed to connect");

	/// <summary>
	/// Rate limited
	/// </summary>
	public static readonly HealthCheckResult RateLimited = HealthCheckResult.Degraded("Rate limited");
}
