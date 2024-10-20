namespace Willow.HealthChecks;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Healthcheck publisher records health checks locally so that UI can display status too.
/// </summary>
/// <remarks>
/// Register this as a singleton and use it in any class that needs to examine overall system health
/// without calling the health endpoint.
/// </remarks>
public class HealthCheckPublisher : IHealthCheckPublisher
{
    private readonly string systemName;

    private volatile HealthCheckDto slimResult;

    /// <summary>
    /// Creates a new <see cref="HealthCheckPublisher" />.
    /// </summary>
    public HealthCheckPublisher(string systemName)
    {
        this.systemName = systemName ?? throw new System.ArgumentNullException(nameof(systemName));
        this.slimResult = new HealthCheckDto(this.systemName, this.systemName, new HealthReport(new Dictionary<string, HealthReportEntry>(), TimeSpan.FromSeconds(1)));
    }

    /// <summary>
    /// Gets the health tree.
    /// </summary>
    public HealthCheckDto Result => this.slimResult;

    /// <summary>
    /// Publishes the health report locally.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        this.slimResult = new HealthCheckDto(this.systemName, this.systemName, report);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Walk the tree to find all health checks including descendant and remote.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<HealthCheckDto> GetAll()
    {
        return this.slimResult?.SelfAndDescendants("root") ?? Array.Empty<HealthCheckDto>();
    }
}
