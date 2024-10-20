using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.DataQuality;
using Willow.DataQuality.Model.Capability;

namespace Willow.AzureDigitalTwins.DataQuality.Api.Controllers;

/// <summary>
/// Data Quality controller of capability status
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class DQCapabilityController : Controller
{

    private readonly IDataQualityAdxService _dataQualityAdxService;

    public DQCapabilityController(IDataQualityAdxService dataQualityAdxService)
    {
        _dataQualityAdxService = dataQualityAdxService;
    }
    /// <summary>
    /// Creates status
    /// </summary>
    /// <response code="200">Returns when status created</response>
    /// <response code="400">If no status provided</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<ActionResult> CreateStatus([FromBody][Required] IEnumerable<CapabilityStatusDto> status)
    {
        if (!status.Any())
            return BadRequest(new ValidationProblemDetails { Detail = "Need status values." });

        await _dataQualityAdxService.IngestCapabilityStatusToValidationTableAsync(status);
        return Ok();
    }
}
