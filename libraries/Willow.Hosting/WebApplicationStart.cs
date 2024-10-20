namespace Willow.Hosting.Web;

using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Willow.AppContext;
using Willow.HealthChecks;
using Willow.Telemetry;
using Willow.Telemetry.Web;

/// <summary>
/// Starts an application using the WebApplicationBuilder method.
/// </summary>
public static class WebApplicationStart
{
    private static readonly CancellationTokenSource AppStopping = new();

    /// <summary>
    /// Starts an application using the WebApplicationBuilder method.
    /// </summary>
    /// <remarks>
    /// Adds:
    /// * Willow Context and Open Telemetry Config.
    /// * Willow Health Checks.
    /// * Livez and Readyz health checks.
    /// * DI for the Default Azure Credential.
    /// * DI for Meter and MetricsAttributesHelper.
    /// </remarks>
    /// <param name="args">Command line arguments.</param>
    /// <param name="appName">The name of the worker system for health checks.</param>
    /// <param name="configure">A method that adds services and configures the application builder.</param>
    /// <param name="configureApp">A method that configures the web application.</param>
    /// <param name="configureHealthChecks">An optional function too add custom health checks.</param>
    /// <returns>0 if app exits cleanly, or a non-zero value if there is an error.</returns>
    public static int Run(string[] args, string appName, Action<WebApplicationBuilder> configure, Func<WebApplication, ValueTask> configureApp, Action<IHealthChecksBuilder>? configureHealthChecks)
    {
        try
        {
            var app = BuildApp(args, appName, configure, configureHealthChecks);

            app.UseForwardedHeaders();

            app.UseWillowContext(app.Configuration);

            configureApp(app).AsTask().Wait();

            app.UseWillowHealthChecks(appName, $"Health check for {appName}", AppStopping);

            app.Run();

            Console.WriteLine($"Completing {appName}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{appName} exited with exception: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Starts an application using the WebApplicationBuilder method.
    /// </summary>
    /// <remarks>
    /// Adds:
    /// * Willow Context and Open Telemetry Config.
    /// * Willow Health Checks.
    /// * Livez and Readyz health checks.
    /// * DI for the Default Azure Credential.
    /// * DI for Meter and MetricsAttributesHelper.
    /// </remarks>
    /// <param name="args">Command line arguments.</param>
    /// <param name="appName">The name of the worker system for health checks.</param>
    /// <param name="configure">A method that adds services and configures the application builder.</param>
    /// <param name="configureApp">A method that configures the web application.</param>
    /// <param name="configureHealthChecks">An optional function too add custom health checks.</param>
    /// <returns>0 if app exits cleanly, or a non-zero value if there is an error.</returns>
    public static async Task<int> RunAsync(string[] args, string appName, Action<WebApplicationBuilder> configure, Func<WebApplication, ValueTask> configureApp, Action<IHealthChecksBuilder>? configureHealthChecks)
    {
        try
        {
            var app = BuildApp(args, appName, configure, configureHealthChecks);

            app.UseForwardedHeaders();

            app.UseWillowContext(app.Configuration);

            await configureApp(app);

            app.UseWillowHealthChecks(appName, $"Health check for {appName}", AppStopping);

            await app.RunAsync().ConfigureAwait(false);

            Console.WriteLine($"Completing {appName}");

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{appName} exited with exception: {ex.Message}");
            return 1;
        }
    }

    private static WebApplication BuildApp(string[] args, string appName, Action<WebApplicationBuilder> configure, Action<IHealthChecksBuilder>? configureHealthChecks)
    {
        Console.WriteLine($"Starting {appName}");

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args });

        builder.Services.AddTokenCredential(builder.Environment, options => builder.Configuration.Bind("ManagedIdentityCredential", options));

        builder.Services.AddWillowContext(builder.Configuration);

        builder.Services.AddSingleton(new MetricsAttributesHelper(builder.Configuration));

        builder.Services.AddSingleton(_ =>
        {
            var willowContext = builder.Configuration.GetSection("WillowContext").Get<WillowContextOptions>();
            return new Meter(willowContext?.MeterOptions.Name ?? "Unknown", willowContext?.MeterOptions.Version ?? "Unknown");
        });

        builder.Services.AddSingleton<IMetricsCollector, MetricsCollector>();

        builder.Services.Configure<HostOptions>(hostOptions =>
        {
            // Do not stop the host when there is an unhandled exception
            hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        configure(builder);

        var healthChecksBuilder = builder.Services.AddWillowHealthChecks(appName)
                                                  .AddLivez()
                                                  .AddReadyz(AppStopping);

        configureHealthChecks?.Invoke(healthChecksBuilder);

        return builder.Build();
    }
}
