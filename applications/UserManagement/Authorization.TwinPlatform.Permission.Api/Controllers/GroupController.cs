using Authorization.Common.Models;
using Authorization.TwinPlatform.Permission.Api.Abstracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Willow.Batch;

namespace Authorization.TwinPlatform.Permission.Api.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class GroupController(IGroupManager groupManager, ILogger<GroupController> logger) : ControllerBase
{
    /// <summary>
    /// Get batch of groups with Group Type: Application
    /// </summary>
    /// <param name="batchRequest">Batch Request DTO</param>
    /// <returns>BatchDTO of group model.</returns>
    [HttpPost("batch")]
    public async Task<BatchDto<GroupModel>> GetApplicationGroupsAsync([FromBody] BatchRequestDto batchRequest)
    {
        logger.LogInformation("Received: Request to batch groups: {request}", JsonSerializer.Serialize(batchRequest));
        var results = await groupManager.GetApplicationGroupsAsync(batchRequest);

        logger.LogInformation("Completed: Get batch of application groups with total count:{Count} ", results.Total);

        return results;
    }

    /// <summary>
    /// Get batch of groups with Group Type: Application for the User (UserId)
    /// </summary>
    /// <param name="userId">Id of the user.</param>
    /// <param name="batchRequest">Batch Request DTO</param>
    /// <returns>BatchDTO of group model.</returns>
    [HttpPost("batch/User")]
    public async Task<BatchDto<GroupModel>> GetApplicationGroupsByUserAsync([FromQuery][Required] string userId, [FromBody] BatchRequestDto batchRequest)
    {
        logger.LogInformation("Received: Request to batch groups: {request}", JsonSerializer.Serialize(batchRequest));
        var results = await groupManager.GetApplicationGroupsByUserIdAsync(Guid.Parse(userId), batchRequest);

        logger.LogInformation("Completed: Get batch of application groups with total count:{Count} ", results.Total);

        return results;
    }


}

