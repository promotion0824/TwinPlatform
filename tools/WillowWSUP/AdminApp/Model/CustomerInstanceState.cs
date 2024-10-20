namespace Willow.AdminApp;

using Microsoft.Extensions.Diagnostics.HealthChecks;

public record CustomerInstanceState(CustomerInstance CustomerInstance, IEnumerable<HealthStatus> statuses)
{
    public HealthStatus Status => !statuses.Any() ? HealthStatus.Healthy : (HealthStatus)statuses.Min(x => (int)x);
};
