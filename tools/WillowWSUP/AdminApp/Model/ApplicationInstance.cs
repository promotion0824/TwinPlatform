namespace Willow.AdminApp;

using Willow.HealthChecks;
using Willow.Infrastructure.Entities;

public record ApplicationInstance(bool IsPrimary, string Region, string devOrPrd, string customerInstanceCode, string ApplicationName,
    DeploymentPhase deploymentPhase,
    string Domain,
    string Url,
    string HealthUrl,
    string CloudRoleName,
    string ApplicationInsightsLink,
    string ApplicationInsightsExceptionsLink,
    HealthCheckDto Health,
    DateTimeOffset last);
