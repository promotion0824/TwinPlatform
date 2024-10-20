namespace Willow.AdminApp;

using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Overall health status
/// </summary>
public class OverallState
{
    public HealthStatus Status { get; set; }

    private readonly ConcurrentDictionary<(string domain, string name), CustomerInstanceState> customerInstances = [];

    public IEnumerable<CustomerInstanceState> CustomerInstances => customerInstances.Values;

    private readonly ConcurrentDictionary<(string domain, string application), ApplicationInstance> applicationInstances = [];

    public IEnumerable<ApplicationInstance> ApplicationInstances => applicationInstances.Values;

    public void Report(CustomerInstanceState state)
    {
        if (state.CustomerInstance.Name is null) throw new ArgumentNullException(nameof(state), "has no name");
        if (state.CustomerInstance.Domain is null) throw new ArgumentNullException(nameof(state), "has no domain");
        customerInstances.AddOrUpdate((state.CustomerInstance.Domain, state.CustomerInstance.Name), state, (k, v) => state);
    }

    public void Report(ApplicationInstance applicationInstance)
    {
        applicationInstances[(applicationInstance.Domain, applicationInstance.ApplicationName)] = applicationInstance;
    }
}
