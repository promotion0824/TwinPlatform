using System.ComponentModel.DataAnnotations;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Controllers;

/// <summary>
/// Controller for interacting with User Data
/// </summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{

	private readonly ILogger<UsersController> _logger;
	private readonly IUserManager _userManager;
	private readonly IPermissionManager _permissionManager;

	public UsersController(ILogger<UsersController> logger, IUserManager userManager, IPermissionManager permissionManager)
	{
		_logger = logger;
		_userManager = userManager;
		_permissionManager = permissionManager;
	}


    /// <summary>
    /// Users Batch Request.
    /// </summary>
    /// <param name="batchRequest">Batch Request Instance.</param>
    /// <returns></returns>
	[Authorize(AppPermissions.CanReadUser)]
	[HttpPost("batch")]
	public Task<BatchDto<UserModel>> GetUsers([FromBody] BatchRequestDto batchRequest)
	{
		return _userManager.GetUsersAsync(batchRequest);
	}

    /// <summary>
    /// Get Users by group Id.
    /// </summary>
    /// <param name="groupId">Id of the group</param>
    /// <param name="batchRequest">Batch Request for Filtering, Sorting and Pagination.</param>
    /// <param name="getOnlyNonMembers">Only users who are not a member of the group Id will be returned.</param>
    /// <returns></returns>
    [Authorize(AppPermissions.CanReadUser)]
    [Authorize(AppPermissions.CanReadGroup)]
    [HttpPost("batch/group")]
    public Task<BatchDto<UserModel>> GetUsersByGroup([Required][FromQuery] string groupId,[FromQuery] bool getOnlyNonMembers, [FromBody] BatchRequestDto batchRequest)
    {
        var response = _userManager.GetUsersByGroupAsync(groupId, batchRequest, getOnlyNonMembers);
        return response; 
    }

    /// <summary>
    /// Method to get an user record by email and optionally user's inherited permission based on their assignments
    /// </summary>
    /// <param name="email">Email Id of the user</param>
    /// <param name="includePermissions">True to include permissions in the response;else false</param>
    /// <param name="includeADGroupPermissions">True to include AD Group granted permissions in the response;else false. This setting will be honored only if includePermission is set to true</param>
    /// <returns>User DTO and permissions</returns>
    [HttpGet("{email}")]
	public async Task<ActionResult> GetUserByEmail([FromRoute][Required] string email, [FromQuery] bool includePermissions = false, [FromQuery] bool includeADGroupPermissions = false)
	{
		if (!ModelState.IsValid)    
			return ValidationProblem(ModelState);

		var user = await _userManager.GetUserByEmailAsync(email);
		if (user == null)
			return NotFound();

		if (includePermissions)
		{
			var permissions = await _permissionManager.GetPermissionsByUserEmail(email);
			IEnumerable<ConditionalPermissionModel> adGroupBasedPermissions  = new List<ConditionalPermissionModel>();
			if (includeADGroupPermissions)
				adGroupBasedPermissions = await _permissionManager.GetPermissionBasedOnADGroupMembership(email);
			return Ok(new { user, permissions, adGroupBasedPermissions });
		}
		return Ok(new { user });
	}

	/// <summary>
	/// Method to get user's permission by email
	/// </summary>
	/// <param name="email">Email Id of the User</param>
	/// <returns>List of Conditional Permissions</returns>
	[HttpGet("{email}/permissions")]
	public async Task<ActionResult> GetUserPermissionByEmail([FromQuery][Required] string email)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);
		if (null == await _userManager.GetUserByEmailAsync(email))
			return NotFound();
		var permissions = await _permissionManager.GetPermissionsByUserEmail(email);
		return Ok(new { permissions });
	}

	/// <summary>
	/// Method to add a User record
	/// </summary>
	/// <param name="userModel">UserModel to add</param>
	/// <returns>OkResult with status 200</returns>
	[Authorize(AppPermissions.CanCreateUser)]
	[HttpPost]
	public async Task<ActionResult> AddUser([FromBody] UserModel userModel)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);
		await _userManager.AddUserAsync(userModel);
		return Ok();
	}

	/// <summary>
	/// Method to update user record
	/// </summary>
	/// <param name="userModel">UserModel to update</param>
	/// <returns>OkResult</returns>
	[Authorize(AppPermissions.CanEditUser)]
	[HttpPut]
	public async Task<ActionResult> UpdateUser([FromBody] UserModel userModel)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);
		await _userManager.UpdateUserAsync(userModel);
		return Ok();
	}

	/// <summary>
	/// Method to delete a user record
	/// </summary>
	/// <param name="Id">Id of the User Record</param>
	/// <returns>OkResult</returns>
	[Authorize(AppPermissions.CanDeleteUser)]
	[HttpDelete("{Id}")]
	public async Task<ActionResult> DeleteUser(string Id)
	{
		var userId = Guid.Parse(Id);
		var result = await _userManager.GetUserByIdAsync(userId);

		if (result == null)
		{
			return NotFound();
		}

		await _userManager.DeleteUserAsync(userId);
		return Ok();
	}

    /// <summary>
    /// Method to get all user record
    /// </summary>
    /// <param name="filterModel">Defines the filter fto limit the user records to retrieve</param>
    /// <returns>List of User DTO</returns>
    [Authorize(AppPermissions.CanReadUser)]
    [HttpGet("admins")]
    public async Task<IEnumerable<UserModel>> GetAdminUsers()
    {
        return await _userManager.GeAdminUsersAsync();
    }
}
