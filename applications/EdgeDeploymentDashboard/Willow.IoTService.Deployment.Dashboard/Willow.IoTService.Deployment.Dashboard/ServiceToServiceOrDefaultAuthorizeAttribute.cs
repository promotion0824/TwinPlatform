namespace Willow.IoTService.Deployment.Dashboard;

using Microsoft.AspNetCore.Authorization;

internal class ServiceToServiceOrDefaultAuthorizeAttribute(string? actionName = null)
    : AuthorizeAttribute(string.Concat(PolicyPrefix, actionName))
{
    public const string PolicyPrefix = "ServiceToServiceOrDefault";
}
