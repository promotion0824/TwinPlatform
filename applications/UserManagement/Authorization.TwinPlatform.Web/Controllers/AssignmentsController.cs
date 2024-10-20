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
public class AssignmentsController : ControllerBase
{
    private readonly IRoleAssignmentManager _roleAssignmentManager;
    private readonly IClientAssignmentManager _clientAssignmentManager;

    public AssignmentsController(IRoleAssignmentManager roleAssignmentManager, IClientAssignmentManager clientAssignmentManager)
    {
        _roleAssignmentManager = roleAssignmentManager;
        _clientAssignmentManager = clientAssignmentManager;
    }

    [Authorize(AppPermissions.CanReadAssignment)]
    [HttpPost("user/batch")]
    public async Task<BatchDto<UserRoleAssignmentModel>> GetUserAssignmentsBatch(BatchRequestDto batchRequest)
    {
        return await _roleAssignmentManager.GetUserRoleAssignmentsAsync(batchRequest);
    }

    [Authorize(AppPermissions.CanCreateAssignment)]
    [HttpPost("user")]
    public async Task<ActionResult> CreateUserAssignment([FromBody] UserRoleAssignmentModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        var expressionStatus = _roleAssignmentManager.EvaluateConditionExpression(model.Condition, out List<string> errors);
        if (expressionStatus == ExpressionStatus.Error)
        {
            ModelState.AddModelError(nameof(model.Condition), string.Join(',', errors));
            return ValidationProblem(ModelState);
        }
        var response = await _roleAssignmentManager.AddUserRoleAssignmentAsync(model);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        return Created();
    }

    [Authorize(AppPermissions.CanEditAssignment)]
    [HttpPut("user")]
    public async Task<ActionResult> UpdateUserAssignment([FromBody] UserRoleAssignmentModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        var expressionStatus = _roleAssignmentManager.EvaluateConditionExpression(model.Condition, out List<string> errors);
        if (expressionStatus == ExpressionStatus.Error)
        {
            ModelState.AddModelError(nameof(model.Condition), string.Join(',', errors));
            return ValidationProblem(ModelState);
        }
        var response = await _roleAssignmentManager.UpdateUserRoleAssignment(model);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }
        return Ok();
    }

    [Authorize(AppPermissions.CanDeleteAssignment)]
    [HttpDelete("user/{id}")]
    public async Task<ActionResult> DeleteUserAssignment([FromRoute] string Id)
    {
        if (!Guid.TryParse(Id, out var idToRemove))
        {
            return BadRequest();
        }
        var response = await _roleAssignmentManager.DeleteUserRoleAssignmentAsync(idToRemove);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }
        return Ok();
    }

    [Authorize(AppPermissions.CanReadAssignment)]
    [HttpPost("group/batch")]
    public async Task<BatchDto<GroupRoleAssignmentModel>> GetGroupAssignmentsBatch(BatchRequestDto batchRequest)
    {
        return await _roleAssignmentManager.GetGroupRoleAssignmentsAsync(batchRequest);
    }

    [Authorize(AppPermissions.CanCreateAssignment)]
    [HttpPost("group")]
    public async Task<ActionResult> CreateGroupAssignment([FromBody] GroupRoleAssignmentModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        var expressionStatus = _roleAssignmentManager.EvaluateConditionExpression(model.Condition, out List<string> errors);
        if (expressionStatus == ExpressionStatus.Error)
        {
            ModelState.AddModelError(nameof(model.Condition), string.Join(',', errors));
            return ValidationProblem(ModelState);
        }
        var response = await _roleAssignmentManager.AddGroupRoleAssignmentAsync(model);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }
        return Created();
    }

    [Authorize(AppPermissions.CanEditAssignment)]
    [HttpPut("group")]
    public async Task<ActionResult> UpdateGroupAssignment([FromBody] GroupRoleAssignmentModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        var expressionStatus = _roleAssignmentManager.EvaluateConditionExpression(model.Condition, out List<string> errors);
        if (expressionStatus == ExpressionStatus.Error)
        {
            ModelState.AddModelError(nameof(model.Condition), string.Join(',', errors));
            return ValidationProblem(ModelState);
        }
        var response = await _roleAssignmentManager.UpdateGroupRoleAssignment(model);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }
        return Ok();
    }

    [Authorize(AppPermissions.CanDeleteAssignment)]
    [HttpDelete("group/{id}")]
    public async Task<ActionResult> DeleteGroupAssignment([FromRoute] string Id)
    {
        if (!Guid.TryParse(Id, out var idToRemove))
        {
            return BadRequest();
        }
        var response = await _roleAssignmentManager.DeleteGroupRoleAssignmentAsync(idToRemove);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }
        return Ok();
    }

    [Authorize(AppPermissions.CanReadClientAssignment)]
    [HttpGet("client/{applicationName}")]
    public async Task<IEnumerable<ClientAssignmentModel>> GetClientAssignments([FromRoute]string applicationName, [FromQuery] FilterPropertyModel filter)
    {
        return await _clientAssignmentManager.GetClientAssignmentsAsync(applicationName, filter);
    }

    [Authorize(AppPermissions.CanCreateClientAssignment)]
    [HttpPost("client")]
    public async Task<ActionResult> CreateClientAssignment([FromBody] ClientAssignmentModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        var expressionStatus = _clientAssignmentManager.EvaluateConditionExpression(model.Condition, out List<string> errors);
        if (expressionStatus == ExpressionStatus.Error)
        {
            ModelState.AddModelError(nameof(model.Condition), string.Join(',', errors));
            return ValidationProblem(ModelState);
        }
        var response = await _clientAssignmentManager.AddClientAssignmentAsync(model);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }
        return Created();
    }

    [Authorize(AppPermissions.CanEditClientAssignment)]
    [HttpPut("client")]
    public async Task<ActionResult> UpdateClientAssignment([FromBody] ClientAssignmentModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        var expressionStatus = _clientAssignmentManager.EvaluateConditionExpression(model.Condition, out List<string> errors);
        if (expressionStatus == ExpressionStatus.Error)
        {
            ModelState.AddModelError(nameof(model.Condition), string.Join(',', errors));
            return ValidationProblem(ModelState);
        }
        var response = await _clientAssignmentManager.UpdateClientAssignmentAsync(model);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }
        return Ok();
    }

    [Authorize(AppPermissions.CanDeleteClientAssignment)]
    [HttpDelete("client/{id}")]
    public async Task<ActionResult> DeleteClientAssignment([FromRoute] string Id)
    {
        if (!Guid.TryParse(Id, out var idToRemove))
        {
            return BadRequest();
        }
        var response = await _clientAssignmentManager.DeleteClientAssignmentAsync(idToRemove);
        if (response.FailedAuthorization)
        {
            return Unauthorized();
        }
        else if (response.Result is null)
        {
            return NotFound();
        }
        return Ok();
    }

    [Authorize]
    [HttpPost("validate")]
    public List<string> ValidateBoolReturn([FromBody] string expression)
    {
        _ = _roleAssignmentManager.EvaluateConditionExpression(expression, out var errors);
        return errors;
    }
}
