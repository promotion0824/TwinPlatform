using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Graph;
using Willow.HealthChecks;

namespace Authorization.TwinPlatform.HealthChecks;
public class HealthCheckAD : HealthCheckBase<GraphServiceClient>
{
    /// <summary>
    /// Status indicates connection to Azure AD B2C is not configured.
    /// </summary>
    public static readonly HealthCheckResult NotConfigured = HealthCheckResult.Degraded("Not configured");

    /// <summary>
    /// Status indicates graph api calls to Azure AD B2C failing.
    /// </summary>
    public static readonly HealthCheckResult FailingCalls = HealthCheckResult.Degraded("Failing calls");
}

public class HealthCheckAzureAD: HealthCheckAD { }

public class HealthCheckAzureB2C: HealthCheckAD { }
