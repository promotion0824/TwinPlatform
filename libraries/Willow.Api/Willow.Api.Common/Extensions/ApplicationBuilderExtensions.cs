namespace Willow.Api.Common.Extensions;

using Microsoft.AspNetCore.Builder;
using Willow.Api.Common.Middlewares;

/// <summary>
/// A class containing extension methods for <see cref="IApplicationBuilder"/>.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the <see cref="RequestIdMiddleware"/> to the pipeline.
    /// </summary>
    /// <param name="app">An IApplicationBuilderInstance.</param>
    /// <returns>The updated IApplicationBuilderInstance.</returns>
    public static IApplicationBuilder UseRequestIdMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestIdMiddleware>();
        return app;
    }
}
