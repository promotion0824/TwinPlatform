namespace Willow.CommandAndControl.Application.Services;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

/// <summary>
/// A health check for command executor service.
/// </summary>
internal class HealthCheckCommandExecutor : HealthCheckBase<PeriodicCommandExecutorJob>
{
    /// <summary>
    /// Not Enabled.
    /// </summary>
    public static readonly HealthCheckResult NotEnabled = HealthCheckResult.Degraded("Command executor is not enabled");

    /// <summary>
    /// Failed to execute command.
    /// </summary>
    public static readonly HealthCheckResult FailedToExecuteCommand = HealthCheckResult.Degraded("Failed to execute command");
}
