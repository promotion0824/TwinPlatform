using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Models;
using Willow.TwinLifecycleManagement.Web.Models.Mapped;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Mapping Service Controller.
/// </summary>
/// <param name="mappingService">Implementation of Mapping Service.</param>
[ApiController]
[Route("api/[controller]")]
public class MappingController(IMappingService mappingService) : ControllerBase
{
    /// <summary>
    /// Get mapped entries.
    /// </summary>
    /// <param name="request">Mapped Entry Request.</param>
    /// <returns>Mapped Entry Response.</returns>
    [HttpPost("getMappedEntries", Name = "getMappedEntries")]
    [Authorize(Policy = AppPermissions.CanReadMappings)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<MappedEntryResponse>>))]
    public async Task<ActionResult<MappedEntryResponse>> GetMappedEntries(
        [FromBody] MappedEntryRequest request)
    {
        var ret = await mappingService.GetMappedEntriesAsync(request);

        return Ok(ret);
    }

    /// <summary>
    /// Get list of mapped entries grouped by Building Id and Connector Id and its count. Used for filter dropdowns.
    /// </summary>
    [HttpGet("filterDropdown", Name = "getFilterDropdown")]
    [Authorize(Policy = AppPermissions.CanReadMappings)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<CombinedMappedEntriesGroupCount>>))]
    public async Task<ActionResult<CombinedMappedEntriesGroupCount>> GetFilterDropdown()
    {
        var ret = await mappingService.GetCombinedMappedEntriesGroupCountAsync();

        return Ok(ret);
    }


    /// <summary>
    /// Create mapped entry.
    /// </summary>
    /// <param name="entry">Mapped entry object.</param>
    /// <returns>Created Mapped entry object.</returns>
    [HttpPost(Name = "createMappedEntry")]
    [Authorize(Policy = AppPermissions.CanReadMappings)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<MappedEntry>> CreateMappedEntry(CreateMappedEntry entry)
    {
        return await mappingService.CreateMappedEntry(entry);
    }

    /// <summary>
    /// Update a mapped entry.
    /// </summary>
    /// <param name="entry">Mapped entry object.</param>
    /// <returns>Updated Mapped entry object.</returns>
    [HttpPut(Name = "putMappedEntry")]
    [Authorize(Policy = AppPermissions.CanReadMappings)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<MappedEntry>> UpdateMappedEntry(UpdateMappedEntry entry)
    {
        var entity = await mappingService.UpdateMappedEntry(entry);
        if (entity == null)
        {
            return NotFound();
        }

        return Ok(entity);
    }

    /// <summary>
    /// Get the count of mapped entries.
    /// </summary>
    /// <param name="statuses">query based on mapped entries' status.</param>
    /// <param name="prefixToMatchId">Prefixes to match with the first few characters of Mapped Id.</param>
    /// <param name="excludePrefixes">Exclude records where prefixes match with the first few characters of Mapped Id.</param>
    /// <returns>Return counts based on query.</returns>
    [HttpGet("count", Name = "getMappedEntriesCount")]
    [Authorize(Policy = AppPermissions.CanReadMappings)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<int>>))]
    public async Task<ActionResult<int>> GetMappedEntriesCount(
        [FromQuery] IEnumerable<Status> statuses = default,
        [FromQuery] string[] prefixToMatchId = null,
        [FromQuery] bool? excludePrefixes = false)
    {
        int count = await mappingService.GetMappedEntriesCountAsync(statuses, prefixToMatchId, excludePrefixes);

        return Ok(count);
    }

    /// <summary>
    /// Update mapped entries status to specified status
    /// Any bad mapped ids will be ignored. Return total number of mapped entries updated.
    /// </summary>
    /// <param name="mappedIds">the ids of MappedEntries that will be changed.</param>
    /// <param name="status">The Status MappedEntries will be changed to.</param>
    /// <returns>Number of updates.</returns>
    [HttpPut("changeMappedEntriesStatus")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> UpdateMappedEntryStatus(
        [FromBody] string[] mappedIds,
        [FromQuery] Status status)
    {
        var request = new UpdateMappedEntryStatusRequest();
        request.MappedIds = mappedIds;
        request.Status = status;
        return await mappingService.UpdateMappedEntryStatus(request);
    }

    /// <summary>
    /// Update all mapped entries status to specified status based on MappedEntryAllRequest
    /// </summary>
    [HttpPut("updateAllstatus")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> UpdateMappedEntryStatus(MappedEntryAllRequest request, Status status)
    {
        return await mappingService.UpdateAllMappedEntryStatus(request, status);
    }

    /// <summary>
    /// Delete mapped entries with the input mapped ids.
    /// Ignored bad or not found mapped ids.
    /// </summary>
    /// <param name="mappedIds">the ids of MappedEntries that to be delete.</param>
    /// <returns>Total number of mapped entries deleted.</returns>
    /// <response code="404">No mapped ids input.</response>
    /// <response code="200">Total deleted items.</response>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<int>>))]
    [HttpDelete("deleteBulk")]
    public async Task<ActionResult<int>> DeleteMappedEntries([Required][FromBody] IEnumerable<string> mappedIds)
    {
        if (mappedIds?.Any() != true)
        {
            return BadRequest("No mapped ids in input request");
        }

        return await mappingService.DeleteBulk(mappedIds);
    }

    /// <summary>
    /// Delete mapped entries based on MappedEntryAllRequest.
    /// </summary>
    /// <param name="request">Filter records based on MappedEntryAllRequest to delete.</param>
    /// <returns>Total number of mapped entries deleted.</returns>
    /// <response code="404">Confirm should be set to true.</response>
    /// <response code="200">Total deleted items.</response>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<int>>))]
    [HttpDelete("deleteAll")]
    public async Task<ActionResult<int>> DeleteAllMappedEntries(MappedEntryAllRequest request)
    {
        return await mappingService.DeleteAll(request);
    }

    /// <summary>
    /// Get Update twin requests.
    /// </summary>
    /// <param name="offset">Used to identify the starting point to return rows.</param>
    /// <param name="pageSize">Amount of records to fetch for each requests.</param>
    /// <returns>List of update twin requests.</returns>
    [HttpGet("getUpdateTwinRequests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Policy = AppPermissions.CanReadMappings)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<IEnumerable<UpdateMappedTwinRequestResponse>>>))]
    public async Task<ActionResult<IAsyncEnumerable<UpdateMappedTwinRequestResponse>>> GetUpdateTwinRequests(
        [FromQuery] int offset = 0,
        [FromQuery] int pageSize = 100)
    {
        var entries = await mappingService.GetUpdateTwinRequestsAsync(offset, pageSize);

        return Ok(entries);
    }

    /// <summary>
    /// Creates update twin request.
    /// </summary>
    /// <param name="willowTwinId">Id.</param>
    /// <param name="jsonPatch">Json Patch Operation.</param>
    /// <returns><see cref="UpdateMappedTwinRequest"/>.</returns>
    [HttpPost("createUpdateTwinRequests")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateMappedTwinRequest>> CreateUpdateTwinRequest(
        [Required][FromQuery] string willowTwinId,
        [Required][FromBody] List<JsonPatchOperation> jsonPatch)
    {
        return await mappingService.CreateUpdateTwinRequest(willowTwinId, jsonPatch);
    }

    /// <summary>
    /// Update a update twin request.
    /// </summary>
    /// <param name="id">Id.</param>
    /// <param name="jsonPatch">Json Patch Operation.</param>
    /// <returns><see cref="UpdateMappedTwinRequest"/>.</returns>
    [HttpPut("UpdateTwinUpdateRequest")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateMappedTwinRequest>> UpdateTwinUpdateRequest(
        [Required][FromQuery] Guid id,
        [Required][FromBody] List<JsonPatchOperation> jsonPatch)
    {
        var entity = await mappingService.UpdateTwinUpdateRequest(id, jsonPatch);
        if (entity == null)
        {
            return NotFound();
        }

        return Ok(entity);
    }

    /// <summary>
    /// Upsert update twin request. If record with willowTwinId exists, update it. Otherwise, create a new record.
    /// </summary>
    /// <param name="willowTwinId">willowTwinId.</param>
    /// <param name="jsonPatch">json patch.</param>
    /// <returns> twin update request.</returns>
    [HttpPut("UpsertUpdateTwinRequest")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<UpdateMappedTwinRequest>> UpsertUpdateTwinRequest(
        [Required][FromQuery] string willowTwinId,
        [Required][FromBody] List<JsonPatchOperation> jsonPatch)
    {
        var entity = await mappingService.UpsertUpdateTwinRequest(willowTwinId, jsonPatch);
        return Ok(entity);
    }

    /// <summary>
    /// Get the count of update twin requests.
    /// </summary>
    /// <returns>Return counts based on query.</returns>
    [HttpGet("UpdateTwinRequestsCount", Name = "getUpdateTwinRequestsCount")]
    [Authorize(Policy = AppPermissions.CanReadMappings)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<int>>))]
    public async Task<ActionResult<int>> GetUpdateTwinRequestsCount()
    {
        int count = await mappingService.GetUpdateTwinRequestsCountAsync();

        return Ok(count);
    }

    /// <summary>
    /// Delete update twin requests based on ids.
    /// Ignored bad or not found ids.
    /// </summary>
    /// <param name="ids">Array of Ids.</param>
    /// <returns>Total number of twin update requests deleted.</returns>
    /// <response code="404">No ids input.</response>
    /// <response code="200">Total deleted items.</response>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete(Name = "deleteUpdateTwinRequests")]
    public async Task<ActionResult<int>> DeleteUpdateTwinRequests([Required][FromBody] IEnumerable<Guid> ids)
    {
        if (ids?.Any() != true)
        {
            return BadRequest("No ids in input request");
        }

        return await mappingService.DeleteBulkUpdateTwinRequests(ids);
    }

    /// <summary>
    /// Delete all twin update requests.
    /// </summary>
    /// <returns>Total number of mapped entries deleted.</returns>
    /// <response code="404">Confirm should be set to true.</response>
    /// <response code="200">Total deleted items.</response>
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [HttpDelete("deleteAllUpdateTwinRequests")]
    public async Task<ActionResult<int>> DeleteAllUpdateTwinRequests()
    {
        return await mappingService.DeleteAllUpdateTwinRequests();
    }
}
