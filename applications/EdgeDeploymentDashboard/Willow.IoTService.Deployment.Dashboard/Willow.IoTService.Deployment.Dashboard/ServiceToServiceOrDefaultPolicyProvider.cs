namespace Willow.IoTService.Deployment.Dashboard;

using Authorization.TwinPlatform.Common.Authorization.Providers;
using Authorization.TwinPlatform.Common.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

public class ServiceToServiceOrDefaultPolicyProvider(
    IOptions<AuthorizationOptions> options,
    IOptions<AuthorizationAPIOption> apiOptions,
    IHttpContextAccessor httpContextAccessor)
    : IAuthorizationPolicyProvider
{
    private AuthorizePermissionPolicyProvider BackupPolicyProvider { get; } = new(options, apiOptions);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(ServiceToServiceOrDefaultAuthorizeAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return BackupPolicyProvider.GetPolicyAsync(policyName)!;
        }

        var policy = policyName[ServiceToServiceOrDefaultAuthorizeAttribute.PolicyPrefix.Length..];
        var defaultPolicy = new AuthorizationPolicyBuilder(Constants.AzureAd).RequireAuthenticatedUser()
            .Build();

        if (string.IsNullOrWhiteSpace(policy))
        {
            return Task.FromResult(defaultPolicy)!;
        }

        if (httpContextAccessor.HttpContext != null &&
            httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization-Scheme", out var headerValue) &&
            headerValue[0] == Constants.AzureAd)
        {
            return Task.FromResult(defaultPolicy)!;
        }

        return BackupPolicyProvider.GetPolicyAsync(policy)!;
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => BackupPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => BackupPolicyProvider.GetFallbackPolicyAsync();
}
