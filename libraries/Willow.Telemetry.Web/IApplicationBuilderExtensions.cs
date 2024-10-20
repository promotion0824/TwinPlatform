namespace Willow.Telemetry.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Willow.AppContext;

    /// <summary>
    /// Extension methods for setting up WillowContext services in an <see cref="IApplicationBuilder"/> for a web application. />.
    /// </summary>
    public static class IApplicationBuilderExtensions
    {
        /// <summary>
        /// Add the values for the Willow context to the implementations for metrics for the application incoming http requests.
        /// </summary>
        /// <param name="app">The existing WebApplication.</param>
        /// <param name="configuration">An IConfiguration Instance.</param>
        /// <returns>The updated WebApplication.</returns>
        public static IApplicationBuilder UseWillowContext(this IApplicationBuilder app, IConfiguration configuration)
        {
#if NET8_0_OR_GREATER
            var willowContext = configuration.GetSection("WillowContext").Get<WillowContextOptions>();

            if (willowContext != null)
            {
                app.Use(async (context, next) =>
                {
                    var tagsFeature = context.Features.Get<IHttpMetricsTagsFeature>();

                    if (tagsFeature != null)
                    {
                        foreach (var val in willowContext.Values)
                        {
                            tagsFeature.Tags.Add(val);
                        }
                    }

                    await next.Invoke();
                });
            }
#endif
            return app;
        }
    }
}
