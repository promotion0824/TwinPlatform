namespace Willow.Api.Logging.ApplicationInsights;

using System.Reflection;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

/// <summary>
/// Sets the version of the application in the telemetry.
/// </summary>
public class VersionTelemetryInitializer : ITelemetryInitializer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionTelemetryInitializer"/> class.
    /// </summary>
    /// <param name="telemetry">A telemetry instance.</param>
    public void Initialize(ITelemetry telemetry)
    {
        var assemblyName = Assembly.GetEntryAssembly()?.GetName();
        var version = assemblyName?.Version?.ToString();
        if (!string.IsNullOrWhiteSpace(version))
        {
            telemetry.Context.Component.Version = version;
        }
    }
}
