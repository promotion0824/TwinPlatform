using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Willow.Exceptions;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Controller for Twins Export.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IFileExporterService _exporterService;
    private readonly DateTimeOffset now = DateTimeOffset.UtcNow;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExportController"/> class.
    /// </summary>
    /// <param name="exporterService">Implementation of Exporter Service.</param>
    public ExportController(IFileExporterService exporterService)
    {
        ArgumentNullException.ThrowIfNull(exporterService, nameof(exporterService));
        _exporterService = exporterService;
    }

    /// <summary>
    /// Get the Twins from ADT, for the given query parameters and download in form of zipped CSV-s, grouped by modelId-s.
    /// </summary>
    /// <param name="modelIds">Filters twins by Model Ids.</param>
    /// <param name="locationId">Location Id.</param>
    /// <param name="exactModelMatch">Flag to include only twins with exact match of the Model Id.</param>
    /// <param name="includeRelationships">Flag to include Outgoing Relationships in a result.</param>
    /// <param name="includeIncomingRelationships">Flag to include Incoming Relationships in a result.</param>
    /// <param name="isTemplateExportOnly">Flag to export template file only.</param>
    /// <remarks>
    /// Sample request:
    /// 	GET https://[base-address]/Export/Twins?includeRelationships=true
    /// Sample response:
    /// 	Byte array with file explorer prompt to save on a physical location (user's local pc).
    /// </remarks>
    /// <returns>Compressed(.zip) file with Twins in form of CSV files, grouped by modelIds.</returns>
    [HttpPost("twins")]
    [Authorize(Policy = AppPermissions.CanExportTwins)]
    [Consumes("application/json")]
    [Produces("application/zip")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(FileContentResult))]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ExportTwinsAsync(
        [FromBody] string[] modelIds,
        [FromQuery] string locationId,
        [FromQuery] bool? exactModelMatch,
        [FromQuery] bool? includeRelationships,
        [FromQuery] bool? includeIncomingRelationships,
        [FromQuery] bool? isTemplateExportOnly)
    {
        var file = await _exporterService.ExportZippedTwinsAsync(
                locationId,
                modelIds,
                exactModelMatch,
                includeRelationships,
                includeIncomingRelationships,
                isTemplateExportOnly);

        return File(
            file,
            "application/zip",
            $"ExportedTwins-{now.Year}-{now.Month:00}-{now.Day:00}-{now.Hour:00}-{now.Minute:00}.zip");
    }

    /// <summary>
    /// Get the Twins from ADT, for the given query parameters and download in form of zipped CSV-s.
    /// </summary>
    /// <param name="twinIds">Twin Ids.</param>
    /// <remarks>
    /// Sample request:
    /// 		GET https://[base-address]/Export/TwinIds.
    /// Sample response:
    /// 		Byte array with file explorer prompt to save on a physical location (user's local pc).
    /// </remarks>
    /// <returns>Compressed(.zip) file with Twins in form of CSV files.</returns>
    [HttpPost("twinIds")]
    [Authorize(Policy = AppPermissions.CanExportTwins)]
    [Consumes("application/json")]
    [Produces("application/zip")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(FileContentResult))]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ExportTwinsByTwinIdsAsync([FromBody] string[] twinIds)
    {
        var file = await _exporterService.ExportZippedTwinsByTwinIdsAsync(twinIds);
        if (file == null)
        {
            return NotFound();
        }
        else
        {
            return File(
                    file,
                    "application/zip",
                    $"ExportedTwins-{now.Year}-{now.Month:00}-{now.Day:00}-{now.Hour:00}-{now.Minute:00}.zip");
        }
    }
}
