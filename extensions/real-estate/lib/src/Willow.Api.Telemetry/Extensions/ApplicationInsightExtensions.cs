using Microsoft.AspNetCore.Builder;
using Willow.Api.Telemetry.Middleware;

namespace Willow.Api.Telemetry.Extensions
{
    public static class ApplicationInsightExtensions
    {
        public static IApplicationBuilder UseRequestBodyLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestBodyLoggingMiddleware>();
        }

        public static IApplicationBuilder UseResponseBodyLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResponseBodyLoggingMiddleware>();
        }
    }
}
