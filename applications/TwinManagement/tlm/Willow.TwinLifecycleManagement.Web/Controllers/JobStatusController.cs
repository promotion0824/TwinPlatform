using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Exceptions;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.Model.Responses;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Job Status Controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class JobStatusController : ControllerBase
{
    private readonly IJobStatusService _jobStatusService;

    /// <summary>
    /// Initializes a new instance of the <see cref="JobStatusController"/> class.
    /// </summary>
    /// <param name="jobStatusService">Service to retrieve status of async jobs.</param>
    public JobStatusController(IJobStatusService jobStatusService)
    {
        ArgumentNullException.ThrowIfNull(jobStatusService);
        _jobStatusService = jobStatusService;
    }


    /// <summary>
    /// Get Time Series Import Job.
    /// </summary>
    /// <param name="jobId">Id of the Job.</param>
    /// <returns><see cref="TimeSeriesImportJob"/>.</returns>
    [HttpGet("timeSeries/{jobId}")]
    [Authorize(Policy = AppPermissions.CanReadJobs)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(TimeSeriesImportJob))]
    [ProducesResponseType(typeof(TimeSeriesImportJob), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TimeSeriesImportJob>> GetTimeSeriesJobStatus([FromRoute] string jobId)
    {
        var job = await _jobStatusService.GetTimeSeriesJobStatus(jobId);
        return job != null ? Ok(job) : NotFound("Job not found");
    }


    /// <summary>
    /// Cancel time series import job using distinct Job Id provided by the user.
    /// </summary>
    /// <param name="jobId">Job ID.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>
    /// Empty response from API.
    /// </returns>
    [HttpPost("CancelTimeSeriesImport/{jobId}")]
    [Authorize(Policy = AppPermissions.CanCancelJobs)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(ActionResult))]
    [ProducesResponseType(typeof(ActionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> CancelTimeSeriesImportJob(
        [Required][FromRoute] string jobId,
        [Required][FromHeader(Name = "User-Id")] string userId)
    {
        await _jobStatusService.CancelTimeSeriesImportJob(jobId, userId);

        return Ok();
    }
}
