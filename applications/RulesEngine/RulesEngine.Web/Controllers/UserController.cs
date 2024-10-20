using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RulesEngine.Web;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WillowRules.Logging;

namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for users
/// </summary>
[AllowAnonymous]
[ApiExplorerSettings(GroupName = "v1")]
public class UserController : ControllerBase
{
    private readonly IUserService userService;
    private readonly IMemoryCache memoryCache;
    private readonly IPolicyDecisionService policyDecisionService;
    private readonly ILogger<UserController> logger;
    private readonly IAuditLogger<UserController> auditLogger;

    /// <summary>
    /// Creates a new <see cref="UserController" />
    /// </summary>
    public UserController(
        IUserService userService,
        IMemoryCache memoryCache,
        IPolicyDecisionService policyDecisionService,
        ILogger<UserController> logger,
        IAuditLogger<UserController> auditLogger)
    {
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        this.policyDecisionService = policyDecisionService ?? throw new ArgumentNullException(nameof(policyDecisionService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.auditLogger = auditLogger ?? throw new ArgumentNullException(nameof(auditLogger));
    }

    /// <summary>
    /// Gets a user
    /// </summary>
    /// <param name="id">Id for the user</param>
    /// <returns>The user object with policy evaluations</returns>
    [HttpGet]
    [Route("api/user/{id}", Name = "GetUserInfo")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthenticatedUserAndPolicyDecisionsDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(string))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetUserInfo(string id = "me")
    {
        try
        {
            var userId = this.User.FindFirstValue(ClaimTypes.NameIdentifier);  // A guid
            if (string.IsNullOrEmpty(userId)) return BadRequest("Missing user");

            var result = await memoryCache.GetOrCreateAsync(userId + "_userinfo", async (c) =>
            {
                // Wait more than 30s between page loads and we refresh your permissions
                c.SetSlidingExpiration(TimeSpan.FromSeconds(30));

                // These next two calls both cache internally
                var userDto = await userService.GetUser(this.User);
                var policyDecisions = await policyDecisionService.GetPolicyDecisions(this.User, null);

                var userAndPolicyDto = new AuthenticatedUserAndPolicyDecisionsDto(userDto, policyDecisions);

                auditLogger.LogInformation(
                    userId: userDto.Email,
                    scope: new() { ["Permissions"] = string.Join(",", policyDecisions.Where(x => x.Success).Select(x => x.Name)) },
                    messageTemplate: "Login");

                return userAndPolicyDto;
            });

            logger.LogInformation("Loaded user {email} {name}", result.User.Email, result.User.DisplayName);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching user");
            throw;
        }
    }
}
