using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Willow.Model.Adt;
namespace Willow.AzureDigitalTwins.Api.Controllers;

/// <summary>
/// Env controller to get environment info
/// </summary>
[Route("[controller]")]
[ApiController]
[Authorize]
public class EnvController : ControllerBase
{
    public EnvController() { }

    /// <summary>
    /// Get the current version of ADT API build.
    /// </summary>
    /// <returns>Document stream</returns>
    /// <response code="200">Document stream</response>
    [HttpGet("version")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<AppVersion> Version()
    {
        var response = new AppVersion()
        {
            AdtApiVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString()
        };

        return Ok(response);
    }
}
