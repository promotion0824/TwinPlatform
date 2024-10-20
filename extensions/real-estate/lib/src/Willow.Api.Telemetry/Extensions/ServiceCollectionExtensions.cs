#nullable enable
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Willow.Api.Telemetry.TelemetryInitializer;
using Willow.Api.Telemetry.TelemetryProcessor;

namespace Willow.Api.Telemetry.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureApplicationInsightsTelemetry(this IServiceCollection services, string? cloudRoleName = null)
        {
            services.AddHttpContextAccessor();
            
            services.AddApplicationInsightsTelemetry();
            
            // Telemetry filters
            services.AddApplicationInsightsTelemetryProcessor<HealthCheckTelemetryFilter>(); 
            
            // Telemetry property initializers
            services.AddSingleton<ITelemetryInitializer, VersionTelemetryInitializer>(); 
            services.AddSingleton<ITelemetryInitializer, UseridTelemetryInitializer>(); 
            services.AddSingleton<ITelemetryInitializer, FrontdoorTelemetryInitializer>();
            services.AddSingleton<ITelemetryInitializer, RoleNameTelemetryInitializer>();
        }
    }
}