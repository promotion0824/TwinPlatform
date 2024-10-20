using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.CognitiveSearch;
using Willow.HealthChecks;

namespace Willow.AzureDigitalTwins.Api.Helpers;

public static class HealthStatusChangeHelper
{
    public static void ChangeHealthStatus(HealthCheckBase<HealthService> healthCheck, HealthCheckResult newStatus, ILogger logger)
    {
        // Compare by description because the status number of both starting and healthy are the same
        if (!healthCheck.Current.Description.Equals(newStatus.Description))
        {
            healthCheck.Current = newStatus;
            logger.LogInformation($"Set {healthCheck.GetType().Name} Health to: {healthCheck.Current.Description}");
        }
    }

    // HealthCheckSearch does not inherit from HealthCheckBase<HealthService> so we need to overload the method
    public static void ChangeHealthStatus(HealthCheckSearch healthCheck, HealthCheckResult newStatus, ILogger logger)
    {
        // Compare by description because the status number of both starting and healthy are the same
        if (!healthCheck.Current.Description.Equals(newStatus.Description))
        {
            healthCheck.Current = newStatus;
            logger.LogInformation($"Set {healthCheck.GetType().Name} Health to: {healthCheck.Current.Description}");
        }
    }
}
