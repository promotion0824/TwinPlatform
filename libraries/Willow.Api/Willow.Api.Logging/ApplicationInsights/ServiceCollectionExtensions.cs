namespace Willow.Api.Logging.ApplicationInsights
{
    using Microsoft.ApplicationInsights.AspNetCore;
    using Microsoft.ApplicationInsights.AspNetCore.Extensions;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// The service collection extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds application insights to the service collection.
        /// </summary>
        /// <param name="services">The existing service collection.</param>
        /// <param name="applicationinsightsConfigSection">The configuration section for the application insights connectivity.</param>
        /// <param name="environment">The IHostEnvironment instance for the application.</param>
        /// <param name="isWorker">A flag indicating whether the app is a worker instance or a web host.</param>
        /// <param name="enabledInDevelopment">Whether or not to enable App Insights when running in development mode.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddApplicationInsights(
            this IServiceCollection services,
            IConfigurationSection applicationinsightsConfigSection,
            IHostEnvironment environment,
            bool isWorker = false,
            bool enabledInDevelopment = false)
        {
            if (environment.IsDevelopment() && !enabledInDevelopment)
            {
                return services;
            }

            var options = new ApplicationInsightsOptions();
            applicationinsightsConfigSection.Bind(options);

            if (isWorker)
            {
                var appInsightOptions = new Microsoft.ApplicationInsights.WorkerService.ApplicationInsightsServiceOptions
                {
                    ConnectionString = options.ConnectionString,
                };
                services.AddApplicationInsightsTelemetryWorkerService(appInsightOptions);
            }
            else
            {
                var appInsightOptions = new ApplicationInsightsServiceOptions
                {
                    ConnectionString = options.ConnectionString,
                };
                services.AddApplicationInsightsTelemetry(appInsightOptions);
            }

            services.AddSingleton<ITelemetryInitializer, CloudRoleNameTelemetryInitializer>(_ =>
                new CloudRoleNameTelemetryInitializer(options.CloudRoleName));

            services.AddSingleton<ITelemetryInitializer, VersionTelemetryInitializer>();
            services.AddSingleton<ITelemetryInitializer, UseridTelemetryInitializer>();
            services.AddSingleton<ITelemetryInitializer, FrontdoorTelemetryInitializer>();

            services.AddSingleton<ITelemetryProcessorFactory>(_ => new CustomTelemetryProcessorFactory(options.Ignore));

            return services;
        }
    }
}
