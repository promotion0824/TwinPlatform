using System.Security.Claims;
using Authorization.TwinPlatform.Common.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.TwinLifecycleManagement.Web.Models;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Controller for used for fetching Authorization data by front end.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthorizationController : ControllerBase
{
	private readonly ILogger<AuthorizationController> _logger;
	private readonly IUserAuthorizationService _userAuthorizationService;

	/// <summary>
	/// Initializes a new instance of the <see cref="AuthorizationController"/> class.
	/// </summary>
	/// <param name="logger">Logger.</param>
	/// <param name="userAuthorizationService">User Authorization Service. </param>
	public AuthorizationController(ILogger<AuthorizationController> logger, IUserAuthorizationService userAuthorizationService)
	{
		_logger = logger;
		_userAuthorizationService = userAuthorizationService;
	}

	/// <summary>
	/// Get current user's authorization data (contains permission list).
	/// </summary>
	/// <returns>Instance of AuthorizationResponse. </returns>
	[Authorize]
	[HttpGet]
	public async Task<ActionResult<AuthorizationResponseDto>> GetMyAuthorizationPermissions()
	{
		// Get the current user's email from the claims
		var currentUserEmailClaim = User.FindFirst(claim => claim.Type == "emails" || claim.Type == ClaimTypes.Email);

		if (currentUserEmailClaim is null)
		{
			return Unauthorized();
		}

		_logger.LogDebug("Retrieved request for getting user authorization permissions");

		var response = await _userAuthorizationService.GetAuthorizationResponse(currentUserEmailClaim.Value);

		_logger.LogDebug("Responding to user authorization permissions");

		return new AuthorizationResponseDto()
		{
			Permissions = response.Permissions.Select(x => x.Name),
			IsAdminUser = false,
		};
	}
}
