using System.ComponentModel.DataAnnotations;
using Authorization.TwinPlatform.Permission.Api.Abstracts;
using Authorization.TwinPlatform.Permission.Api.Requests;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.TwinPlatform.Permission.Api.Controllers;

/// <summary>
/// Class to handle Authorization Permission request
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class PermissionController : ControllerBase
{

    private readonly ILogger<PermissionController> _logger;
    private readonly IPermissionAggregationManager _permissionAggrManager;

    public PermissionController(ILogger<PermissionController> logger, IPermissionAggregationManager permissionAggregationManager)
    {
        _logger = logger;
        _permissionAggrManager = permissionAggregationManager;
    }


    /// <summary>
    /// Method to get allowed permissions for user filtered strictly by extension and optionally by resource Id
    /// </summary>
    /// <param name="permissionRequest">Instance of UserPermissionRequest</param>
    /// <returns>List of Permission Response</returns>
    [HttpGet]
    public async Task<ActionResult> GetAllowedPermissionForUser([FromQuery][Required] UserPermissionRequest permissionRequest)
    {
        _logger.LogInformation("Received: Permission request for User: {User} App: {App} ", permissionRequest.UserEmail, permissionRequest.Extension);

        if (!ModelState.IsValid)
        {
            _logger.LogInformation("Invalid Permission request for User: {User} App: {App} ", permissionRequest.UserEmail, permissionRequest.Extension);

            return ValidationProblem(ModelState);
        }

        var result = await _permissionAggrManager.GetAllowedPermissionForUser(permissionRequest);

        _logger.LogInformation("Completed: Permission request for User: {User} App: {App} Permissions:{Count} AdminUser:{IsAdmin}",
            permissionRequest.UserEmail, permissionRequest.Extension, result.Permissions?.Count() ?? 0, result.IsAdminUser);

        return Ok(result);
    }

    [HttpGet("client")]
    public async Task<ActionResult> GetAllowedPermissionForClient([FromQuery][Required] ClientPermissionRequest permissionRequest)
    {
        _logger.LogInformation("Received: Permission request for Client: {ClientId} App: {App} ", permissionRequest.ClientId, permissionRequest.Application);
        if (!ModelState.IsValid)
        {
            _logger.LogInformation("Invalid Permission request for Client: {ClientId} App: {App} ", permissionRequest.ClientId, permissionRequest.Application);

            return ValidationProblem(ModelState);
        }

        var result = await _permissionAggrManager.GetAllowedPermissionForClient(permissionRequest);
        _logger.LogInformation("Completed: Permission request for Client: {ClientId} App: {App} Permissions:{Count}",
            permissionRequest.ClientId, permissionRequest.Application, result?.Count ?? 0);
        return Ok(result);
    }

}
