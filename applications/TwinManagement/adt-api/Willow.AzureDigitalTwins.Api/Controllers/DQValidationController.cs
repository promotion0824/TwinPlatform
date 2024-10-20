using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.DataQuality;
using Willow.DataQuality.Model.Validation;
using Willow.Model.Adt;
using Willow.Model.Async;

namespace Willow.AzureDigitalTwins.DataQuality.Api.Controllers;

/// <summary>
/// Data Quality controller of validation results
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class DQValidationController : Controller
{
    private readonly IDataQualityAdxService _dataQualityAdxService;

    public DQValidationController(IDataQualityAdxService dataQualityAdxService)
    {
        _dataQualityAdxService = dataQualityAdxService;
    }


    /// Sample request:
    ///
    ///     POST validation results:
    ///		[
    ///			{
    ///				"TwinDtId": "BPY-XX1",
    ///				"TwinIdentifiers": {},
    ///				"ModelId": "dtmi:com:willowinc:Actuator;1",
    ///				"ResultSource": "StaticDataQuality",
    ///				"Description": "",
    ///				"ResultType": "OK",
    ///				"ResultInfo": {},
    ///				"RuleScope": {},
    ///				"RuleId": "PropertyCheck",
    ///				"RunInfo": {},
    ///				"TwinInfo": {},
    ///				"Score": 100
    ///			}
    ///		]
    /// <summary>
    /// Write Validation results to ADX
    /// </summary>
    /// <response code="201">Returns when results are written to ADX</response>
    /// <response code="400">If no status provided</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<ActionResult> CreateValidationResults([FromBody][Required] IEnumerable<ValidationResults> validationData)
    {
        if (!validationData.Any())
            return Ok();

        await _dataQualityAdxService.IngestDataToValidationTableAsync(validationData);
        // TODO: Do we want to map this to a separate ValidationResultsResponse DTO?
        return Ok();
    }

    /// <summary>
    /// Get validation results by Twin DtId
    /// </summary>
    /// <remarks>Sample request: https://localhost:8001/dqvalidation/id?dtid=MSFT-XX1&dtid=AY-XX1
    /// There may be more than one result returned for a given twin, depending on rule definitions.
    /// If a twin ID is not found in a list of twins, no results will be returned for that twin and it is not considered an error.
    /// </remarks>
    /// <param name="dtId">Twin DtId</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="continuationToken">Continuation Token</param>
    /// <returns>Twin Validation information</returns>
    /// <response code="400">Bad Request</response>
    /// <response code="200">Twin Validation information</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Page<ValidationResults>>> GetTwinDataQualityResults(
        [FromQuery][Required(AllowEmptyStrings = false)] string[] dtId,
        [FromQuery] int pagesize = 100,
        [FromHeader] string continuationtoken = null)
    {
        var result = await _dataQualityAdxService.GetTwinDataQualityResultsByIdAsync(dtId, idsAreModels: false, pageSize: pagesize, continuationToken: continuationtoken);
        return result;
    }

    /// <summary>
    /// Get validation results by ModelId
    /// </summary>
    /// <remarks>Sample request: https://localhost:8001/dqvalidation/modelid?resultSources=StaticDataQuality&modelIds=dtmi:com:willowinc:FanCoilUnit;1&resultSources=RulesEngineCapabilityStatus&modelIds=dtmi:com:willowinc:AirHandlingUnit;1
    /// There may be more than one result returned for a given twin, depending on rule definitions.
    /// If a twin ID is not found in a list of twins, no results will be returned for that twin and it is not considered an error.
    /// </remarks>
    /// <param name="modelIds">One or more Model Ids of Twins</param>
    /// <param name="resultSources">One or more result source; current sources are: RulesEngineCapabilityStatus, StaticDataQuality</param>
    /// <param name="resultTypes">One or more result types; Ok / Error</param>
    /// <param name="checkTypes"> Type of data quality validation check: DataQualityRule, Properties, Relationships, Telemetry</param>
    /// <param name="startDate"> State date to filter data quality results</param>
    /// <param name="endDate">End date to filter data quality results</param>
    /// <param name="searchString">Search string in Twin DtId or Name or any ResultInfo fields </param>
    /// <param name="locationId">TwinInfo / Location/SiteId, SiteDtId, SiteName, FloorId, FloorDtId, FloorName value</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="continuationToken">Continuation Token</param>
    /// <returns>Twin Validation information</returns>
    /// <response code="400">Bad Request</response>
    /// <response code="200">Twin Validation information</response>
    [HttpGet("modelid")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Page<ValidationResults>>> GetTwinDataQualityResultsByModelIds(
        [FromQuery] string[] modelIds,
        [FromQuery] string[] resultSources,
        [FromQuery] Result[] resultTypes,
        [FromQuery] CheckType[] checkTypes,
        [FromQuery] DateTimeOffset? startDate,
        [FromQuery] DateTimeOffset? endDate,
        [FromQuery] string searchString = null,
        [FromQuery] string locationId = null,
        [FromQuery] int pageSize = 100,
        [FromHeader] string continuationToken = null)
    {
        var result = await _dataQualityAdxService.GetTwinDataQualityResultsByIdAsync(modelIds, idsAreModels: true, resultSources, resultTypes, checkTypes, startDate, endDate, searchString, locationId, pageSize, continuationToken);
        return result;
    }

    /// <summary>
    /// Triggers an async twins validation job
    /// </summary>
    /// <param name="modelIds">Model ids</param>
    /// <param name="locationId">Location Id</param>
    /// <param name="exactModelMatch">Indicates if model filter must be exact match</param>
    /// <param name="startCheckTime">Starting Export time of the Twins</param>
    /// <param name="endCheckTime">Ending Export time of the Twin</param>
    /// <param name="userId">User Id</param>
    /// <returns>Twins validation job created</returns>
    [HttpGet("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<TwinsValidationJob>> TriggerTwinsValidation(
        [FromHeader(Name = "User-Id")][Required] string userId,
        [FromQuery] string[] modelIds = null,
        [FromQuery] string locationId = null,
        [FromQuery] bool? exactModelMatch = null,
        [FromQuery] DateTimeOffset? startCheckTime = null,
        [FromQuery] DateTimeOffset? endCheckTime = null)
    {
        var job = await _dataQualityAdxService.QueueTwinsValidationProcessAsync(userId, modelIds?.Any() == true ? modelIds : null, exactModelMatch, locationId, startCheckTime, endCheckTime);

        return job;
    }

    /// <summary>
    /// Search twin validation jobs filtering by query string parameters
    /// </summary>
    /// <param name="id">twin validation job Id</param>
    /// <param name="userId">User Id</param>
    /// <param name="status">twin validation job status</param>
    /// <param name="from">twin validation job creation date time from filter</param>
    /// <param name="to">twin validation job creation date time to filter</param>
    /// <param name="fullDetails">Indicates if full details body from twin validation jobs must be retrieved</param>
    /// <returns>Returns a collection of twin validation jobs that match the provided filters</returns>
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
    ///				"target": [
    ///					"Twins"
    ///				]
    ///			}
    ///		]
    ///
    /// </remarks>
    /// <response code="200">Returns a collection of twin validation jobs that match the provided filters</response>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TwinsValidationJob>>> FindValidationJobs(
            [FromQuery] string id,
            [FromQuery] string userId,
            [FromQuery] AsyncJobStatus? status = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] bool fullDetails = false)
    {
        var jobs = await _dataQualityAdxService.FindValidationJobsAsync(id, status, userId, from, to, fullDetails);

        return jobs.ToList();
    }

    [HttpGet("get-latest-job")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<TwinsValidationJob>> GetLatestValidationJob(
            [FromQuery] AsyncJobStatus? status = null)
    {
        var job = await _dataQualityAdxService.GetLatestValidationJobAsync(status);

        if (job is null)
        {
            return NotFound("No latest validation job found.");
        }

        return job;
    }

    /// <summary>
    /// Delete twin validation jobs by jobId
    /// Sample request: https://https://localhost:8001/dqvalidation/delete?jobIds=nsmoorthy@willowinc.com.Twins.2023.03.24.18.39.26
    /// </summary>
    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> DeleteValidationJob(
            [FromQuery] string[] jobIds)
    {
        await _dataQualityAdxService.DeleteValidationJobAsync(jobIds);

        return Ok();
    }
}
