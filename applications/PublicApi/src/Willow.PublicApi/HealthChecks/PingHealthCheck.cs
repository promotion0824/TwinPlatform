namespace Willow.PublicApi.HealthChecks;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

internal class PingHealthCheck(PingHealthCheckArgs args, IHttpClientFactory httpClientFactory) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage message = new(HttpMethod.Head, args.Uri);
        var response = await httpClientFactory.CreateClient().SendAsync(message, cancellationToken);

        if (response == null || !response.IsSuccessStatusCode)
        {
            return HealthCheckResult.Unhealthy();
        }

        return HealthCheckResult.Healthy();
    }
}

internal record PingHealthCheckArgs(string Uri);
