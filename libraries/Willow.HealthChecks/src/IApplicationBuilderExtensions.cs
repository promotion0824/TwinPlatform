namespace Willow.HealthChecks
{
    using System.Threading;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;

    /// <summary>
    /// Extension methods for setting up HealthChecks in an <see cref="IApplicationBuilder"/> for an application. />.
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Add various default health checks to the application.
        /// </summary>
        /// <param name="app">The existing WebApplication.</param>
        /// <param name="healthCheckResponse">The response object for health checks.</param>
        /// <returns>The updated WebApplication.</returns>
        public static WebApplication UseWillowHealthChecks(this WebApplication app, HealthCheckResponse healthCheckResponse)
        {
            UseWillowHealthChecks(app as IApplicationBuilder, healthCheckResponse);
            return app;
        }

        /// <summary>
        /// Add various default health checks to the application.
        /// </summary>
        /// <param name="app">IApplicationBuilder instance.</param>
        /// <param name="healthCheckResponse">The response object for health checks.</param>
        /// <returns>The updated IApplicationBuilder for chaining.</returns>
        public static IApplicationBuilder UseWillowHealthChecks(this IApplicationBuilder app, HealthCheckResponse healthCheckResponse)
        {
            app.UseHealthChecks("/healthz", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("healthz"),
                AllowCachingResponses = false,
                ResponseWriter = healthCheckResponse.WriteHealthZResponse,
            });

            app.UseHealthChecks("/livez", new HealthCheckOptions()
            {
                Predicate = (check) => check.Name == "livez",
                ResponseWriter = healthCheckResponse.WriteLiveZResponse,
            });

            app.UseHealthChecks("/readyz", new HealthCheckOptions()
            {
                Predicate = (check) => check.Name == "readyz",
                ResponseWriter = healthCheckResponse.WriteReadyZResponse,
            });

            return app;
        }

        /// <summary>
        /// A comprehensive extension to add health checks to the app.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication"/> instance that this method extends.</param>
        /// <param name="name">The name of the app as it appears in the health check.</param>
        /// <param name="description">The description of the app as it appears in the health check.</param>
        /// <param name="appStopping">A cancellation token source that will cancel when the app is stopped.</param>
        /// <returns>The updated <see cref="WebApplication"/> for chaining.</returns>
        public static WebApplication UseWillowHealthChecks(this WebApplication app, string name, string description, CancellationTokenSource appStopping)
        {
            HealthCheckResponse healthCheckResponse = new()
            {
                HealthCheckName = name,
                HealthCheckDescription = description,
            };

            app.Lifetime.ApplicationStopping.Register(appStopping.Cancel);
            app.Lifetime.ApplicationStopped.Register(appStopping.Cancel);
            return app.UseWillowHealthChecks(healthCheckResponse);
        }
    }
}
