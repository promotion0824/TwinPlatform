using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;
using Willow.Rules.Processor;

namespace RulesEngine.Processor.Services;

/// <summary>
/// A health check for the processor
/// </summary>
public class HealthCheckProcessor : HealthCheckBase<RuleExecutionProcessor>
{
	/// <summary>
	/// No rule instances
	/// </summary>
	public static readonly HealthCheckResult NoRuleInstances = HealthCheckResult.Healthy("No skill instances", data: extraData);
}
