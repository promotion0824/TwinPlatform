using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;
using Willow.ServiceBus;

namespace RulesEngine.Processor.Services;

/// <summary>
/// A health check for ADT
/// </summary>
public class HealthCheckADT : HealthCheckBase<MessageConsumer>
{
	public static HealthCheckResult HealthyWithCounts(int twins, int relationships) =>
		HealthCheckResult.Healthy($"Healthy t={twins} r={relationships}");

	/// <summary>
	/// Failing calls
	/// </summary>
	public static readonly HealthCheckResult ConnectionFailed = HealthCheckResult.Degraded("Failed to connect");
}
