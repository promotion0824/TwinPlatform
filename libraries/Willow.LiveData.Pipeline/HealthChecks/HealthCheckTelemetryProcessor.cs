namespace Willow.LiveData.Pipeline;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

/// <summary>
/// Health checks for telemetry processors.
/// </summary>
public class HealthCheckTelemetryProcessor : HealthCheckBase<ITelemetryProcessor>
{
    /// <summary>
    /// Use if there is a configuration issue with the processor.
    /// </summary>
    public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Telemetry Processor not able to start due to missing configuration");

    /// <summary>
    /// Use if the processor is unable to send telemetry.
    /// </summary>
    public static readonly HealthCheckResult FailedToSend = HealthCheckResult.Degraded("Telemetry Processor unable to send processed telemetry");

    /// <summary>
    /// Use if the processor is unable to process telemetry.
    /// </summary>
    public static readonly HealthCheckResult FailedToProcess = HealthCheckResult.Degraded("Telemetry Processor unable to process telemetry");
}
