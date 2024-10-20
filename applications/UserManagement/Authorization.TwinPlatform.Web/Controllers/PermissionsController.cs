using Authorization.Common.Models;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Controllers;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class PermissionsController : ControllerBase
{
	private readonly ILogger<PermissionsController> _logger;
	private readonly IPermissionManager _permissionManager;

	public PermissionsController(ILogger<PermissionsController> logger, IPermissionManager permissionManager)
	{
		_logger = logger;
		_permissionManager = permissionManager;
	}

	[Authorize(AppPermissions.CanReadPermission)]
	[HttpPost("batch")]
	public async Task<BatchDto<PermissionModel>> GetPermissions([FromBody]BatchRequestDto batchRequest)
	{
		return await _permissionManager.GetPermissionsAsync(batchRequest);
	}

    /// <summary>
    /// Get Permissions by role Id.
    /// </summary>
    /// <param name="roleId">Id of the role.</param>
    /// <param name="batchRequest">Batch Request for Filtering, Sorting and Pagination.</param>
    /// <param name="getOnlyNonMembers">Only permissions who are not a member of the Role Id will be returned.</param>
    /// <returns></returns>
    [Authorize(AppPermissions.CanReadPermission)]
    [Authorize(AppPermissions.CanReadRole)]
    [HttpPost("batch/role")]
    public Task<BatchDto<PermissionModel>> GetPermissionsByRole([Required][FromQuery] string roleId, [FromQuery] bool getOnlyNonMembers, [FromBody] BatchRequestDto batchRequest)
    {
        var response = _permissionManager.GetPermissionsByRoleAsync(roleId, batchRequest, getOnlyNonMembers);
        return response;
    }

    [Authorize(AppPermissions.CanReadPermission)]
	[HttpGet("{id}")]
	public async Task<ActionResult<PermissionModel>> GetPermissionById(string id)
	{
		var permissionId = Guid.Parse(id);
		var result = await _permissionManager.GetPermissionByIdAsync(permissionId);
		if (result == null)
			return NotFound();
		return Ok(result);
	}

	[Authorize(AppPermissions.CanCreatePermission)]
	[HttpPost]
	public async Task<ActionResult<PermissionModel>> AddPermission([FromBody] PermissionModel model)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);
		var result = await _permissionManager.AddPermissionAsync(model);
		return Ok(result);
	}

	[Authorize(AppPermissions.CanEditPermission)]
	[HttpPut]
	public async Task<ActionResult> UpdatePermission([FromBody] PermissionModel model)
	{
		if (!ModelState.IsValid)
			return ValidationProblem(ModelState);
		var result = await _permissionManager.GetPermissionByIdAsync(model.Id);

		if (result == null)
		{
			return NotFound();
		}

		await _permissionManager.UpdatePermissionAsync(model);
		return Ok();
	}

	[Authorize(AppPermissions.CanDeletePermission)]
	[HttpDelete("{Id}")]
	public async Task<ActionResult<bool>> DeletePermission(string Id)
	{
		var permissionId = Guid.Parse(Id);
		var result = await _permissionManager.GetPermissionByIdAsync(permissionId);

		if (result == null)
		{
			return NotFound();
		}

		await _permissionManager.DeletePermissionAsync(permissionId);
		return Ok();
	}
}
