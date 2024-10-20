using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

namespace RulesEngine.Processor.Services;

/// <summary>
/// A health check for ADT Api
/// </summary>
public class HealthCheckADTApi : HealthCheckBase<string>
{
	/// <summary>
	/// Authorization failure
	/// </summary>
	public static readonly HealthCheckResult AuthorizationFailure = HealthCheckResult.Degraded("Authorization failure");

	/// <summary>
	/// Not configured
	/// </summary>
	public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Not configured, check env vars");

	/// <summary>
	/// Failing calls
	/// </summary>
	public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}
