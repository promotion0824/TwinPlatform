using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System;
using Willow.Model.Requests;
using Willow.AzureDigitalTwins.Api.Model.Response;
using System.Text.Json;
using Willow.Model.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Willow.AzureDigitalTwins.Api.Services.Hosted.Jobs;
using Willow.Batch;

namespace Willow.AzureDigitalTwins.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobsService _jobsService;
    private readonly IServiceProvider _serviceProvider;

    public JobsController(IJobsService jobsService, IServiceProvider serviceProvider)
    {
        _jobsService = jobsService;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Get Job info
    /// </summary>
    /// <returns>Get Job entry</returns>
    [HttpGet("{jobId}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<JobsEntry>> GetJobsEntry([Required][FromRoute] string jobId)
    {
        if (string.IsNullOrEmpty(jobId))
        {
            return BadRequest("Invalid JobId");
        }

        var entry = await _jobsService.GetJob(jobId, includeDetail: true);
        if (entry == null)
        {
            return NotFound();
        }

        return Ok(entry);
    }

    [HttpPost("onDemand")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> CreateOnDemandJob([Required][FromBody] JsonDocument jobPayload,
        [FromHeader(Name = "User-Id")][Required] string userId,
        [FromHeader(Name = "User-Data")] string userData = null)
    {
        var jobOption = jobPayload.Deserialize<JobBaseOption>();
        if (string.IsNullOrWhiteSpace(jobOption.Use))
        {
            return BadRequest("Job must specify the type of processor to use. Missing parameter in the payload [Use].");
        }
        if (string.IsNullOrWhiteSpace(jobOption.JobName))
        {
            return BadRequest("Job must specify a job name. Missing parameter in the payload [JobName].");
        }
        var targetProcessor = _serviceProvider.GetServices<IJobProcessor>().SingleOrDefault(x => x.GetType().Name.ToLowerInvariant() == jobOption.Use.ToLowerInvariant());
        if (targetProcessor == null)
        {
            return BadRequest($"Cannot find the target job processor: Use :{jobOption.Use}");
        }

        var job = await targetProcessor.CreateJobAsync(jobPayload, userId, userData, default);
        var jobEntry = await _jobsService.CreateOrUpdateJobEntry(job);

        return jobEntry.JobId;
    }

    /// <summary>
    /// Creates/Updates a Job entry
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<JobsEntry>> CreateOrUpdateJobEntry([Required][FromBody] JobsEntry entry)
    {
        return await _jobsService.CreateOrUpdateJobEntry(entry);
    }

    /// <summary>
    /// Find a Job entry
    /// </summary>
    [HttpPost("FindJobs")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<JobsResponse>> FindJobs([Required][FromBody] JobSearchRequest searchJobs, [FromQuery] bool isPagination = true)
    {
        var jobs = _jobsService.FindJobEntries(searchJobs, isPagination);
        var count = await _jobsService.GetJobEntriesCount(searchJobs);

        var result = new JobsResponse
        {
            TotalCount = count,
            Jobs = jobs
        };

        return Ok(result);
    }

    /// <summary>
    /// List Jobs Entry using Willow Pagination Library.
    /// </summary>
    [HttpPost("ListJobs")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<JobsResponse> ListJobs([Required][FromBody] BatchRequestDto batchRequestDto,
        [FromQuery] bool includeDetails = false,
        [FromQuery] bool includeTotalCount = false)
    {
        var (Jobs, Count) = _jobsService.ListJobEntries(batchRequestDto, includeDetails, includeTotalCount);

        var result = new JobsResponse
        {
            TotalCount = Count,
            Jobs = Jobs
        };

        return Ok(result);
    }

    /// <summary>
    /// Get all Job types
    /// </summary>
    [HttpGet("JobTypes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<string>>> GetAllJobTypes()
    {
        var jobTypes = await _jobsService.GetAllJobTypes().ToListAsync();

        return Ok(jobTypes);
    }

    /// <summary>
    /// Delete Jobs entries with the input job ids.
    /// Ignored bad or not found job ids.
    /// </summary>
    /// <returns>Total number of job entries deleted.</returns>
    /// <response code="404">No job ids input</response>
    /// <response code="200">Total deleted items</response>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status428PreconditionRequired)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("deleteBulk")]
    public async Task<ActionResult<int>> DeleteJobsEntries([Required][FromBody] IEnumerable<string> jobIds, [FromQuery] bool isHardDelete = false)
    {
        if (!jobIds.Any())
        {
            return BadRequest("No Job ids in input request");
        }

        var (deleted, notDeletedJobs) = await _jobsService.DeleteBulkJobs(jobIds, isHardDelete);
        if (notDeletedJobs.Any())
        {
            return StatusCode(StatusCodes.Status428PreconditionRequired, $"Unable to delete Jobs in Processing state: {string.Join(",", notDeletedJobs)}");
        }
        return Ok(deleted);
    }

    /// <summary>
    /// Delete Jobs entries with Jobs TimeCreated earlier than the input date
    /// </summary>
    /// <returns>Total number of job entries deleted.</returns>
    /// <response code="404">Date is in the future</response>
    /// <response code="200">Total deleted items</response>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("deleteJobsOlderThan")]
    public async Task<ActionResult<int>> DeleteOlderJobEntries([Required][FromQuery] DateTimeOffset date, [FromQuery] string jobType = null, [FromQuery] bool hardDelete = false)
    {
        if (date > DateTime.Now)
        {
            return BadRequest("Input for Date needs to be in the past.");
        }

        return await _jobsService.DeleteOlderJobs(date, jobType, hardDelete);
    }
}
