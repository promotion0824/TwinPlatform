using Authorization.TwinPlatform.Web.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.TwinPlatform.Web.Controllers;

/// <summary>
/// Twins Controller
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class TwinsController (ITwinManager twinManager) : ControllerBase
{
    [Authorize]
    [HttpGet("locations")]
    public async Task<ActionResult> GetLocationTwins()
    {
        var response = await twinManager.GetTwinLocationsAsync();
        return  Ok(response);
    }
}
