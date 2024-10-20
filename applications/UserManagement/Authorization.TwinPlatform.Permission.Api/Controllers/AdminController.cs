using Authorization.TwinPlatform.Permission.Api.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Authorization.TwinPlatform.Permission.Api.Controllers;

/// <summary>
/// Controller to retrieve admin details.
/// </summary>
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly ILogger<AdminController> _logger;
    private readonly AdminOption _adminOption;

    /// <summary>
    /// Initialize a new instance of <see cref="AdminController"/>
    /// </summary>
    /// <param name="logger">Instance of ILogger.</param>
    /// <param name="adminOption">Instance of IOptions.</param>
    public AdminController(ILogger<AdminController> logger,IOptions<AdminOption> adminOption)
    {
        _logger = logger;
        _adminOption = adminOption.Value;
    }

    /// <summary>
    /// Get the list of admin emails configured in permission api as super admins.
    /// </summary>
    /// <returns>List of admin email.</returns>
    [HttpGet]
    [ResponseCache(Duration =int.MaxValue)]
    public ActionResult<IEnumerable<string>> Get()
    {
        _logger.LogInformation("Received GET admin request and returning {count} response", _adminOption.Admins.Count());
        return _adminOption.Admins.ToList();
    }
}
