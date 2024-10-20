using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.TimeSeries;
using Willow.Model.Async;
using Willow.Model.Responses;
using Willow.Model.TimeSeries;

namespace Willow.AzureDigitalTwins.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TimeSeriesController : Controller
{
    private readonly ITimeSeriesAdxService _timeSeriesAdxService;

    public TimeSeriesController(ITimeSeriesAdxService timeSeriesAdxService)
    {
        _timeSeriesAdxService = timeSeriesAdxService;
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost("import")]
    [DisableRequestSizeLimit]
    public async Task<ActionResult<string>> TriggerImport([Required][FromBody] ImportTimeSeriesHistoricalRequest request,
                [FromHeader(Name = "User-Id")][Required] string userId,
                [FromHeader(Name = "User-Data")] string userData = null)
    {
        var importJob = await _timeSeriesAdxService.QueueBulkProcess(request, userId, userData);
        return importJob.JobId;
    }

    /// <summary>
    /// Trigger import from blob request
    /// </summary>
    /// <param name="request">Import request object with sas url</param>
    /// <param name="userId">Request user id</param>
    /// <param name="userData">Request user data</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost("sasUriImport")]
    [DisableRequestSizeLimit]
    public async Task<ActionResult<string>> TriggerImportFromBlobRequest([Required][FromBody] ImportTimeSeriesHistoricalFromBlobRequest request,
                [FromHeader(Name = "User-Id")][Required] string userId,
                [FromHeader(Name = "User-Data")] string userData = null)
    {
        var importJob = await _timeSeriesAdxService.QueueBulkProcess(request, userId, userData);
        return importJob.JobId;
    }

    /// <summary>
    /// Search async jobs filtering by query string parameters
    /// </summary>
    /// <param name="id">Async job Id</param>
    /// <param name="userId">User Id</param>
    /// <param name="status">Async job status</param>
    /// <param name="from">Async job creation date time from filter</param>
    /// <param name="to">Async job creation date time to filter</param>
    /// <param name="fullDetails">Indicates if full details body from async jobs must be retrieved</param>
    /// <returns>Returns a collection of async jobs that match the provided filters</returns>
    /// <remarks>
    /// Sample response:
    ///
    ///		[
    ///			{
    ///				"jobId": "user@domain.com.Twins.2022.08.17.14.21.49",
    ///				"details": {
    ///					"status": "Queued"
    ///				},
    ///				"createTime": "2022-08-17T13:59:29.2075064Z",
    ///				"userId": "user@domain.com",
    ///			}
    ///		]
    ///
    /// </remarks>
    /// <response code="200">Returns a collection of async jobs that match the provided filters</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TimeSeriesImportJob>>> FindImports(
        [FromQuery] string id,
        [FromQuery] string userId,
        [FromQuery] AsyncJobStatus? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] bool fullDetails = false)
    {
        var jobs = await _timeSeriesAdxService.FindImportJobs(id, status, userId, from, to, fullDetails);

        return jobs.ToList();
    }

    /// <summary>
    /// Cancel async job by id
    /// </summary>
    /// <param name="id">Async job id</param>
    /// <param name="userId">User id</param>
    /// <response code="200">Async job successfully cancelled</response>
    /// <response code="404">Async job not found</response>
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpGet("cancel/{id}")]
    public async Task<ActionResult> CancelImport(
        [Required][FromRoute] string id,
        [FromHeader(Name = "User-Id")][Required] string userId)
    {
        var jobs = await _timeSeriesAdxService.FindImportJobs(id, fullDetails: true);

        if (!jobs.Any())
            return NotFound();

        var job = jobs.Single();

        if (job.Details.Status != AsyncJobStatus.Processing && job.Details.Status != AsyncJobStatus.Queued)
            return BadRequest(new ValidationProblemDetails { Detail = "Only jobs in Processing or Queued status can be cancelled" });

        await _timeSeriesAdxService.CancelImport(job, userId);

        return Ok();
    }

    /// <summary>
    /// Get new blob upload info for new time series data
    /// </summary>
    /// <returns>container sas token</returns>
    /// <response code="200">container sas token</response>
    [HttpGet("GetTimeSeriesBlobUploadInfo", Name = "GetTimeSeriesBlobUploadInfo")]
    [ProducesResponseType(typeof(BlobUploadInfo), StatusCodes.Status200OK)]
    public async Task<BlobUploadInfo> GetBlobUploadInfo([FromQuery] string[] fileNames)
    {
        return await _timeSeriesAdxService.GetBlobUploadInfo(fileNames);
    }
}
