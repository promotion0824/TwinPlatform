namespace Willow.Telemetry.Web
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extension methods for setting up WillowContext services in an <see cref="IServiceCollection"/> for a web application. />.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the values for the Willow context to the implementations for metrics and logging for the application.
        /// </summary>
        /// <param name="services">The application service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddWillowContext(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.ConfigureOpenTelemetry(configuration);
            return services;
        }
    }
}
