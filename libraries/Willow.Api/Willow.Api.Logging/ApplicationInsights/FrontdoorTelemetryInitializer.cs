namespace Willow.Api.Logging.ApplicationInsights;

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

/// <summary>
/// The frontdoor telemetry initializer.
/// </summary>
public class FrontdoorTelemetryInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor? httpContextAccessor;
    private const string XAzureFdid = "X-Azure-FDID";
    private const string XAzureRef = "X-Azure-Ref";

    /// <summary>
    /// Initializes a new instance of the <see cref="FrontdoorTelemetryInitializer"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">An instance of the httpContextAccessor.</param>
    public FrontdoorTelemetryInitializer(IHttpContextAccessor? httpContextAccessor = null)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Initializes the telemetry to add the frontdoor headers.
    /// </summary>
    /// <param name="telemetry">The telemetry entry to add the headers to.</param>
    public void Initialize(ITelemetry telemetry)
    {
        if (httpContextAccessor is null)
        {
            return;
        }

        if (!(telemetry is RequestTelemetry req))
        {
            return;
        }

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext?.Request?.Headers?.TryGetValue(XAzureFdid, out var frontDoorId) ?? false)
        {
            req.Context.GlobalProperties.TryAdd(XAzureFdid, frontDoorId);
        }

        if (httpContext?.Request?.Headers?.TryGetValue(XAzureRef, out var frontDoorRef) ?? false)
        {
            req.Context.GlobalProperties.TryAdd(XAzureRef, frontDoorRef);
        }
    }
}
