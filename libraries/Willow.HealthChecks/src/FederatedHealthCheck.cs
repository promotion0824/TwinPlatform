namespace Willow.HealthChecks;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;

/// <summary>
/// Federated Health Check collects health reports from dependencies.
/// </summary>
/// <remarks>
/// Rate limited to once every 30s, returns cached value in between.
/// </remarks>
public class HealthCheckFederated : IHealthCheck
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings();

    private static readonly DateTimeOffset started = DateTimeOffset.Now;

    /// <summary>
    /// Optionally set this to true during development of a service to prevent it calling out
    /// while you are trying to debug something else
    /// </summary>
    private readonly bool isDevelopment = false;

    /// <remarks>
    /// All this because someone overrode the default settings for Newtonsoft.
    /// </remarks>
    private readonly JsonSerializer jsonConverter = JsonSerializer.Create(JsonSerializerSettings);

    private readonly IHttpClientFactory? httpClientFactory;

    private HealthCheckResult cached;

    private DateTimeOffset lastFetched = DateTimeOffset.MinValue;

    private HealthCheckFederatedArgs? healthCheckFederatedArgs;

    /// <summary>
    /// The path like /healthz or /healthchecks
    /// </summary>
    private readonly string path = string.Empty;

    /// <summary>
    /// Creates a new <see cref="HealthCheckFederated" /> which brings in health from dependencies.
    /// </summary>
    public HealthCheckFederated(HealthCheckFederatedArgs args, IHttpClientFactory clientFactory)
    {
        if (args is null || string.IsNullOrWhiteSpace(args.Path) || string.IsNullOrWhiteSpace(args.BaseUri))
        {
            return;
        }

        path = args.Path;
        isDevelopment = args.IsDevelopment;
        httpClientFactory = clientFactory;
        healthCheckFederatedArgs = args;
    }

    /// <summary>
    /// Asynch health check.
    /// </summary>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (httpClientFactory is null)
        {
            return HealthCheckResult.Degraded("Missing httpClientFactory");
        }

        using var httpClient = GetHttpClient();

        try
        {
            if (httpClient is null)
            {
                return HealthCheckResult.Degraded("Not configured");
            }

            if (isDevelopment)
            {
                return HealthCheckResult.Healthy("Development mode, skipped calling out");
            }

            // Prevent thrashing / DDOS
            if (lastFetched.AddSeconds(30) > DateTimeOffset.Now)
            {
                return cached;
            }

            lastFetched = DateTimeOffset.Now;
            cached = await CheckHealthInternal(cancellationToken);
            return cached;
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded($"Could not contact subsystem {httpClient?.BaseAddress}{this.path.TrimStart('/')} - {ex.Message}", ex);
        }
    }

    private async Task<HealthCheckResult> CheckHealthInternal(CancellationToken cancellationToken)
    {
        if (httpClientFactory is null)
        {
            return HealthCheckResult.Degraded("Missing httpClientFactory");
        }

        using var httpClient = GetHttpClient();

        if (httpClient is null)
        {
            return HealthCheckResult.Degraded("Unable to create httpClient");
        }

        if (started.AddSeconds(30) > DateTimeOffset.Now)
        {
            return HealthCheckResult.Healthy("Starting up");
        }

        if (httpClient is null)
        {
            return HealthCheckResult.Degraded("Missing httpClient");
        }

        var dependency = await httpClient.GetAsync(this.path, cancellationToken);

        if (dependency.IsSuccessStatusCode)
        {
            var json = await dependency.Content.ReadAsStringAsync(cancellationToken);
            return Parse(jsonConverter, json);
        }
        else if (dependency.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        {
            try
            {
                var json = await dependency.Content.ReadAsStringAsync(cancellationToken);
                using JsonTextReader reader = new JsonTextReader(new StringReader(json));
                var subhealth = jsonConverter.Deserialize<HealthCheckDto>(reader);

                if (subhealth is null)
                {
                    return HealthCheckResult.Unhealthy("Unhealthy");
                }

                return HealthCheckResult.Unhealthy(subhealth.Description,
                                                   data: subhealth.EntriesWithPayload?.Where(x => x.Key != "Assembly Version")?.ToDictionary(x => x.Key, x => x.Value));
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"500 response {httpClient.BaseAddress}, can't read payload {ex.Message}");
            }
        }
        else
        {
            return HealthCheckResult.Degraded($"Could not contact subsystem {httpClient?.BaseAddress}{this.path.TrimStart('/')}, error {dependency.StatusCode}");
        }
    }

    /// <summary>
    /// Parse a JSON response using either new format or old format
    /// </summary>
    public static HealthCheckResult Parse(JsonSerializer jsonConverter, string json)
    {
        try
        {
            using JsonTextReader reader = new JsonTextReader(new StringReader(json));
            if (json.Contains("totalDuration"))
            {
                // Old format (/healthcheck)
                var subhealth = jsonConverter.Deserialize<HealthCheckOldStyle>(reader);
                if (subhealth is null)
                {
                    return HealthCheckResult.Healthy("Subsystem is healthy");
                }

                if (subhealth.Status == HealthStatus.Healthy)
                {
                    return HealthCheckResult.Healthy("Healthy", data: subhealth.EntriesWithPayload.ToDictionary(x => x.Key, x => (object)x.Value));
                }
                else
                {
                    return HealthCheckResult.Degraded("Degraded", data: subhealth?.EntriesWithPayload.ToDictionary(x => x.Key, x => (object)x.Value));
                }
            }
            else
            {
                // New format (/healthz)
                var subhealth = jsonConverter.Deserialize<HealthCheckDto>(reader);
                if (subhealth is null)
                {
                    return HealthCheckResult.Healthy("Subsystem is healthy");
                }

                if (subhealth.Status == HealthStatus.Healthy)
                {
                    return HealthCheckResult.Healthy(subhealth.Description,
                                                     data: subhealth?.EntriesWithPayload?.Where(x => x.Key != "Assembly Version").ToDictionary(x => x.Key, x => x.Value));
                }
                else
                {
                    return HealthCheckResult.Degraded(subhealth.Description,
                                                      data: subhealth?.EntriesWithPayload?.Where(x => x.Key != "Assembly Version").ToDictionary(x => x.Key, x => x.Value));
                }
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded("Could not parse payload " + ex.Message);
        }
    }

    private HttpClient? GetHttpClient()
    {
        if (healthCheckFederatedArgs is null || httpClientFactory is null)
        {
            return null;
        }

        var client = httpClientFactory.CreateClient("HealthCheck");
        client.BaseAddress = new Uri(healthCheckFederatedArgs.BaseUri);
        client.Timeout = TimeSpan.FromSeconds(2);
        return client;
    }
}
