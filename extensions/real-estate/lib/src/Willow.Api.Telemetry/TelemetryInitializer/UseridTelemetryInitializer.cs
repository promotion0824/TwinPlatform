#nullable enable
using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Willow.Api.Telemetry.TelemetryInitializer
{
    /// <summary>
    /// Enriches log and telemetry
    /// </summary>
    public class UseridTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UseridTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        
        public void Initialize(ITelemetry telemetry)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User == null || !httpContext.User.Claims.Any())
                    return;

                
                var nameIdentifier =  httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrWhiteSpace(nameIdentifier))
                {
                    telemetry.Context.User.Id = nameIdentifier;
                }

            }
            catch (Exception)
            {
                // ignored don't break all Telemetry and requests when you can't get the appId
            }
        }
    }
}
