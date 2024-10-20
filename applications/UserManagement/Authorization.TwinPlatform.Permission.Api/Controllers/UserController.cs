using Authorization.Common.Enums;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Permission.Api.Abstracts;
using Authorization.TwinPlatform.Permission.Api.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.TwinPlatform.Permission.Api.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class UserController(IUserManager userManager, ILogger<UserController> logger) : ControllerBase
{
    /// <summary>
    /// Gets the list of user based on the filter
    /// </summary>
    /// <param name="filterModel">The filter model</param>
    /// <returns>List response of user model.</returns>
    [HttpGet]
    public async Task<ListResponse<UserModel>> GetListOfUsers([FromQuery] FilterPropertyModel filterModel)
    {
        logger.LogInformation("Received: Request to get list of users.");

        var results = await userManager.GetListAsync<UserModel>(filterModel);

        logger.LogInformation("Completed: Request to get list of users");

        return new ListResponse<UserModel>(results);
    }

    /// <summary>
    /// Get user by email address.
    /// </summary>
    /// <param name="email">Email Address.</param>
    /// <returns>UserModel.</returns>
    [HttpGet("email/{email}")]
    public async Task<UserModel?> GetUserByEmail([FromRoute] string email)
    {
        logger.LogInformation("Received: Request to get user by email:{email}", email);
        var result = await userManager.GetByEmailAsync(email);
        if (result == null)
        {
            logger.LogWarning("Completed: Unable to find user with email:{email}", email);
        }
        else
        {
            logger.LogInformation("Completed: Request to find user by email:{email}", email);
        }
        return result;
    }

    /// <summary>
    /// Get List of user by Ids.
    /// </summary>
    /// <param name="userId">Array of User Ids.</param>
    /// <returns>List of User Model.</returns>
    [HttpGet("byIds")]
    public async Task<ActionResult<ListResponse<UserModel>>> GetUserByIds([FromQuery] string[] userIds)
    {
        Guid[] ids;
        try
        {
             ids = userIds.Select(x => Guid.Parse(x)).ToArray();
        }
        catch (FormatException)
        {
            return ValidationProblem($"One or more userIds is not in a valid guid format: {string.Join(',', userIds)}.");
        }

        logger.LogInformation("Received: Request to get user by ids:{Ids}", string.Join(',', userIds));
        var result = await userManager.GetByIds(ids);
        logger.LogInformation("Completed: Request to find user by ids.");
        return Ok(new ListResponse<UserModel>(result));
    }
}
