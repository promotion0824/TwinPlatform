namespace Willow.LiveData.Core.Infrastructure.HealthCheck;

using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

internal class AssemblyVersionHealthCheck : IHealthCheck
{
    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default(CancellationToken))
    {
        var assemblyVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
        var healthCheckResult = new HealthCheckResult(HealthStatus.Healthy, assemblyVersion);
        return Task.FromResult(healthCheckResult);
    }
}
