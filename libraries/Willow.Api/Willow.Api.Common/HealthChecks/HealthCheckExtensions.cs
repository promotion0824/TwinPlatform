namespace Willow.Api.Common.HealthChecks;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;

/// <summary>
/// Defines the Health Check Extensions.
/// </summary>
public static class HealthCheckExtensions
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Adds the Health Check Endpoint to the Application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static void UseHealthCheckEndpoint(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health",
            new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";

                    var healthCheckResponse = new HealthCheckResponse
                    {
                        Status = report.Status.ToString(),
                        HealthChecks =
                            report.Entries.Select(entry =>
                                new HealthCheck(entry.Key, entry.Value.Status.ToString(), entry.Value.Description)),
                        Duration = report.TotalDuration,
                    };

                    var json = JsonSerializer.Serialize(healthCheckResponse, JsonSerializerOptions);

                    await context.Response.WriteAsync(json);
                },
            });
    }
}
