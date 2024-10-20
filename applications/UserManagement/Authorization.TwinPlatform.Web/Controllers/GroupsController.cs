using Authorization.Common.Models;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class GroupsController : ControllerBase
{

	private readonly ILogger<GroupsController> _logger;
	private readonly IGroupManager _groupManager;

	public GroupsController(ILogger<GroupsController> logger, IGroupManager groupManager)
	{
		_logger = logger;
		_groupManager = groupManager;
	}

	[Authorize(AppPermissions.CanReadGroup)]
	[HttpPost("batch")]
	public async Task<BatchDto<GroupModel>> GetGroups([FromBody] BatchRequestDto batchRequest)
	{
		var response =  await _groupManager.GetGroupsAsync(batchRequest);
        return response;
	}

    [Authorize(AppPermissions.CanReadGroup)]
    [HttpGet]
    public async Task<ActionResult<GroupModel>> GetGroupByName([FromQuery]string name)
    {
        var response = await _groupManager.GetGroupByNameAsync(name);

        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }

        return response.Result;
    }

    [Authorize(AppPermissions.CanCreateGroup)]
	[HttpPost]
	public async Task<ActionResult<GroupModel>> CreateGroup([FromBody] GroupModel model)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);

		var response = await _groupManager.AddGroupAsync(model);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        // Invalid scenario, code will never hit this path
        // still just a safe check
        else if (response.Result is null) 
        {
            return NotFound();
        }

        return response.Result;
	}

	[Authorize(AppPermissions.CanEditGroup)]
	[HttpPut]
	public async Task<ActionResult<GroupModel>> UpdateGroup([FromBody] GroupModel model)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);

		var response = await _groupManager.UpdateGroupAsync(model);

        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }

        return response.Result;
	}

	[Authorize(AppPermissions.CanDeleteGroup)]
	[HttpDelete("{Id}")]
	public async Task<ActionResult> DeleteGroup(string Id)
	{
		var groupId = Guid.Parse(Id);
		var response = await _groupManager.DeleteGroupAsync(groupId);
        if (response.FailedAuthorization)
        {
            return Unauthorized(); 
        }
        else if (response.Result is null)
        {
            return NotFound();
        }

        return NoContent();
	}
	[Authorize(AppPermissions.CanAssignUser)]
	[HttpPost("{Id}/users")]
	public async Task<ActionResult> AssignUserToGroup([FromRoute] string Id,[FromBody] UserModel userModel)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);
		if (!Guid.TryParse(Id, out var groupId) || !ModelState.IsValid)
		{
			return BadRequest();
		}
		await _groupManager.AssignUserAsync(groupId, userModel);
		return Ok();
	}

	[Authorize(AppPermissions.CanRemoveUser)]
	[HttpDelete("{Id}/users/{uId}")]
	public async Task<ActionResult> RemoveUserFromGroup([FromRoute] string Id, [FromRoute] string uId)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);
		if (!Guid.TryParse(Id, out var groupId) || !Guid.TryParse(uId,out var userId))
		{
			return BadRequest();
		}
		await _groupManager.RemoveUserAsync(groupId, userId);
		return Ok();
	}
}

