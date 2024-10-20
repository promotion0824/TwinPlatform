using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Controller for getting twin graph.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GraphController"/> class.
/// </remarks>
/// <param name="graphService">Graph Service.</param>
[ApiController]
[Route("api/[controller]")]
public class GraphController(IGraphService graphService) : ControllerBase
{
    /// <summary>
    /// Returns a twin graph with the input twin ids.
    /// </summary>
    /// <param name="twinIds">Collection of input twin ids.</param>
    /// <response code="200">Target twins retrieved.</response>
    /// <response de="400">Bad Request.</response>
    /// <returns><TwinGraph>A <see cref="Task"/>a twin graph with the input twin ids.</TwinGraph></returns>
    [HttpGet("GetTwinGraph")]
    [Authorize(Policy = AppPermissions.CanReadTwins)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<TwinGraph>>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TwinGraph>> GetTwinGraph([FromBody][Required] string[] twinIds)
    {
        if (!twinIds.Any())
        {
            return BadRequest(new ValidationProblemDetails { Detail = "Root twin ids is mandatory" });
        }

        var twinGraph = await graphService.GetTwinGraph(twinIds);
        return Ok(twinGraph);
    }
}
