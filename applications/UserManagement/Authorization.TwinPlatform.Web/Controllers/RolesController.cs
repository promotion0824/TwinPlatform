using Authorization.Common.Models;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Batch;

namespace Authorization.TwinPlatform.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleManager _roleManager;

    public RolesController(IRoleManager roleManager)
    {
        _roleManager = roleManager;
    }

    [Authorize(AppPermissions.CanReadRole)]
    [HttpPost("batch")]
    public Task<BatchDto<RoleModelWithPermissions>> GetRoles([FromBody] BatchRequestDto batchRequest)
    {
        return _roleManager.GetRolesAsync(batchRequest);
    }

    [Authorize(AppPermissions.CanReadRole)]
    [HttpGet]
    public async Task<ActionResult<RoleModelWithPermissions>> GetRoleByName([FromQuery]string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest();
        }
        var result = await _roleManager.GetByNameAsync(name);
        if (result == null)
            return NotFound();
        return result;
    }

    [Authorize(AppPermissions.CanCreateRole)]
    [HttpPost]
    public async Task<ActionResult> CreateRole([FromBody] RoleModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        await _roleManager.AddRoleAsync(model);
        return Ok();
    }

    [Authorize(AppPermissions.CanEditRole)]
    [HttpPut]
    public async Task<ActionResult<bool>> UpdateRole([FromBody] RoleModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        var result = await _roleManager.GetRoleByIdAsync(model.Id);

        if (result == null)
        {
            return NotFound(new ValidationProblemDetails() { Title = "Role does not exist", Detail = "Role Record does not exist" });
        }

        return Ok(await _roleManager.UpdateRoleAsync(model));
    }

    [Authorize(AppPermissions.CanDeleteRole)]
    [HttpDelete("{Id}")]
    public async Task<ActionResult<bool>> DeleteRole(string Id)
    {
        var roleId = Guid.Parse(Id);
        var result = await _roleManager.GetRoleByIdAsync(roleId);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(await _roleManager.DeleteRoleAsync(roleId));
    }

    [Authorize(AppPermissions.CanAssignPermission)]
    [HttpPost("{Id}/permissions")]
    public async Task<ActionResult> AssignPermission([FromRoute] string Id, [FromBody] PermissionModel permission)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        if (!Guid.TryParse(Id, out var roleId) || !ModelState.IsValid)
        {
            return BadRequest();
        }
        await _roleManager.AssignPermission(roleId, permission);

        return Ok();
    }

    [Authorize(AppPermissions.CanRemovePermission)]
    [HttpDelete("{Id}/permissions/{pId}")]
    public async Task<ActionResult> RemovePermission([FromRoute] string Id, [FromRoute] string pId)
    {
        if (!Guid.TryParse(Id, out var roleId) || !Guid.TryParse(pId, out var permissionId))
        {
            return BadRequest();
        }
        await _roleManager.RemovePermission(roleId, new PermissionModel() { Id = permissionId });
        return Ok();
    }
}

