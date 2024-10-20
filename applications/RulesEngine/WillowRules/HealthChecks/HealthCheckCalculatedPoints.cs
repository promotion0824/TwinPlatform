using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

namespace RulesEngine.Processor.Services;

/// <summary>
/// A health check for the event hub configuration and security settings
/// </summary>
public class HealthCheckCalculatedPoints : HealthCheckBase<string>
{
	/// <summary>
	/// No calculated points yet, considered healthy
	/// </summary>
	public static readonly HealthCheckResult NoCalculatedPoints = HealthCheckResult.Healthy("No calculated points processed yet");

	/// <summary>
	/// Authorization failure
	/// </summary>
	public static readonly HealthCheckResult AuthorizationFailure = HealthCheckResult.Degraded("Authorization failure");

	/// <summary>
	/// Not configured
	/// </summary>
	public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Not configured, check env vars");

	/// <summary>
	/// Integration Errors
	/// </summary>
	public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing Calls");
}
