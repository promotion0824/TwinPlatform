using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

namespace Willow.TwinLifecycleManagement.Web.Diagnostic;

/// <summary>
/// A health check for MTI.
/// </summary>
public class HealthCheckMTI : HealthCheckBase<string>
{
    /// <summary>
    /// Failing calls.
    /// </summary>
    public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}
