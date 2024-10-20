namespace Willow.Infrastructure.HealthCheck;

using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
///     This isn't a health check, it only reports the current assembly version, which is useful information to have on the health check output.
/// </summary>
internal class AssemblyVersionHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
    {
        var assemblyVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        var healthCheckResult = new HealthCheckResult(HealthStatus.Healthy, assemblyVersion);
        return Task.FromResult(healthCheckResult);
    }
}
