using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;
using Willow.ServiceBus;

namespace RulesEngine.Processor.Services;

/// <summary>
/// A health check for Service Bus
/// </summary>
public class HealthCheckServiceBus : HealthCheckBase<MessageConsumer>
{
	/// <summary>
	/// Failing calls
	/// </summary>
	public static readonly HealthCheckResult ConnectionFailed = HealthCheckResult.Degraded("Failed to connect");
}


