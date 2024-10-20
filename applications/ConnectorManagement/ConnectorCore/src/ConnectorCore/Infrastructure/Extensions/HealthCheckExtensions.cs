namespace ConnectorCore.Infrastructure.Extensions;

using System;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

internal static class HealthCheckExtensions
{
    public static Task WriteResponse(HttpContext context, HealthReport report)
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();

        var json = JsonSerializer.Serialize(
                                            new
                                            {
                                                Status = report.Status.ToString(),
                                                Duration = report.TotalDuration,
                                                AssemblyVersion = assemblyVersion,
                                                Info = report.Entries
                                                             .Select(e =>
                                                                         new
                                                                         {
                                                                             e.Key,
                                                                             e.Value.Description,
                                                                             e.Value.Duration,
                                                                             Status = Enum.GetName(
                                                                                                   typeof(HealthStatus),
                                                                                                   e.Value.Status),
                                                                             Error = e.Value.Exception?.Message,
                                                                             e.Value.Data,
                                                                         })
                                                             .ToList(),
                                            },
                                            jsonSerializerOptions);

        context.Response.ContentType = MediaTypeNames.Application.Json;
        return context.Response.WriteAsync(json);
    }
}
