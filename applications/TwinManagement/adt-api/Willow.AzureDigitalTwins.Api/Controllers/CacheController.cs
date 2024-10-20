using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Api.Controllers;

[Route("[controller]")]
[ApiController]
[Authorize]
public class CacheController : ControllerBase
{
    private readonly ILogger<CacheController> _logger;
    private readonly IAzureDigitalTwinCacheProvider _azureDigitalTwinCacheProvider;

    public CacheController(ILogger<CacheController> logger, IAzureDigitalTwinCacheProvider azureDigitalTwinCacheProvider)
    {
        _logger = logger;
        _azureDigitalTwinCacheProvider = azureDigitalTwinCacheProvider;
    }

    /// <summary>
    /// Clears the cache from the memory for the supplied entity types
    /// </summary>
    /// <param name="entityTypes">Array of Entity Types ["Twins","Models","Relationships"]</param>
    /// <returns>Empty Ok Result</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST
    ///     ["twins", "relationships", "models"]
    ///
    /// </remarks>
    /// <response code="200">Returns the Ok result with no content</response>
    /// <response code="400">When supplied entity types array is not valid.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost("clear")]
    public async Task<IActionResult> ClearCache([FromBody][Required] IEnumerable<EntityType> entityTypes)
    {
        _logger.LogInformation("Request received for ClearCache.");
        if (!entityTypes.Any())
        {
            _logger.LogError("Request for ClearCache failed. Must specify atleast one entity type.");
            return BadRequest(new ValidationProblemDetails
            {
                Detail = "Invalid Entity Types. Must specify atleast one entity type to clear from cache."
            });
        }

        await _azureDigitalTwinCacheProvider.ClearCacheAsync(entityTypes);
        _logger.LogInformation("Request for ClearCache completed successfully.");

        return Ok();
    }

    /// <summary>
    /// Clears and reloads the cache
    /// </summary>
    /// <returns>No Content</returns>
    /// <response code="204">No Content</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RefreshCache()
    {
        _logger.LogInformation("Request received for RefreshCache.");

        await _azureDigitalTwinCacheProvider.RefreshCacheAsync();

        _logger.LogInformation("Request for RefreshCache completed successfully.");

        return NoContent();
    }

}

