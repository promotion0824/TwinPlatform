namespace Willow.HealthChecks
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Newtonsoft.Json;

    /// <summary>
    /// A health check response.
    /// </summary>
    public class HealthCheckResponse
    {
        /// <summary>
        /// Gets the default JSON settings.
        /// </summary>
        public static JsonSerializerSettings JsonSettings { get; } = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        /// <summary>
        /// Gets or sets the health check name.
        /// </summary>
        public string HealthCheckName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the health check description.
        /// </summary>
        public string HealthCheckDescription { get; set; } = string.Empty;

        /// <summary>
        /// Write response to /healthz endpoint.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <param name="healthReport">The health report.</param>
        /// <returns>An awaitable task.</returns>
        public virtual Task WriteHealthZResponse(HttpContext context, HealthReport healthReport)
        {
            context.Response.ContentType = "application/json; charset=utf-8";
            var slimResult = new HealthCheckDto(HealthCheckName, HealthCheckDescription, healthReport);

            string json = JsonConvert.SerializeObject(slimResult, JsonSettings);
            return context.Response.WriteAsync(json);
        }

        /// <summary>
        /// Write response to /livez endpoint.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <param name="healthReport">The health report.</param>
        /// <returns>An awaitable task.</returns>
        public virtual Task WriteLiveZResponse(HttpContext context, HealthReport healthReport)
        {
            return context.Response.WriteAsync("Live");
        }

        /// <summary>
        /// Write response to /readyz endpoint.
        /// </summary>
        /// <param name="context">The http context.</param>
        /// <param name="healthReport">The health report.</param>
        /// <returns>An awaitable task.</returns>
        public virtual Task WriteReadyZResponse(HttpContext context, HealthReport healthReport)
        {
            return context.Response.WriteAsync("Ready");
        }
    }
}
