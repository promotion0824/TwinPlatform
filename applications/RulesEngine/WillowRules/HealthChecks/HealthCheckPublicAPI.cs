using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

namespace RulesEngine.Processor.Services;

/// <summary>
/// A health check for public api
/// </summary>
public class HealthCheckPublicAPI : HealthCheckBase<Willow.RealEstate.Command.Generated.CommandClient>
{
	/// <summary>
	/// Not configured
	/// </summary>
	public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Not configured");

	/// <summary>
	/// Rate limited
	/// </summary>
	public static readonly HealthCheckResult RateLimited = HealthCheckResult.Degraded("Rate limited");

	/// <summary>
	/// Failing calls
	/// </summary>
	public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");

	/// <summary>
	/// Not authorized
	/// </summary>
	public static readonly HealthCheckResult NotAuthorized = HealthCheckResult.Degraded("Not authorized");
}
