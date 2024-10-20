using Authorization.Common.Models;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.TwinPlatform.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GroupTypeController : ControllerBase
{
    private readonly ILogger<GroupTypeController> _logger;
    private readonly IGroupTypeManager _groupTypeManager;

    public GroupTypeController(ILogger<GroupTypeController> logger, IGroupTypeManager groupTypeManager)
    {
        _logger = logger;
        _groupTypeManager = groupTypeManager;
    }

    [Authorize(AppPermissions.CanReadGroup)]
    [HttpGet]
    public async Task<IEnumerable<GroupTypeModel>> GetGroupTypes()
    {
        return await _groupTypeManager.GetGroupTypesAsync();
    }
}
