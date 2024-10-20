namespace Willow.Api.Logging;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

/// <summary>
/// Extension methods for the <see cref="IHostBuilder"/> interface.
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    /// Configures the logging for the hostbuilder.
    /// </summary>
    /// <param name="hostBuilder">The input HostBuilder.</param>
    /// <param name="consoleLoggingEnabled">Whether or not console logging is enabled. Defaults to true.</param>
    /// <returns>The updated HostBuilder.</returns>
    public static IHostBuilder ConfigureLogging(this IHostBuilder hostBuilder, bool consoleLoggingEnabled = true)
    {
        return hostBuilder
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders();
                if (consoleLoggingEnabled)
                {
                    builder.AddConsole();
                }
            });
    }

    /// <summary>
    /// Configures the Serilog logging for the hostbuilder.
    /// </summary>
    /// <param name="hostBuilder">The instance of the HostBuilder.</param>
    /// <returns>The updated HostBuilder.</returns>
    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, logger) =>
        {
#pragma warning disable CA1305 // Specify IFormatProvider
            var loggerConfiguration = logger.ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console();
#pragma warning restore CA1305 // Specify IFormatProvider

            if (!context.HostingEnvironment.IsDevelopment())
            {
                loggerConfiguration.WriteTo.ApplicationInsights(
                    services.GetRequiredService<TelemetryConfiguration>(),
                    TelemetryConverter.Traces);
            }
        });
    }
}
