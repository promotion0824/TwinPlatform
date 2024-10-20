using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.Mapped;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.Model.Async;
using Willow.Model.Mapping;
using Willow.AzureDigitalTwins.Api.Model.Response.Mapped;

namespace Willow.AzureDigitalTwins.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class MappingController : ControllerBase
    {
        private readonly IMappingService _mappingService;
        private readonly IMappedAsyncService _mappedAsyncService;
        private readonly ILogger<MappingController> _logger;

        public MappingController(IMappingService mappingService, IMappedAsyncService mappedAsyncService, ILogger<MappingController> logger)
        {
            _mappingService = mappingService;
            _mappedAsyncService = mappedAsyncService;
            _logger = logger;
        }

        /// <summary>
        /// Get mapped entries
        /// </summary>
        /// <returns>Get mapped entries</returns>
        [HttpPost("GetMappedEntriesAsync", Name = "GetMappedEntriesAsync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MappedEntryResponse>> GetMappedEntries(
            [FromBody] MappedEntryRequest request)
        {
            var entries = await _mappingService.GetMappedEntriesAsync(request);

            return Ok(entries);
        }

        /// <summary>
        /// Get mapped entries grouped by fieldName, and count of each group.
        /// </summary>
        /// <returns>List of records grouped by fieldName and its count</returns>
        [HttpGet("GetGroupedMappedEntriesCount", Name = "GetGroupedMappedEntriesCount")]
        public async Task<ActionResult<IEnumerable<MappedEntriesGroupCount>>> GetGroupedCount([FromQuery] string fieldName, [FromQuery] Status? status)
        {
            var result = await _mappingService.GetGroupedMappedEntriesCountAsync(fieldName, status);
            return Ok(result);
        }

        /// <summary>
        /// Get mapped entry
        /// </summary>
        /// <returns>Get mapped entry</returns>
        [HttpGet("{mappedId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MappedEntry>> GetMappedEntry([Required][FromRoute] string mappedId)
        {
            if (string.IsNullOrEmpty(mappedId))
            {
                return BadRequest("Invalid MappedId");
            }

            var entry = await _mappingService.GetMappedEntry(mappedId);
            if (entry == null)
            {
                return NotFound();
            }

            return Ok(entry);
        }

        /// <summary>
		/// Creates a mapped entry
		/// </summary>
		[HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MappedEntry>> CreateMappedEntry(CreateMappedEntry entry)
        {
            return await _mappingService.CreateMappedEntry(entry);
        }

        /// <summary>
		/// Update a mapped entry
		/// </summary>
		[HttpPut]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MappedEntry>> UpdateMappedEntry(UpdateMappedEntry entry)
        {
            var entity = await _mappingService.UpdateMappedEntry(entry);
            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        /// <summary>
		/// Update mapped entries status to specified status.
        /// Any bad mapped ids will be ignored. Return total number of mapped entries updated.
		/// </summary>
		[HttpPut("status")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> UpdateMappedEntryStatus(UpdateMappedEntryStatusRequest request)
        {
            return await _mappingService.UpdateMappedEntryStatus(request);
        }

        /// <summary>
        /// Update all mapped entries status to specified status based on MappedEntryAllRequest
        /// </summary>
        [HttpPut("updateAllstatus")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> UpdateAllMappedEntryStatus(MappedEntryAllRequest request, Status status)
        {
            return await _mappingService.UpdateAllMappedEntryStatus(request, status);
        }

        /// <summary>
		/// Delete a mapped entry
		/// </summary>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{mappedId}")]
        public async Task<ActionResult> DeleteMappedEntry([Required][FromRoute] string mappedId)
        {
            if (string.IsNullOrEmpty(mappedId))
            {
                return BadRequest("Invalid MappedId");
            }

            var entry = await _mappingService.GetMappedEntry(mappedId);
            if (entry == null)
            {
                return NotFound();
            }

            await _mappingService.DeleteMappedEntry(entry);

            return NoContent();
        }

        /// <summary>
		/// Delete mapped entries with the input mapped ids.
        /// Ignored bad or not found mapped ids.
		/// </summary>
        /// <returns>Total number of mapped entries deleted.</returns>
        /// <response code="404">No mapped ids input</response>
        /// <response code="200">Total deleted items</response>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpDelete("deleteBulk")]
        public async Task<ActionResult<int>> DeleteMappedEntries([Required][FromBody] IEnumerable<string> mappedIds)
        {
            if (mappedIds?.Any() != true)
            {
                return BadRequest("No mapped ids in input request");
            }

            return await _mappingService.DeleteBulk(mappedIds);
        }

        /// <summary>
        /// Delete all mapped entries based on MappedEntryDeleteAllRequest.
        /// </summary>
        /// <param name="request">Filter records based on request</param>
        /// <returns>Total number of mapped entries deleted.</returns>
        /// <response code="404">Confirm should be set to true</response>
        /// <response code="200">Total deleted items</response>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpDelete("deleteAll")]
        public async Task<ActionResult<int>> DeleteAllMappedEntries(MappedEntryAllRequest request)
        {
            return await _mappingService.DeleteAll(request);
        }

        /// <summary>
        /// Get the count of mapped entries.
        /// <param name="statuses">query based on mapped entries' status</param>
        /// <param name="prefixToMatchId">Prefixes to match with the first few characters of Mapped Id</param>
        /// <param name="excludePrefixes">Exclude records where prefixes match with the first few characters of Mapped Id</param>
        /// <returns>Get mapped entries</returns>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetMappedEntriesCount(
            [FromQuery] IEnumerable<Status> statuses,
            [FromQuery] string[]? prefixToMatchId = null,
            [FromQuery] bool? excludePrefixes = false)
        {
            var result = await _mappingService.GetMappedEntriesCountAsync(statuses, prefixToMatchId, excludePrefixes);

            return Ok(result);
        }

        /// <summary>
        /// Get Update twin requests
        /// </summary>
        /// <param name="offset">Used to identify the starting point to return records.</param>
        /// <param name="pageSize">Amount of records to fetch for each requests.</param>
        /// <returns>List of update twin requests</returns>
        [HttpGet("GetUpdateTwinRequests")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IAsyncEnumerable<UpdateMappedTwinRequestResponse>>> GetUpdateTwinRequests(
            [FromQuery] int offset = 0,
            [FromQuery] int pageSize = 100)
        {
            var entries = await _mappingService.GetUpdateTwinRequestsAsync(offset, pageSize);

            return Ok(entries);
        }

        /// <summary>
        /// Get the count of update twin requests.
        /// </summary>
        [HttpGet("updateTwinRequestsCount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetUpdateTwinRequestsCount()
        {
            var result = await _mappingService.GetUpdateTwinRequestsCountAsync();

            return Ok(result);
        }

        /// <summary>
        /// Creates update twin request
        /// </summary>
        /// <param name="jsonPatch">List of json patch operations.</param>
        /// <param name="willowTwinId">Willow twin id</param>
        [HttpPost("CreateUpdateTwinRequests")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<UpdateMappedTwinRequest>> CreateUpdateTwinRequest(
            [Required][FromQuery] string willowTwinId,
            [Required][FromBody] List<JsonPatchOperation> jsonPatch)
        {
            return await _mappingService.CreateUpdateTwinRequest(willowTwinId, jsonPatch);
        }

        /// <summary>
        /// Update a update twin request
        /// </summary>
        /// <param name="jsonPatch">List of json patch operations.</param>
        [HttpPut("UpdateTwinUpdateRequest")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<UpdateMappedTwinRequest>> UpdateTwinUpdateRequest(
            [Required][FromQuery] Guid id,
            [Required][FromBody] List<JsonPatchOperation> jsonPatch)
        {
            var entity = await _mappingService.UpdateTwinUpdateRequest(id, jsonPatch);
            if (entity == null)
            {
                return NotFound();
            }

            return Ok(entity);
        }

        /// <summary>
        /// Upsert update twin request. If record with willowTwinId exists, update it. Otherwise, create a new record.
        /// </summary>
        /// <param name="willowTwinId">willowTwinId</param>
        /// <param name="jsonPatch">json patch</param>
        /// <returns> twin update request</returns>
        [HttpPut("UpsertUpdateTwinRequest")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<UpdateMappedTwinRequest>> UpsertUpdateTwinRequest(
            [Required][FromQuery] string willowTwinId,
            [Required][FromBody] List<JsonPatchOperation> jsonPatch)
        {
            var entity = await _mappingService.UpsertUpdateTwinRequest(willowTwinId, jsonPatch);
            return Ok(entity);
        }

        /// <summary>
        /// Delete update twin requests based on ids.
        /// Ignored bad or not found ids.
        /// </summary>
        /// <param name="ids">List of updated twin request id</param>
        /// <returns>Total number of twin update requests deleted.</returns>
        /// <response code="404">No ids input</response>
        /// <response code="200">Total deleted items</response>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpDelete("deleteBulkUpdateTwinRequests")]
        public async Task<ActionResult<int>> DeleteUpdateTwinRequests([Required][FromBody] IEnumerable<Guid> ids)
        {
            if (ids?.Any() != true)
            {
                return BadRequest("No ids in input request");
            }

            return await _mappingService.DeleteBulkUpdateTwinRequests(ids);
        }

        /// <summary>
        /// Delete all twin update requests
        /// </summary>
        /// <returns>Total number of mapped entries deleted.</returns>
        /// <response code="404">Confirm should be set to true</response>
        /// <response code="200">Total deleted items</response>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpDelete("deleteAllUpdateTwinRequests")]
        public async Task<ActionResult<int>> DeleteAllUpdateTwinRequests()
        {
            return await _mappingService.DeleteAllUpdateTwinRequests();
        }


        /// <summary>
        /// Create a MTI async job and store in storage account.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("CreateMtiAsyncJob")]
        public async Task<ActionResult<MtiAsyncJob>> CreateMtiAsyncJob([Required][FromBody] MtiAsyncJobRequest request)
        {
            var mtiJob = await _mappedAsyncService.CreateMtiAsyncJob(request);

            return mtiJob;
        }

        /// <summary>
        /// Update  MTI async job's status and last updated time.
        /// </summary>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPost("UpdateMtiAsyncJobStatus")]
        public async Task<ActionResult<MtiAsyncJob>> UpdateMtiAsyncJobStatus(MtiAsyncJob job, AsyncJobStatus status)
        {
            var ret = await _mappedAsyncService.UpdateMtiAsyncJobStatus(job, status);

            if (ret == null)
            {
                return NotFound();
            }

            return ret;
        }

        /// <summary>
        /// Search for MTI async jobs
        /// </summary>
        /// <param name="jobId">Filter by MTI async job's id</param>
        /// <param name="status">Filter by status</param>
        /// <returns></returns>
        [HttpGet("FindMtiAsyncJobs")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<MtiAsyncJob>>> FindMtiAsyncJobs(
            [FromQuery] string jobId,
            [FromQuery] AsyncJobStatus? status = null)
        {
            _logger.LogInformation($"FindMtiAsyncJobs: jobId={jobId}, status={status}");
            var jobs = await _mappedAsyncService.FindMtiAsyncJobs(jobId, status);

            var ret = jobs.ToList();
            return ret;
        }

        /// <summary>
        /// Get latest MTI async jobs
        /// </summary>
        /// <param name="status">Filter by status</param>
        [HttpGet("getLatestMtiAsyncJob")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<MtiAsyncJob>> getLatestMtiAsyncJob(
        [FromQuery] AsyncJobStatus? status = null)
        {
            var job = await _mappedAsyncService.GetLatestMtiAsyncJob(status);

            // If no job found, return a job with status Done
            if (job is null)
            {
                return new MtiAsyncJob { Details = new AsyncJobDetails { Status = AsyncJobStatus.Done } };
            }

            return job;
        }
    }
}
