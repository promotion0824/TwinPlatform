namespace Willow.Api.Logging.ApplicationInsights;

using System.Security.Claims;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

/// <summary>
/// The userid telemetry initializer.
/// </summary>
public class UseridTelemetryInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor? httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="UseridTelemetryInitializer"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The input IHttpContextAccessor.</param>
    public UseridTelemetryInitializer(IHttpContextAccessor? httpContextAccessor = null)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Initializes the telemetry to add the user id.
    /// </summary>
    /// <param name="telemetry">The telemetry instance to process.</param>
    public void Initialize(ITelemetry telemetry)
    {
        var httpContext = httpContextAccessor?.HttpContext;
        if (httpContext?.User == null || !httpContext.User.Claims.Any())
        {
            return;
        }

        var nameIdentifier = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(nameIdentifier))
        {
            telemetry.Context.User.Id = nameIdentifier;
        }

        var accountIdentifier = httpContext.User.Claims.FirstOrDefault(c => c.Type == "identityProvider" || c.Type == "identityProviders")?.Value;

        if (!string.IsNullOrWhiteSpace(accountIdentifier))
        {
            telemetry.Context.User.AccountId = accountIdentifier;
        }
    }
}
