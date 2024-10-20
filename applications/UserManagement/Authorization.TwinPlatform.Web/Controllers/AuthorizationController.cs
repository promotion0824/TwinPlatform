using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Authorization.TwinPlatform.Web.Controllers;

/// <summary>
/// Controller class for User Management App Authorization needs
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthorizationController : ControllerBase
{
	private readonly ILogger<AuthorizationController> _logger;
	private readonly IUserAuthorizationManager _userAuthorizationManager;

	public AuthorizationController(ILogger<AuthorizationController> logger, IUserAuthorizationManager userAuthorizationManager)
	{
		_logger = logger;
		_userAuthorizationManager = userAuthorizationManager;
	}

	/// <summary>
	/// Get current user's authorization data (contains permission list)
	/// </summary>
	/// <returns>Instance of AuthorizationResponseDto</returns>
	[Authorize]
	[HttpGet]
	public async Task<ActionResult<AuthorizationResponseDto>> GetMyAuthorizationPermissions()
		{
		//Get the current user's email from the claims
		var currentUserEmailClaim = User.FindFirst(claim => claim.Type == "emails" || claim.Type == ClaimTypes.Email);

		if (currentUserEmailClaim is null)
		{
			return Unauthorized();
		}

		_logger.LogDebug("Retrieved request for getting user authorization permissions");

		var response = await _userAuthorizationManager.GetAuthorizationPermissions(currentUserEmailClaim.Value);

		_logger.LogDebug("Responding to user authorization permissions");

		return response;
	}
}
