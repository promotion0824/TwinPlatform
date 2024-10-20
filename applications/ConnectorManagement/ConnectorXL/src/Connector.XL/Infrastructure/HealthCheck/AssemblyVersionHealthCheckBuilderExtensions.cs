namespace Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Willow.Infrastructure.HealthCheck;

internal static class AssemblyVersionHealthCheckBuilderExtensions
{
    public static IHealthChecksBuilder AddAssemblyVersion(this IHealthChecksBuilder builder)
    {
        return builder.Add(new HealthCheckRegistration("Assembly Version", sp => new AssemblyVersionHealthCheck(), HealthStatus.Healthy, new string[0]));
    }
}
