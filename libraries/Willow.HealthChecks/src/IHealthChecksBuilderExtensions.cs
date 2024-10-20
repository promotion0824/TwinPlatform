using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.HealthChecks.Checks;

namespace Willow.HealthChecks;

/// <summary>
/// Extensions for the <see cref="IHealthChecksBuilder"/> interface to add health checks for known services.
/// </summary>
public static class IHealthChecksBuilderExtensions
{
    /// <summary>
    /// Adds the health check for the livez endpoint.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> that this method extends.</param>
    /// <returns>The builder instance so that calls can be changed.</returns>
    public static IHealthChecksBuilder AddLivez(this IHealthChecksBuilder builder) =>
        builder.AddCheck("livez", () => HealthCheckResult.Healthy("System is live."), tags: new[] { "livez" });

    /// <summary>
    /// Adds the health check for the readyz endpoint.
    /// </summary>
    /// <remarks>
    /// Pass the <paramref name="appStopping"/> source to <see cref="IApplicationBuilderExtensions.UseWillowHealthChecks(Microsoft.AspNetCore.Builder.WebApplication, string, string, CancellationTokenSource)"/>
    /// in order to connect these health checks to the application stopping event.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> that this method extends.</param>
    /// <param name="appStopping">A cancellation token that cancelled when the app is stopping or stopped.</param>
    /// <returns>The builder instance so that calls can be changed.</returns>
    [Obsolete("Use AddReadyz")]
    public static IHealthChecksBuilder Readyz(this IHealthChecksBuilder builder, CancellationTokenSource appStopping) =>
            builder.AddCheck("readyz",
                   () =>
                   {
                       appStopping.Token.ThrowIfCancellationRequested();
                       return HealthCheckResult.Healthy("System is ready.");
                   },
                   tags: new[] { "readyz" });

    /// <summary>
    /// Adds the health check for the readyz endpoint.
    /// </summary>
    /// <remarks>
    /// Pass the <paramref name="appStopping"/> source to <see cref="IApplicationBuilderExtensions.UseWillowHealthChecks(Microsoft.AspNetCore.Builder.WebApplication, string, string, CancellationTokenSource)"/>
    /// in order to connect these health checks to the application stopping event.
    /// </remarks>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> that this method extends.</param>
    /// <param name="appStopping">A cancellation token that cancelled when the app is stopping or stopped.</param>
    /// <returns>The builder instance so that calls can be changed.</returns>
    public static IHealthChecksBuilder AddReadyz(this IHealthChecksBuilder builder, CancellationTokenSource appStopping) =>
            builder.AddCheck("readyz",
                   () =>
                   {
                       appStopping.Token.ThrowIfCancellationRequested();
                       return HealthCheckResult.Healthy("System is ready.");
                   },
                   tags: new[] { "readyz" });

    /// <summary>
    /// Adds a new health check with the specified name and implementation as a singleton.
    /// </summary>
    /// <typeparam name="T">The health check implementation type.</typeparam>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> object that this method extends.</param>
    /// <param name="name">The name of the health check.</param>
    /// <param name="failureStatus">
    /// The Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus that should be
    /// reported when the health check reports a failure. If the provided value is null,
    /// then Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy will
    /// be reported.
    /// </param>
    /// <param name="tags">A list of tags that can be used to filter health checks.</param>
    /// <param name="timeout">An optional System.TimeSpan representing the timeout of the check.</param>
    /// <returns>The <see cref="Microsoft.Extensions.DependencyInjection.IHealthChecksBuilder"/>.</returns>
    public static IHealthChecksBuilder AddSingletonCheck<T>(this IHealthChecksBuilder builder, string name, HealthStatus? failureStatus = null, IEnumerable<string>? tags = null, TimeSpan? timeout = null)
        where T : class, IHealthCheck
    {
        builder.Services.AddSingleton<T>();
        builder.AddCheck<T>(name, failureStatus, tags, timeout);

        return builder;
    }

    /// <summary>
    /// Adds a health check for the Authorization service.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> that this method extends.</param>
    /// <returns>The builder instance so that calls can be changed.</returns>
    public static IHealthChecksBuilder AddAuthz(this IHealthChecksBuilder builder) =>
        builder.AddCheck<HealthCheckFederatedAuthz>("Authorization Service", tags: new[] { "healthz" });

    /// <summary>
    /// Adds a health check for Public API.
    /// </summary>
    /// <param name="builder">The <see cref="IHealthChecksBuilder"/> that this method extends.</param>
    /// <returns>The builder instance so that calls can be changed.</returns>
    public static IHealthChecksBuilder AddPublicApi(this IHealthChecksBuilder builder) =>
        builder.AddCheck<HealthCheckFederatedPublicApi>("Public API", tags: new[] { "healthz" });
}
