using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;
using Willow.Rules.Services;

namespace RulesEngine.Processor.Services;

/// <summary>
/// A health check for Command and Control Api
/// </summary>
public class HealthCheckCommandApi : HealthCheckBase<CommandService>
{
	/// <summary>
	/// Not configured
	/// </summary>
	/// <remarks>
	/// Changed to healthy because TPD installs don't have it
	/// </remarks>
	public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Healthy("Not configured");

	/// <summary>
	/// Failing calls
	/// </summary>
	public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}
