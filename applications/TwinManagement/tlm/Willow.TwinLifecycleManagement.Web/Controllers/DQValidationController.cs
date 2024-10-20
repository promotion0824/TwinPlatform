using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Willow.DataQuality.Model.Validation;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Data Quality Validation Controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DQValidationController : ControllerBase
{
    private readonly IDataQualityService _dataQualityService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DQValidationController"/> class.
    /// </summary>
    /// <param name="dataQualityService">Implementation of DataQuality Service.</param>
    public DQValidationController(IDataQualityService dataQualityService)
    {
        ArgumentNullException.ThrowIfNull(dataQualityService, nameof(dataQualityService));
        _dataQualityService = dataQualityService;
    }

    /// <summary>
    /// Get Data Quality Validation Jobs.
    /// </summary>
    /// <param name="id">Id of the Job.</param>
    /// <param name="userId">User Id.</param>
    /// <param name="status">Status of the job.</param>
    /// <param name="from">Data from.</param>
    /// <param name="to">Data til.</param>
    /// <param name="fullDetails">True if full details; else false.</param>
    /// <returns><see cref="TwinsValidationJob"/>.</returns>
    [HttpGet("validationJobs", Name = "getDQValidationJobs")]
    [Authorize(Policy = AppPermissions.CanReadDQValidationJobs)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(IEnumerable<TwinsValidationJob>))]
    [ProducesResponseType(typeof(IEnumerable<TwinsValidationJob>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TwinsValidationJob>>> GetDQValidationJobs(
        [FromQuery] string id = null,
        [FromQuery] string userId = null,
        [FromQuery] AsyncJobStatus? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] bool fullDetails = true)
    {
        var jobs = await _dataQualityService.GetDQValidationJobs(id, status, userId, from, to, fullDetails);
        return jobs.ToList();
    }

    /// <summary>
    /// Get Latest Data Quality Validation jobs.
    /// </summary>
    /// <param name="status">Async Job Status.</param>
    /// <returns><see cref="TwinsValidationJob"/>.</returns>
    [HttpGet("latestValidationJob", Name = "getLatestDQValidationJob")]
    [Authorize(Policy = AppPermissions.CanReadDQValidationJobs)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(TwinsValidationJob))]
    [ProducesResponseType(typeof(TwinsValidationJob), StatusCodes.Status200OK)]
    public async Task<ActionResult<TwinsValidationJob>> GetLatestDQValidationJob(
        [FromQuery] AsyncJobStatus? status = null)
    {
        var job = await _dataQualityService.GetLatestDQValidationJob(status);
        return job;
    }

    /// <summary>
    /// Trigger Validation.
    /// </summary>
    /// <param name="userId">User Id.</param>
    /// <param name="modelIds">Array of Model Ids.</param>
    /// <param name="locationId">Location Id.</param>
    /// <param name="isIncrementalScan">True if incremental scan.</param>
    /// <param name="exactModelMatch">True if exact model match; else false.</param>
    /// <returns><see cref="TwinsValidationJob"/>.</returns>
    [HttpGet("validate", Name = "validateTwins")]
    [Authorize(Policy = AppPermissions.CanValidateTwins)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(TwinsValidationJob))]
    [ProducesResponseType(typeof(TwinsValidationJob), StatusCodes.Status200OK)]
    public async Task<ActionResult<TwinsValidationJob>> Validate(
    [FromQuery] string userId = null,
    [FromQuery] string[] modelIds = null,
    [FromQuery] string locationId = null,
    [FromQuery] bool isIncrementalScan = false,
    [FromQuery] bool? exactModelMatch = null)
    {
        DateTimeOffset? startCheckTime = null;
        DateTimeOffset? endCheckTime = null;
        if (isIncrementalScan)
        {
            var latestSuccessfulDQValidationJob = await _dataQualityService.GetLatestDQValidationJob(status: AsyncJobStatus.Done);
            startCheckTime = latestSuccessfulDQValidationJob?.EndTime?.ToLocalTime() ?? new DateTimeOffset(DateTime.MinValue).ToLocalTime();
            endCheckTime = DateTimeOffset.Now;
        }

        var job = await _dataQualityService.DQValidate(userId, modelIds, locationId, exactModelMatch, startCheckTime, endCheckTime);
        return job;
    }

    /// <summary>
    /// Get Twin Data Quality Results By Model Ids.
    /// </summary>
    /// <param name="errorsOnly">True if errors only; else false.</param>
    /// <param name="modelIds">Array of Model Ids.</param>
    /// <param name="resultSources">Array of result sources.</param>
    /// <param name="pageSize">Page Size.</param>
    /// <param name="continuationToken">Continuation Token String.</param>
    /// <param name="searchString">Search Text.</param>
    /// <param name="locationId">Location Id.</param>
    /// <returns>Page of <see cref="ValidationResults"/>.</returns>
    [HttpGet("results", Name = "getDQResults")]
    [Authorize(Policy = AppPermissions.CanReadDQValidationResults)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Page<ValidationResults>))]
    [ProducesResponseType(typeof(Page<ValidationResults>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Page<ValidationResults>>> GetTwinDataQualityResultsByModelIds(
    [FromQuery] bool errorsOnly = false,
    [FromQuery] string[] modelIds = null,
    [FromQuery] string[] resultSources = null,
    [FromQuery] int pageSize = 100,
    [FromHeader] string continuationToken = null,
    [FromQuery] string searchString = null,
    [FromQuery] string locationId = null)
    {
        Result[] resultTypes = null;

        if (errorsOnly)
        {
            // Include all Result enum values except Result.Ok
            resultTypes = Enum.GetValues(typeof(Result))
                                .Cast<Result>()
                                .Where(r => r != Result.Ok)
                                .ToArray();
        }

        var results = await _dataQualityService.GetTwinDataQualityResultsByModelIds(modelIds, resultSources, resultTypes, pageSize, continuationToken, searchString, locationId);
        return results;
    }
}
