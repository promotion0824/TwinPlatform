using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.Exceptions;
using Willow.Batch;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers
{
    /// <summary>
    /// Unified Jobs Controller.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UnifiedJobsController : ControllerBase
    {
        private readonly IUnifiedJobsService _jobService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnifiedJobsController"/> class.
        /// </summary>
        /// <param name="jobService">Service to retrieve status of async jobs.</param>
        public UnifiedJobsController(IUnifiedJobsService jobService)
        {
            ArgumentNullException.ThrowIfNull(jobService);
            _jobService = jobService;
        }

        /// <summary>
        /// List jobs based on request criteria.
        /// </summary>
        /// <param name="request">Batch Request.</param>
        /// <param name="includeDetails">Include Job Entry Details.</param>
        /// <param name="includeTotalCount"> Include Total Count in the response.</param>
        /// <returns>List of JobsEntry</returns>
        [HttpPost("listjobs", Name = "ListJobs")]
        [Authorize(Policy = AppPermissions.CanReadJobs)]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(JobsResponse))]
        [ProducesResponseType(typeof(JobsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<JobsResponse>> FindJobs(
            [FromBody] BatchRequestDto request,
            [FromQuery] bool includeDetails = false,
            [FromQuery] bool includeTotalCount = false)
        {
            var jobs = await _jobService.ListJobEntires(request, includeDetails, includeTotalCount);
            return Ok(jobs);
        }

        /// <summary>
        /// Search the JobEntries table based on JobId
        /// </summary>
        /// <param name="jobId">Job Id</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet("getjob", Name = "GetJob")]
        [Authorize(Policy = AppPermissions.CanReadJobs)]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(JobsEntry))]
        [ProducesResponseType(typeof(JobsEntry), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<JobsEntry>> GetJob([Required][FromQuery] string jobId = null)
        {
            var job = await _jobService.GetJob(jobId);
            return job != null ? Ok(job) : NotFound("Job not found");
        }

        /// <summary>
        /// Search the JobEntries table based on JobId
        /// </summary>
        /// <param name="jobId">Job Id</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet("getJobTypes", Name = "GetJobtypes")]
        [Authorize(Policy = AppPermissions.CanReadJobs)]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(IEnumerable<string>))]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<string>>> GetAllJobTypes()
        {
            return Ok(await _jobService.GetAllJobTypes());
        }

        /// <summary>
        /// Create On-Demand Job.
        /// </summary>
        /// <param name="payload">Job Payload.</param>
        /// <param name="userMessage">User Message.</param>
        /// <returns>Id of the created Job Entry.</returns>
        [HttpPost("createOnDemandJob")]
        [Authorize(Policy = AppPermissions.CanCreateOrUpdateJobs)]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> CreateOnDemandJobEntry([FromBody] JsonDocument payload, [FromHeader] string userMessage)
        {
            return await _jobService.CreateOnDemandJob(payload, userMessage);
        }

        /// <summary>
        /// Create/update JobEntry.
        /// </summary>
        /// <param name="jobEntry">JobEntry object.</param>
        /// <returns>Created/updated jobEntry object.</returns>
        [HttpPost("createorupdateJobEntry", Name = "createorupdateJobEntry")]
        [Authorize(Policy = AppPermissions.CanCreateOrUpdateJobs)]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(JobsEntry))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<JobsEntry>> CreateOrUpdateJobEntry([FromBody] JobsEntry jobEntry)
        {
            return await _jobService.CreateOrUpdateJobEntry(jobEntry);
        }

        /// <summary>
        /// Delete JobEntries in bulk.
        /// </summary>
        /// <param name="jobIds">JobIds</param>
        /// <returns>Number of JobEntry deleted.</returns>
        [HttpDelete("deleteJobEntries", Name = "deleteJobEntries")]
        [Authorize(Policy = AppPermissions.CanDeleteJobs)]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(int))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> DeleteBulkJobs([Required][FromBody] IEnumerable<string> jobIds, [FromQuery] bool hardDelete = false)
        {
            return await _jobService.DeleteBulkJobs(jobIds, hardDelete);
        }

        /// <summary>
        /// Delete older JobEntries
        /// </summary>
        /// <param name="date">date</param>
        /// <param name="jobType">jobType</param>
        /// <param name="hardDelete">hardDelete</param>
        /// <returns>Delete older jobEntries.</returns>
        [HttpDelete("deleteJobsOlderThan", Name = "deleteJobsOlderThan")]
        [Authorize(Policy = AppPermissions.CanDeleteJobs)]
        [SwaggerResponse(StatusCodes.Status200OK, typeof(int))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> DeleteOlderJobs([Required][FromQuery] DateTimeOffset date, [FromQuery] string jobType = null, [FromQuery] bool hardDelete = false)
        {
            return await _jobService.DeleteOlderJobs(date, jobType, hardDelete);
        }
    }
}
