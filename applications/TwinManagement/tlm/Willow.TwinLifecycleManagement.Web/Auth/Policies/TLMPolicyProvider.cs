using Authorization.TwinPlatform.Common.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Willow.TwinLifecycleManagement.Web.Auth.Policies;

public class TLMPolicyProvider : IAuthorizationPolicyProvider
{
	private readonly DefaultAuthorizationPolicyProvider _defaultPolicyProvider;

	public TLMPolicyProvider(IOptions<AuthorizationOptions> options)
	{
		ArgumentNullException.ThrowIfNull(options);
		_defaultPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
	}

	public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
	{
		if (string.IsNullOrEmpty(policyName))
		{
			return _defaultPolicyProvider.GetPolicyAsync(policyName);
		}

		var policyBuilder = new AuthorizationPolicyBuilder().AddRequirements(new AuthorizePermissionRequirement(policyName));
		return Task.FromResult<AuthorizationPolicy>(policyBuilder.Build()) ??
			_defaultPolicyProvider.GetPolicyAsync(policyName);
	}

	public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _defaultPolicyProvider.GetDefaultPolicyAsync();

	public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _defaultPolicyProvider.GetFallbackPolicyAsync();
}
