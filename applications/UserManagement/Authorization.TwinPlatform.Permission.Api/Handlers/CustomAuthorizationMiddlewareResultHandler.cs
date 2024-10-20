using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;

namespace Authorization.TwinPlatform.Permission.Api.Handlers;

/// <summary>
/// Class implementation to intercept authorization requests
/// </summary>
public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
	private readonly AuthorizationMiddlewareResultHandler defaultHandler = new();
	private readonly ILogger<CustomAuthorizationMiddlewareResultHandler> _logger;

	public CustomAuthorizationMiddlewareResultHandler(ILogger<CustomAuthorizationMiddlewareResultHandler> logger)
	{
		_logger= logger;
	}

	public async Task HandleAsync(
		RequestDelegate next,
		HttpContext context,
		AuthorizationPolicy policy,
		PolicyAuthorizationResult authorizeResult)
	{
		// If the authorization did not succeed
		//return a custom response message

		if (!authorizeResult.Succeeded)
		{
            _logger.LogError("Authorization failed - Is default Identity Authenticated : {IsAuthenticated} & Failure Reason: {FailureReason}",
                             context.User?.Identity?.IsAuthenticated,
 	                         string.Join(',', authorizeResult.AuthorizationFailure?.FailureReasons.Select(s=>s.Message) ?? new string[] { }));

            //Set the custom status code and response message
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			await context.Response.WriteAsJsonAsync(new {ErrorMessage = "You are not authorized to access this resource" });
			return;
		}

		// Fall back to the default implementation.
		await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
	}
}

