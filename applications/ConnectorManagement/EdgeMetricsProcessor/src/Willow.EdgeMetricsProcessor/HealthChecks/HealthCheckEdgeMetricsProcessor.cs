namespace Willow.EdgeMetricsProcessor.HealthChecks;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks;

/// <summary>
/// Health checks for telemetry processors.
/// </summary>
internal class HealthCheckEdgeMetricsProcessor : HealthCheckBase<EdgeMetricsProcessor>
{
    /// <summary>
    /// Use if there is a configuration issue with the processor.
    /// </summary>
    public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Edge Metrics Processor not able to start due to missing configuration");

    /// <summary>
    /// Use if the processor is unable to process message.
    /// </summary>
    public static readonly HealthCheckResult FailedToProcess = HealthCheckResult.Degraded("Edge Metrics Processor unable to process incoming message");
}
