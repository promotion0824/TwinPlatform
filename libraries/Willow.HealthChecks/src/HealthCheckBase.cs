namespace Willow.HealthChecks;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// Base health check class, when T is a class or interface, uses the assembly version of that class as the version.
/// </summary>
/// <remarks>
/// Typically T would be the class of the service you are monitoring, e.g. PublicApiService, ...
/// Also you'd typically add a NotConfigured result for that special case.
/// Register your class with DI and inject it everywhere you need to update the status using the .Current property.
/// </remarks>
public abstract class HealthCheckBase<T> : IHealthCheck
{
    /// <summary>
    /// Healthy but haven't tested it yet.
    /// </summary>
    public static readonly HealthCheckResult Starting = HealthCheckResult.Healthy("Starting", extraData);

    /// <summary>
    /// Healthy, configured and working.
    /// </summary>
    public static readonly HealthCheckResult Healthy = HealthCheckResult.Healthy("Healthy", extraData);

    /// <summary>
    /// Assembly version of the code that this health check covers
    /// and any other extra data in a dictionary.
    /// </summary>
    protected static readonly Dictionary<string, object> extraData =
        typeof(T).IsClass || typeof(T).IsInterface ?
        new Dictionary<string, object> { ["Version"] = typeof(T).Assembly.GetName().Version!.ToString() } :
        new Dictionary<string, object>();

    /// <summary>
    /// Creates a new <see cref="HealthCheckBase{T}"/>.
    /// </summary>
    protected HealthCheckBase()
    {
        this.Current = Starting;
    }

    /// <summary>
    /// Gets or sets get or set the current status.
    /// </summary>
    public HealthCheckResult Current { get; set; }

    /// <summary>
    /// Asynch health check.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(this.Current);
    }
}
