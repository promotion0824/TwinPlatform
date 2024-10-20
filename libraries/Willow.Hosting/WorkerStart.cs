namespace Willow.Hosting.Worker;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Willow.Hosting.Web;

/// <summary>
/// Starts up a hosted worker process and adds health checks.
/// </summary>
public static class WorkerStart
{
    /// <summary>
    /// Creates and configures a worker application, builds and runs.
    /// </summary>
    /// <remarks>
    /// Adds:
    /// * Willow Context and Open Telemetry Config.
    /// * Willow Health Checks.
    /// * Livez and Readyz health checks.
    /// * DI for the Default Azure Credential.
    /// </remarks>
    /// <param name="args">Command line arguments.</param>
    /// <param name="appName">The name of the worker system for health checks.</param>
    /// <param name="configureServices">A function to configure dependency injection.</param>
    /// <param name="configureHealthChecks">An optional function to add custom health checks.</param>
    /// <param name="configureBuilder">An optional function to customise the host builder.</param>
    /// <returns>0 if app exits cleanly, or a non-zero value if there is an error.</returns>
    public static int Run(string[] args, string appName, Action<IWebHostEnvironment, IConfiguration, IServiceCollection> configureServices, Action<IHealthChecksBuilder>? configureHealthChecks = null, Action<IHostBuilder>? configureBuilder = null) =>
        WebApplicationStart.Run(
            args,
            appName,
            (builder) =>
            {
                configureServices(builder.Environment, builder.Configuration, builder.Services);
                configureBuilder?.Invoke(builder.Host);
            },
            (_) => ValueTask.CompletedTask,
            configureHealthChecks);

    /// <summary>
    /// Creates and configures a worker application, builds and runs.
    /// </summary>
    /// <remarks>
    /// Adds:
    /// * Willow Context and Open Telemetry Config.
    /// * Willow Health Checks.
    /// * Livez and Readyz health checks.
    /// * DI for the Default Azure Credential.
    /// </remarks>
    /// <param name="args">Command line arguments.</param>
    /// <param name="appName">The name of the worker system for health checks.</param>
    /// <param name="configureServices">A function to configure dependency injection.</param>
    /// <param name="configureHealthChecks">An optional function to add custom health checks.</param>
    /// <param name="configureBuilder">An optional function to customise the host builder.</param>
    /// <returns>0 if app exits cleanly, or a non-zero value if there is an error.</returns>
    public static Task<int> RunAsync(string[] args, string appName, Action<IWebHostEnvironment, IConfiguration, IServiceCollection> configureServices, Action<IHealthChecksBuilder>? configureHealthChecks = null, Action<IHostBuilder>? configureBuilder = null) =>
        WebApplicationStart.RunAsync(
            args,
            appName,
            (builder) =>
            {
                configureServices(builder.Environment, builder.Configuration, builder.Services);
                configureBuilder?.Invoke(builder.Host);
            },
            (_) => ValueTask.CompletedTask,
            configureHealthChecks);
}
