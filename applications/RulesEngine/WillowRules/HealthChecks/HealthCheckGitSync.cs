using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

namespace RulesEngine.Processor.Services;

/// <summary>
/// Health check for git sync
/// </summary>
public class HealthCheckGitSync : HealthCheckBase<string>
{
	/// <summary>
	/// Working directory is up to date with remote fork
	/// </summary>
	public static readonly HealthCheckResult UpToDate = HealthCheckResult.Healthy("Up to date with remote fork");

	/// <summary>
	/// Not configured
	/// </summary>
	public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Not configured, check that Github URI is defined");

	/// <summary>
	/// Authorization failure
	/// </summary>
	public static readonly HealthCheckResult AuthorizationFailure = HealthCheckResult.Degraded("Authorization failure, check that PAT is valid");

	/// <summary>
	/// Illegal arguments in git sync request
	/// </summary>
	public static readonly HealthCheckResult IllegalRequest = HealthCheckResult.Degraded("Illegal arguments in git sync request");
}
