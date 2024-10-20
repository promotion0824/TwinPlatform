using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Willow.HealthChecks;

/// <summary>
/// Filter out successful health check based request telemetry
/// </summary>
/// <remarks>
/// Usage: services.AddApplicationInsightsTelemetryProcessor&lt;HealthCheckTelemetryFilter&gt;();
/// </remarks>
public class HealthCheckTelemetryFilter : ITelemetryProcessor
{
    private readonly ITelemetryProcessor next;

    private readonly List<string> healthCheckPaths = new List<string>
    {
        "/health", "/healthz", "/healthcheck", "/liveness",
        "/readiness", "/livez", "/readyz", "/status", "/ping",
    };

    /// <summary>
    /// Creates a new telemetry filter that filters out any healthcheck related endpoint
    /// </summary>
    public HealthCheckTelemetryFilter(ITelemetryProcessor next)
    {
        this.next = next;
    }

    /// <summary>
    /// Processes the telemetry stopping processing for any healthcheck request
    /// </summary>
    public void Process(ITelemetry item)
    {
        if (item is RequestTelemetry { Success: true } req &&
            this.healthCheckPaths.Exists(p => req.Url.PathAndQuery.StartsWith(p)))
        {
            return;
        }

        this.next.Process(item);
    }
}
