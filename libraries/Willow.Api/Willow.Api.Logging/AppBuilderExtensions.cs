namespace Willow.Api.Logging;

using Microsoft.AspNetCore.Builder;
using Willow.Api.Logging.Correlation;

/// <summary>
/// Extension methods for the <see cref="IApplicationBuilder"/> interface.
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Adds the correlation id to the response.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated application builder.</returns>
    public static IApplicationBuilder UseResponseCorrelationMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<AddCorrelationIdToResponseMiddleware>();
        return app;
    }
}
