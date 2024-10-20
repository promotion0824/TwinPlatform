using System.ComponentModel.DataAnnotations;
using System.Net;
using Azure.DigitalTwins.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Willow.Exceptions;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.Model.Responses;
using Willow.TwinLifecycleManagement.Web.Auth;
using Willow.TwinLifecycleManagement.Web.Models;
using Willow.TwinLifecycleManagement.Web.Services;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Twins Controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TwinsController : ControllerBase
{
    private readonly ITwinsService _twinsService;
    private readonly IDataQualityService _dataQualityService;
    private readonly IMappingService _mappingService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwinsController"/> class.
    /// </summary>
    /// <param name="twinsService">Implementation of ITwinsService.</param>
    /// <param name="dataQualityService">Implementation of IDataQualityService.</param>
    public TwinsController(ITwinsService twinsService, IDataQualityService dataQualityService, IMappingService mappingService)
    {
        ArgumentNullException.ThrowIfNull(twinsService);
        ArgumentNullException.ThrowIfNull(dataQualityService);
        ArgumentNullException.ThrowIfNull(mappingService);
        _twinsService = twinsService;
        _dataQualityService = dataQualityService;
        _mappingService = mappingService;
    }

    /// <summary>
    /// Returns a list of twins.
    /// </summary>
    /// <param name="modelIds">Model ids.</param>
    /// <param name="locationId">Location Id.</param>
    /// <param name="exactModelMatch">Indicates if model filter must be exact match.</param>
    /// <param name="includeRelationships">Include outgoing relationships in response.</param>
    /// <param name="includeIncomingRelationships">Include incoming relationships in response.</param>
    /// <param name="searchString">Search String.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="continuationToken">Continuation token.</param>
    /// <param name="sourceType">Source Type.</param>
    /// <param name="includeTotalCount">When querying by ADT, return the total count of items that match the filter criteria along with the first page of items. Using this flag for ADT has the same cost as issuing an additional call to GetTwinCount with the same filter parameters, minus the extra REST call from your application.</param>
    /// <returns>Matching twins with relationships.</returns>
    /// <response code="200">Target twins retrieved.</response>
    /// <response code="400">Bad Request. LocationId param cannot be empty when relationshipToTraverse param is specified.</response>
    [HttpGet(Name = "getTwins")]
    [Authorize(Policy = AppPermissions.CanReadTwins)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<Willow.Model.Adt.Page<TwinWithRelationships>>>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Willow.Model.Adt.Page<TwinWithRelationships>>> GetTwinsWithRelationshipsAsync(
    [FromQuery] string[] modelIds = null,
    [FromQuery] string locationId = null,
    [FromQuery] bool exactModelMatch = false,
    [FromQuery] bool includeRelationships = false,
    [FromQuery] bool includeIncomingRelationships = false,
    [FromQuery] string searchString = null,
    [FromQuery] int pageSize = 100,
    [FromHeader] string continuationToken = null,
    [FromQuery] SourceType sourceType = SourceType.Adx,
    [FromQuery] bool includeTotalCount = false)
    {
        var response = await _twinsService.GetTwinsWithRelationshipsAsync(
                                                                 locationId,
                                                                 modelIds,
                                                                 exactModelMatch,
                                                                 pageSize,
                                                                 searchString,
                                                                 continuationToken,
                                                                 includeRelationships,
                                                                 includeIncomingRelationships,
                                                                 sourceType,
                                                                 null,
                                                                 null,
                                                                 includeTotalCount);
        return Ok(response);
    }

    /// <summary>
    /// Query for twins with relationships.
    /// </summary>
    /// <param name="request">Query twins criteria.</param>
    /// <remarks>
    /// Sample request
    ///
    /// 		POST
    /// 		{
    /// 			"request": {
    /// 		        "modelIds": ["dtmi:com:willowinc:AirHandlingUnit;1"],
    /// 		        "locationId": "53d380c2-d31a-4cd1-8958-795407407a82",
    /// 		        "exactModelMatch":true,
    /// 		        "includeRelationships": true,
    /// 		        "includeIncomingRelationships": true,
    /// 		        "sourceType": "AdtQuery",
    /// 		        "relationshipsToTraverse":[],
    /// 		        "searchString": "AHU",
    /// 		        "startTime": "2023-04-15T20:37:21.0274638Z",
    /// 		        "endTime": "2023-04-25T20:37:21.0274638Z"
    /// 			}
    /// 		}.
    /// </remarks>
    /// <param name="pageSize">Page size.</param>
    /// <param name="continuationToken">Continuation token.</param>
    /// <param name="includeTotalCount">When querying by ADT, return the total count of items that match the filter criteria along with the first page of items. Using this flag for ADT has the same cost as issuing an additional call to GetTwinCount with the same filter parameters, minus the extra REST call from your application.</param>
    /// <returns>Matching twins with relationships.</returns>
    /// <response code="200">Target twins retrieved.</response>
    /// <response code="400">Bad Request. LocationId param cannot be empty when relationshipToTraverse param is specified.</response>
    [HttpPost(Name = "QueryTwins")]
    [Authorize(Policy = AppPermissions.CanReadTwins)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<Willow.Model.Adt.Page<TwinWithRelationships>>>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Page<TwinWithRelationships>>> QueryTwinsWithRelationshipsAsync(
        [FromBody][Required] GetTwinsInfoRequestBFF request,
        [FromQuery] int pageSize = 100,
        [FromHeader] string continuationToken = null,
        [FromQuery] bool includeTotalCount = false)
    {
        var response = await _twinsService.QueryTwinsWithRelationshipsAsync(request, pageSize, continuationToken, includeTotalCount);
        return Ok(response);
    }

    /// <summary>
    /// Get Twin By Id.
    /// </summary>
    /// <param name="id">Id of the Twin.</param>
    /// <param name="sourceType">Source Type.</param>
    /// <param name="includeRelationships">True if include relationships;else false.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet("{id}", Name = "GetTwinById")]
    [Authorize(Policy = AppPermissions.CanReadTwins)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<TwinWithRelationships>>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<TwinWithRelationships>> GetTwinByIdAsync(
        [FromRoute][Required(AllowEmptyStrings = false)] string id,
        [FromQuery] SourceType sourceType = SourceType.Adx,
        [FromQuery] bool includeRelationships = false)
    {
        var results = await _twinsService.GetTwinAsync(id, sourceType, includeRelationships);

        return Ok(results);
    }

    /// <summary>
    /// Get Twins By Ids.
    /// </summary>
    /// <param name="ids">Array of Twin Id.</param>
    /// <param name="sourceType">Source Type to retrieve the twin.</param>
    /// <param name="includeRelationships">True to include relationships; else false.</param>
    /// <returns><see cref="TwinWithRelationships"/>.</returns>
    [HttpPost("ids", Name = "GetTwinByIds")]
    [Authorize(Policy = AppPermissions.CanReadTwins)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<TwinWithRelationships>>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<TwinWithRelationships>> GetTwinByIdsAsync(
                [FromBody][Required(AllowEmptyStrings = false)] string[] ids,
                [FromQuery] SourceType sourceType = SourceType.AdtQuery,
                [FromQuery] bool includeRelationships = false)
    {
        var results = await _twinsService.GetTwinByIdsAsync(ids, sourceType, includeRelationships);

        return Ok(results);
    }

    /// <summary>
    /// Returns a tree of twins which are related.
    /// </summary>
    /// <param name="modelIds">Model ids.</param>
    /// <param name="outgoingRelationships">List of relationship types to be considered for traversal. E.g. ["isPartOf", "locatedIn"].</param>
    /// <param name="incomingRelationships">List of relationship types to be considered for traversal.</param>
    /// <param name="exactModelMatch">Indicates if model filter must be exact match.</param>
    /// <returns>Matching twins with relationships.</returns>
    /// <response code="200">Target twins retrieved.</response>
    /// <response code="400">Bad Request.</response>
    [HttpGet("GetTwinsTree")]
    [Authorize(Policy = AppPermissions.CanReadTwins)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<IEnumerable<NestedTwin>>>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<NestedTwin>>> GetTwinsTreeAsync(
        [FromQuery][Required] string[] modelIds = null,
        [FromQuery] string[] outgoingRelationships = null,
        [FromQuery] string[] incomingRelationships = null,
        [FromQuery] bool exactModelMatch = false)
    {
        var response = await _twinsService.GetTwinsTreeAsync(modelIds, outgoingRelationships, incomingRelationships, exactModelMatch);
        return Ok(response);
    }

    /// <summary>
    /// Delete Twins.
    /// </summary>
    /// <param name="request">Array of Twins Ids.</param>
    /// <param name="deleteRelationships">True to delete relationships; else false.</param>
    /// <returns>Multiple Entity Response.</returns>
    [HttpDelete("deleteTwins", Name = "deleteTwinsIds")]
    [Authorize(Policy = $"{AppPermissions.CanDeleteTwins}")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(MultipleEntityResponse))]
    [ProducesResponseType(typeof(MultipleEntityResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTwins(
        [FromBody] DeleteTwinsRequest request,
        [FromQuery] bool deleteRelationships)
    {
        var results = await _twinsService.DeleteTwins(request.twinIDs, deleteRelationships);

        // Delete Mapping entry.
        try
        {
            if (request.externalIDs.Length > 0)
            {
                await _mappingService.DeleteBulk(request.externalIDs);
            }
        }
        catch (Exception ex)
        {
            results.Responses.Add(new AzureDigitalTwins.SDK.Client.EntityResponse
            {
                EntityId = "Deleting Mapping entries failed",
                StatusCode = (AzureDigitalTwins.SDK.Client.HttpStatusCode)HttpStatusCode.FailedDependency,
                Message = ex.Message
            });
        }

        return Ok(results);
    }

    /// <summary>
    /// Count of Twins in ADX/ADT matching the criteria.
    /// Sample request: https://localhost:8001/twins/count?exactModelMatch=true&modelId=dtmi:com:willowinc:AirHandlingUnit;1&searchString=ahu.
    /// </summary>
    /// <param name="modelIds">Model ids.</param>
    /// <param name="locationId">Location Id.</param>
    /// <param name="exactModelMatch">Indicates if model filter must be exact match.</param>
    /// <param name="sourceType">Indicates search type, ADX / Adt query / Adt in memory. Supported Values:[Adx,Adtquery,AdtMemory].</param>
    /// <param name="searchString">string to search for in twin $dtid or Name.</param>
    /// <param name="isIncrementalScan">Get count of twins that will be checked during data quality scan.</param>
    /// <returns>Number of Twins.</returns>
    [HttpGet("count", Name = "getTwinsCount")]
    [Authorize(Policy = AppPermissions.CanReadTwins)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(int))]
    public async Task<ActionResult<int>> GetTwinsCount(
        [FromQuery] string[] modelIds = null,
        [FromQuery] string locationId = null,
        [FromQuery] bool exactModelMatch = false,
        [FromQuery] SourceType sourceType = SourceType.Adx,
        [FromQuery] string searchString = null,
        [FromQuery] bool isIncrementalScan = false)
    {
        DateTimeOffset? startTime = null;
        DateTimeOffset? endTime = null;

        if (isIncrementalScan)
        {
            var latestSuccessfulDQValidationJob = await _dataQualityService.GetLatestDQValidationJob(status: AsyncJobStatus.Done);
            startTime = latestSuccessfulDQValidationJob.EndTime ?? new DateTimeOffset(DateTime.MinValue);
            endTime = DateTimeOffset.Now;
        }

        var digitalTwins = await _twinsService.GetTwinsCount(
                                   locationId,
                                   modelIds,
                                   exactModelMatch,
                                   sourceType,
                                   searchString,
                                   startTime,
                                   endTime);

        return Ok(digitalTwins);
    }

    /// <summary>
    /// Patch a twin.
    /// </summary>
    /// <param name="id">Twin id.</param>
    /// <param name="jsonPatch">Patch information.</param>
    /// <param name="includeAdxUpdate">True if IncludeADXUpdate; else false.</param>
    /// <returns>Ok Object Result.</returns>
    /// <remarks>
    /// 	Sample request
    ///
    /// 		PATCH
    /// 		[
    /// 			{
    /// 				"op":"replace",
    /// 				"path":"/customproperties/description",
    /// 				"value":"Patched description yes 8"
    /// 			}
    /// 		].
    /// </remarks>
    /// <response code="404">Twin not found.</response>
    /// <response code="200">Twin patched.</response>
    [HttpPatch("{id}", Name = "patchTwin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> PatchTwin(
        [FromRoute][Required(AllowEmptyStrings = false)] string id,
        [FromBody][Required] IEnumerable<Operation> jsonPatch,
        [FromQuery] bool includeAdxUpdate)
    {
        await _twinsService.PatchTwin(id, jsonPatch, includeAdxUpdate);

        return Ok();
    }

    /// <summary>
    /// Put Twin.
    /// </summary>
    /// <param name="twin">Basic Digital Twin.</param>
    /// <returns>Updated Basic Digital Twin.</returns>
    [HttpPut(Name = "putTwin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BasicDigitalTwin>> PutTwin(
        [FromBody][Required] BasicDigitalTwin twin)
    {
        var results = await _twinsService.PutTwin(twin);
        return Ok(results);
    }

    /// <summary>
    /// Returns a list of twins.
    /// </summary>
    /// <param name="modelIds">Model ids.</param>
    /// <param name="sourceType">Source Type.</param>
    /// <returns>TwinsWithRelationships.</returns>
    /// <response code="200">Target twins retrieved.</response>
    [HttpGet("getAllTwins", Name = "getAllTwins")]
    [Authorize(Policy = AppPermissions.CanReadTwins)]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(Task<ActionResult<IEnumerable<TwinWithRelationships>>>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status424FailedDependency)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<TwinWithRelationships>>> GetAllTwinsAsync(
        [FromQuery] string[] modelIds = null,
        [FromQuery] SourceType sourceType = SourceType.AdtQuery)
    {
        var response = await _twinsService.GetAllTwinsAsync(modelIds: modelIds, sourceType: sourceType);
        return Ok(response);
    }
}
