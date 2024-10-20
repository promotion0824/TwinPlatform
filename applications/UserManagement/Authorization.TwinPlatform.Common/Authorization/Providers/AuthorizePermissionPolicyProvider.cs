
using Authorization.TwinPlatform.Common.Authorization.Requirements;
using Authorization.TwinPlatform.Common.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Authorization.TwinPlatform.Common.Authorization.Providers;

/// <summary>
/// Authorization Policy Provider class for permission based policy evaluation
/// </summary>
public class AuthorizePermissionPolicyProvider : IAuthorizationPolicyProvider
{
	private readonly DefaultAuthorizationPolicyProvider _defaultPolicyProvider;
	private readonly AuthorizationAPIOption _apiOptions;

	public AuthorizePermissionPolicyProvider(IOptions<AuthorizationOptions> options, IOptions<AuthorizationAPIOption> apiOption)
	{
		ArgumentNullException.ThrowIfNull(options);
		ArgumentNullException.ThrowIfNull(apiOption);
		_apiOptions = apiOption.Value;
		_defaultPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
	}

	public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
	{
		if(string.IsNullOrEmpty(policyName))
			return _defaultPolicyProvider.GetPolicyAsync(policyName);

		var policyBuilder = _apiOptions.AuthenticationSchemes is null ?
			new AuthorizationPolicyBuilder() :
			new AuthorizationPolicyBuilder(_apiOptions.AuthenticationSchemes);

		var permissionPolicy = policyBuilder.AddRequirements(new AuthorizePermissionRequirement(policyName));
		return Task.FromResult<AuthorizationPolicy?>(permissionPolicy.Build()) ??
			_defaultPolicyProvider.GetPolicyAsync(policyName);
	}


	public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _defaultPolicyProvider.GetDefaultPolicyAsync();

	public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _defaultPolicyProvider.GetFallbackPolicyAsync();
}
