#nullable enable
using System;
using System.Linq;
using System.Reflection;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Willow.Api.Telemetry.TelemetryInitializer
{
    public class RoleNameTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            // App services supply a website hostname value that includes the slot name in it
            var webSiteHostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
            var roleName = Environment.GetEnvironmentVariable("ROLE_NAME") ?? Environment.GetEnvironmentVariable("ApplicationInsights__CloudRoleName");


           if (!string.IsNullOrWhiteSpace(roleName))
            {
                telemetry.Context.Cloud.RoleName = roleName;
            }
            else if (!string.IsNullOrWhiteSpace(webSiteHostname))
            {
                telemetry.Context.Cloud.RoleName = webSiteHostname.Split(".").FirstOrDefault();
            }
            else
            {
                var assemblyName = Assembly.GetEntryAssembly()?.GetName();
                telemetry.Context.Cloud.RoleName = assemblyName?.Name;
            }
        }
    }
}
