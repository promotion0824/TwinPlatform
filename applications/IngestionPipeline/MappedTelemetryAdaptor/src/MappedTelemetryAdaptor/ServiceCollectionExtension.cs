namespace Willow.MappedTelemetryAdaptor;

using System.Diagnostics.Metrics;
using System.Reflection;

internal static class ServiceCollectionExtension
{
    public static IServiceCollection AddMeterSingletonIfNotExists(this IServiceCollection services)
    {
        var meterName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
        var meterVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "Unknown";
        var meter = new Meter(meterName, meterVersion);

        var meterAlreadyRegistered = services.Any(sd => sd.ServiceType == typeof(Meter));

        if (!meterAlreadyRegistered)
        {
            services.AddSingleton(meter);
        }

        return services;
    }
}
