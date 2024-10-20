#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;

namespace Willow.Api.Telemetry.TelemetryInitializer
{
    public class FrontdoorTelemetryInitializer : ITelemetryInitializer
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string XAzureFdid = "X-Azure-FDID";
        private const string XAzureRef = "X-Azure-Ref";

        public FrontdoorTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Initialize(ITelemetry telemetry)
        {
            if (!(telemetry is RequestTelemetry req)) return;
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;

                if (httpContext?.Request?.Headers?.TryGetValue(XAzureFdid, out var frontDoorId) ?? false)
                {
                    req.Context.GlobalProperties.TryAdd(XAzureFdid, frontDoorId);
                }


                if (httpContext?.Request?.Headers?.TryGetValue(XAzureRef, out var frontDoorRef) ?? false)
                {
                    req.Context.GlobalProperties.TryAdd(XAzureRef, frontDoorRef);
                }
            }
            catch (Exception)
            {
                // ignored don't break all Telemetry and requests when you can't get the frontdoor details
            }

        }
    }
}