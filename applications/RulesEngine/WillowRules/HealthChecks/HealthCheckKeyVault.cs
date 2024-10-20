using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

namespace RulesEngine.Processor.Services;

/// <summary>
/// A health check for KeyVault
/// </summary>
public class HealthCheckKeyVault : HealthCheckBase<Willow.Rules.Repository.IBaseRepository>
{
	/// <summary>
	/// Not configured
	/// </summary>
	public static HealthCheckResult ConfigurationProblem(string uri) =>
		HealthCheckResult.Degraded($"Not configured, check KeyVaultUri '{uri}'");

	/// <summary>
	/// Missing secret
	/// </summary>
	public static readonly HealthCheckResult MissingSecret = HealthCheckResult.Degraded("Missing secret");

	/// <summary>
	/// Authorization failure
	/// </summary>
	public static readonly HealthCheckResult AuthorizationFailure = HealthCheckResult.Degraded("Authorization failure");
}


