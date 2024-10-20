using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Authorization.TwinPlatform.Web.Controllers;

/// <summary>
/// Controller class for User Management Dashboard
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly IUserManager _userManager;
    private readonly IRoleManager _roleManager;
    private readonly IGroupManager _groupManager;
    private readonly IPermissionManager _permissionManager;
    private readonly IRoleAssignmentManager _roleAssignmentManager;
    private readonly IUserAuthorizationManager _userAuthorizationManager;

    public DashboardController(
        IUserManager userManager,
        IRoleManager roleManager,
        IGroupManager groupManager,
        IPermissionManager permissionManager,
        IRoleAssignmentManager roleAssignmentManager,
        IUserAuthorizationManager userAuthorizationManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _groupManager = groupManager;
        _permissionManager = permissionManager;
        _roleAssignmentManager = roleAssignmentManager;
        _userAuthorizationManager = userAuthorizationManager;
    }

    [Authorize]
    [HttpGet]
    public async Task<IDictionary<string, object>> GetAllData()
    {
        Dictionary<string, object> result = [];
        //Get the current user's email from the claims
        var currentUserEmailClaim = User.FindFirst(claim => claim.Type == "emails" || claim.Type == ClaimTypes.Email);

        if (currentUserEmailClaim is null) return result;

        var userAppPermissions = await _userAuthorizationManager.GetAuthorizationPermissions(currentUserEmailClaim.Value);

        var dataTaskMap = new List<(string Permission, string EntityType, int Count)>
        {
            (AppPermissions.CanReadUser,"Users", await _userManager.GetCountAsync()),
            (AppPermissions.CanReadGroup,"Groups", await _groupManager.GetCountAsync()),
            (AppPermissions.CanReadRole,"Roles", await _roleManager.GetCountAsync()),
            (AppPermissions.CanReadPermission,"Permissions", await  _permissionManager.GetCountAsync()),
            (AppPermissions.CanReadAssignment,"UserAssignments", await _roleAssignmentManager.GetUserRoleCountAsync()),
            (AppPermissions.CanReadAssignment,"GroupAssignments", await _roleAssignmentManager.GetGroupRoleCountAsync())
        };

        var authorizedDataTask = dataTaskMap.Where(w => userAppPermissions.Permissions.Contains(w.Permission) || userAppPermissions.IsAdminUser).ToList();

        foreach(var row in authorizedDataTask)
        {
            result.Add(row.EntityType, row.Count);
        }

        return result;
    }
}
