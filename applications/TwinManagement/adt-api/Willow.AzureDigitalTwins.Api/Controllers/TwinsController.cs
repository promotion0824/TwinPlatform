using Azure.DigitalTwins.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Willow.Api.Binding.Attributes;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.DataQuality.Api.Services;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.DataQuality.Model.Responses;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.Model.Responses;

namespace Willow.AzureDigitalTwins.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class TwinsController : Controller
    {
        private readonly IAzureDigitalTwinReader _azureDigitalTwinReader;
        private readonly IAzureDigitalTwinWriter _azureDigitalTwinWriter;
        private readonly ITwinsService _twinsService;
        private readonly IDQRuleService _rulesService;
        private readonly IExportService _exportService;
        private readonly IBulkImportService _importService;
        private readonly ILogger<TwinsController> _logger;

        public TwinsController(IAzureDigitalTwinReader azureDigitalTwinReader,
            IAzureDigitalTwinWriter azureDigitalTwinWriter,
            ITwinsService twinsService,
            IDQRuleService rulesService,
            IExportService exportService,
            IBulkImportService importService,
            ILogger<TwinsController> logger)
        {
            _azureDigitalTwinReader = azureDigitalTwinReader;
            _azureDigitalTwinWriter = azureDigitalTwinWriter;
            _twinsService = twinsService;
            _rulesService = rulesService;
            _exportService = exportService;
            _importService = importService;
            _logger = logger;
        }

        /// <summary>
        /// Query twins
        /// </summary>
        /// <param name="request">Query twins criteria</param>
        /// <remarks>
        /// Sample request
        ///
        ///		POST
        ///		{
        ///			"request": {
        ///		        "modelIds": ["dtmi:com:willowinc:AirHandlingUnit;1"],
        ///		        "locationId": "53d380c2-d31a-4cd1-8958-795407407a82",
        ///		        "exactModelMatch":true,
        ///		        "includeRelationships": true,
        ///		        "includeIncomingRelationships": true,
        ///		        "orphanOnly": false,
        ///		        "sourceType": "AdtQuery",
        ///		        "relationshipsToTraverse":[],
        ///		        "searchString": "AHU",
        ///		        "startTime": "2023-04-15T20:37:21.0274638Z",
        ///		        "endTime": "2023-04-25T20:37:21.0274638Z"
        ///			}
        ///		}
        /// </remarks>
        /// <param name="pageSize">Limit the number of twins to return.</param>
        /// <param name="continuationToken">Continuation token</param>
        /// <param name="includeTotalCount">When querying by ADT, return the total count of items that match the filter criteria along with the first page of items. Using this flag for ADT has the same cost as issuing an additional call to GetTwinCount with the same filter parameters, minus the extra REST call from your application</param>
        /// <returns>Matching twins with relationships</returns>
        /// <response code="200">Matching twins with relationships</response>
        /// <response code="400">Bad Request. LocationId param cannot be empty when relationshipToTraverse param is specified</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Page<TwinWithRelationships>>> QueryTwins(
            [FromBody][Required] GetTwinsInfoRequest request,
            [FromQuery] int pageSize = 100,
            [FromHeader] string continuationToken = null,
            [FromQuery] bool includeTotalCount = false)
        {
            if (request.RelationshipsToTraverse?.Length > 0)
            {
                if (string.IsNullOrEmpty(request.LocationId))
                    return BadRequest(new ValidationProblemDetails
                    {
                        Detail = "LocationId param cannot be empty when relationshipToTraverse param is specified"
                    });
                if (request.SourceType == SourceType.Adx)
                    return BadRequest(new ValidationProblemDetails
                    {
                        Detail = "Search type cannot be adx when relationshipToTraverse param is specified"
                    });
            }

            if (request.OrphanOnly && request.SourceType != SourceType.Adx)
                return BadRequest(new ValidationProblemDetails
                {
                    Detail = "OrphanOnly is only supported for ADX"
                });

            var digitalTwins = await _twinsService.GetTwins(request,
                                                   pageSize,
                                                   continuationToken,
                                                   includeTotalCount);

            return Ok(digitalTwins);
        }

        /// <summary>
        /// Get twins
        /// </summary>
        /// <param name="request">Request includes : modelId, locationId, exactModelMatch, includeRelationships, includeIncomingRelationships, sourceType(Adx,Adtquery,AdtMemory)
        /// , orphanOnly, relationshipsToTraverse(IsPartOf, LocatedIn, IncludedIn), searchString,
        /// QueryFilter (QueryFilter with Type and Filter condition;This filter is meant to only be used in special cases and when the query is not automatically
        /// generated by ADTAPI from the rest of the GetTwinsInfoRequest properties.
        /// A Type=Direct filter must be in the format specific to the database specified by SourceType) </param>
        /// <param name="pageSize">Page size</param>
        /// <remarks>
        /// Sample request
        ///
        ///		POST
        ///		{
        ///			"request": {
        ///		        "modelIds": ["dtmi:com:willowinc:AirHandlingUnit;1"],
        ///		        "locationId": "53d380c2-d31a-4cd1-8958-795407407a82",
        ///		        "exactModelMatch":true,
        ///		        "includeRelationships": true,
        ///		        "includeIncomingRelationships": true,
        ///		        "orphanOnly": false,
        ///		        "sourceType": "AdtQuery",
        ///		        "relationshipsToTraverse":[],
        ///		        "searchString": "AHU",
        ///		        "startTime": "2023-04-15T20:37:21.0274638Z",
        ///		        "endTime": "2023-04-25T20:37:21.0274638Z",
        ///             "QueryFilter.Filter": "Id == 'FAW-IMIC-L01-LGT-1617' and Location.SiteId == '5e2c88fb-42ce-4ede-9203-b3015a701f10'", // For ADX
        ///		        "QueryFilter.Filter": "twins.supplyFan.nominalExternalStaticPressure = 3.25", // For ADT
        ///		        "Type": "DIRECT"
        ///			    }
        ///		}
        /// </remarks>
        /// <param name="continuationToken">Continuation token</param>
        /// <param name="includeTotalCount">When querying by ADT, return the total count of items that match the filter criteria along with the first page of items. Using this flag for ADT has the same cost as issuing an additional call to GetTwinCount with the same filter parameters, minus the extra REST call from your application</param>
        /// <returns>Matching twins with relationships</returns>
        /// <response code="200">Target twins retrieved</response>
        /// <response code="400">Bad Request. LocationId param cannot be empty when relationshipToTraverse param is specified</response>
        [HttpPost("gettwins")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Page<TwinWithRelationships>>> GetTwins(
            [FromBody][Required] GetTwinsInfoRequest request,
            [FromQuery] int pageSize = 100,
            [FromQuery] bool includeTotalCount = false,
            [FromHeader] string continuationToken = null)
        {
            if (request.RelationshipsToTraverse?.Length > 0)
            {
                if (string.IsNullOrEmpty(request.LocationId))
                    return BadRequest(new ValidationProblemDetails
                    {
                        Detail = "LocationId param cannot be empty when relationshipsToTraverse param is specified"
                    });
                if (request.SourceType == SourceType.Adx)
                    return BadRequest(new ValidationProblemDetails
                    {
                        Detail = "Search type cannot be adx when relationshipsToTraverse param is specified"
                    });
            }

            if (request.OrphanOnly && request.SourceType != SourceType.Adx)
                return BadRequest(new ValidationProblemDetails
                {
                    Detail = "OrphanOnly is only supported for ADX"
                });

            _logger.LogInformation($"GetTwins for models: {string.Join(", ", request.ModelId)}," +
                $" IncludeRelationships: {request.IncludeRelationships}, " +
                $"IncludeIncomingRelationships: {request.IncludeIncomingRelationships}, exactModelMatch: {request.ExactModelMatch}");


            var digitalTwins = await _twinsService.GetTwins(request,
                                                   pageSize,
                                                   continuationToken,
                                                   includeTotalCount);

            return Ok(digitalTwins);
        }

        /// <summary>
        /// Get twins in tree form
        /// </summary>
        /// <param name="rootModelIds">Target model ids</param>
        /// <param name="childModelIds">Child Model Ids that restricts the type of twins in the tree response.</param>
        /// <param name="outgoingRelationships">List of relationship types to be considered for traversal.
        /// <br/>             Default Values : ["isPartOf", "locatedIn"] will be used when relationshipsToTraverse is not supplied</param>
        /// <param name="incomingRelationships">List of relationship types to be considered for traversal.</param>
        /// <param name="exactModelMatch">Indicates if model filter must be exact match</param>
        /// <remarks>
        ///	Sample response
        ///	[
        ///		{
        ///			"twin": {
        ///				"$dtId": "THE-TWIN-ID",
        ///				"$metadata": {
        ///					"$model": "dtmi:com:willowinc:OccupancyZone;1"
        ///				}
        ///			},
        ///			"children": [
        ///				{
        ///					"twin": {},
        ///					"children": []
        ///				}
        ///				{
        ///				...
        ///				}
        ///			]
        ///		}
        ///	]
        /// </remarks>
        /// <returns>Nested twins with target models. Full tree is returned following relationships. If any twin has more than one parent, it only will be assigned to a single root</returns>
        /// <response code="200">Twin tree</response>
        /// <response code="400">If no model is provided</response>
        [HttpPost("trees/model")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<NestedTwin>>> GetTreesByModel(
            [FromQuery] string[] rootModelIds,
            [FromQuery] string[] childModelIds=null,
            [FromQuery] string[] outgoingRelationships = null,
            [FromQuery] string[] incomingRelationships = null,
            [FromQuery] bool exactModelMatch = false)
        {
            // set default values
            if (outgoingRelationships == null || outgoingRelationships.Length == 0)
                outgoingRelationships = ["isPartOf", "locatedIn"];


            if (rootModelIds == null || !rootModelIds.Any())
                return BadRequest(new ValidationProblemDetails { Detail = "Provide at least one model" });

            _logger.LogInformation($"GetTree for models: {string.Join(", ", rootModelIds)}, outgoingRelationships: {outgoingRelationships}, " +
                $"incomingRelationships: {incomingRelationships}, exactModelMatch: {exactModelMatch}");

            var tree = await _twinsService.GetTreeByModelsAsync(rootModelIds, childModelIds, outgoingRelationships, incomingRelationships, exactModelMatch);

            return Ok(tree);
        }

        /// <summary>
        /// Get twins in tree form
        /// </summary>
        /// <param name="twinScopeIds">Root twin ids</param>
        /// <param name="childModelIds">Child Model Ids that restricts the type of twins in the tree response.</param>
        /// <param name="outgoingRelationships">List of relationship types to be considered for traversal.
        /// <br/>             Default Values : ["isPartOf", "locatedIn"] will be used when relationshipsToTraverse is not supplied</param>
        /// <param name="incomingRelationships">List of relationship types to be considered for traversal.</param>
        /// <remarks>
        ///	Sample response
        ///	[
        ///		{
        ///			"twin": {
        ///				"$dtId": "THE-TWIN-ID",
        ///				"$metadata": {
        ///					"$model": "dtmi:com:willowinc:OccupancyZone;1"
        ///				}
        ///			},
        ///			"children": [
        ///				{
        ///					"twin": {},
        ///					"children": []
        ///				}
        ///				{
        ///				...
        ///				}
        ///			]
        ///		}
        ///	]
        /// </remarks>
        /// <returns>Nested twins with target models. Full tree is returned following relationships. If any twin has more than one parent, it only will be assigned to a single root</returns>
        /// <response code="200">Twin tree</response>
        /// <response code="400">If no model is provided</response>
        [HttpPost("trees/scope")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<NestedTwin>>> GetTreesByScope(
            [FromQuery] string[] twinScopeIds,
            [FromQuery] string[] childModelIds = null,
            [FromQuery] string[] outgoingRelationships = null,
            [FromQuery] string[] incomingRelationships = null)
        {
            // set default values
            if (outgoingRelationships == null || outgoingRelationships.Length == 0)
                outgoingRelationships = ["isPartOf", "locatedIn"];


            if (twinScopeIds == null || !twinScopeIds.Any())
                return BadRequest(new ValidationProblemDetails { Detail = "Provide at least one twin Id" });

            _logger.LogInformation($"GetTree for scope: {string.Join(", ", twinScopeIds)}, outgoingRelationships: {outgoingRelationships}, " +
                $"incomingRelationships: {incomingRelationships}");

            var tree = await _twinsService.GetTreeByIdsAsync(twinScopeIds, childModelIds, outgoingRelationships, incomingRelationships);

            return Ok(tree);
        }

        /// <summary>
        /// Count of Twins in ADX/ADT matching the criteria
        /// Sample request: https://localhost:8001/twins/count?exactModelMatch=true&modelId=dtmi:com:willowinc:AirHandlingUnit;1&searchString=ahu
        /// </summary>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetTwinsCount(
            [FromQuery][Required] GetTwinsInfoRequest request)
        {
            var digitalTwins = await _twinsService.GetTwinsCount(request);

            return Ok(digitalTwins);
        }

        /// <summary>
        /// Patch a twin
        /// </summary>
        /// <param name="id">Twin id</param>
        /// <param name="jsonPatchDocument">Patch information</param>
        /// <remarks>
        ///	Sample request
        ///
        ///		PATCH
        ///		[
        ///			{
        ///				"op":"replace",
        ///				"path":"/customproperties/description",
        ///				"value":"Patched description yes 8"
        ///			}
        ///		]
        /// </remarks>
        /// <response code="404">Twin not found</response>
        /// <response code="200">Twin patched</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> PatchTwin(
            [FromRoute][Required(AllowEmptyStrings = false)] string id,
            [FromBody][Required][NewsontsoftJsonInputFormatterPriorityBinder] JsonPatchDocument<Twin> jsonPatchDocument,
            [FromQuery] bool includeAdxUpdate = false)
        {
            var digitalTwin = await _azureDigitalTwinReader.GetDigitalTwinAsync(id);
            if (digitalTwin == null)
            {
                return NotFound();
            }

            var twin = digitalTwin.ToApiTwin();

            if (includeAdxUpdate)
            {
                //// patch adt twin and write to ADX
                //jsonPatchDocument.ApplyTo(twin);
                //var updatedTwin = twin.ToBasicDigitalTwin();
                //await _exportService.AppendTwinToAdx(twinForAdxUpdate, flagDelete: false);

                throw new NotImplementedException();

            }

            await _azureDigitalTwinWriter.UpdateDigitalTwinAsync(twin.ToBasicDigitalTwin(), jsonPatchDocument.ToAzureJsonPatchDocument());




            return Ok();
        }

        /// <summary>
        /// Get twin by id
        /// </summary>
        /// <param name="id">Twin id</param>
        /// <param name="sourceType">ADX/AdtQuery</param>
        /// <param name="includeRelationships">true/false</param>
        /// <returns>Twin informtion</returns>
        /// <response code="404">Twin not found</response>
        /// <response code="200">Twin information</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TwinWithRelationships>> GetTwinById(
                                                                    [FromRoute][Required(AllowEmptyStrings = false)] string id,
                                                                    [FromQuery] SourceType sourceType = SourceType.AdtQuery,
                                                                    [FromQuery] bool includeRelationships = false)
        {
            var twin = await _twinsService.GetTwinsByIds(new string[] { id }, sourceType, includeRelationships);

            if (twin == null || !twin.Content.Any())
            {
                return NotFound(id);
            }

            return Ok(twin.Content.First());
        }

        /// <summary>
        /// Get twins by ids
        /// </summary>
        /// <remarks>Currently queries are not chunked into multiple queries if there are a large number of IDs passed in -- it's
        /// up to the caller to ensure that the number of IDs or total query length does not exceed the limitations of the database
        /// that is queried. Paging is not supported. The caller can control the number of twins returned by the number of IDs passed in.
        /// Any ids that do not reference valid twins will silently be omitted from the response -- no 404/NotFound will be generated.
        /// Match response against your query to find any invalid IDs.
        ///  </remarks>
        /// <param name="ids">Twin ids</param>
        /// <param name="sourceType">Adx,AdtQuery,AdtMemory,Acs</param>
        /// <param name="includeRelationships"></param>
        /// <returns>Twin informtion</returns>
        /// <response code="404">Twin not found</response>
        /// <response code="200">Twin information</response>
        [HttpPost("ids")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Page<TwinWithRelationships>>> GetTwinsByIds(
                                                                    [FromBody][Required(AllowEmptyStrings = false)] string[] ids,
                                                                    [FromQuery] SourceType sourceType = SourceType.AdtQuery,
                                                                    [FromQuery] bool includeRelationships = false)
        {
            var twins = await _twinsService.GetTwinsByIds(ids, sourceType, includeRelationships);

            if (twins.Content.Count() != ids.Length)
            {
                var idsNotFound = ids.Where(n => !twins.Content.Any(a => a.Twin.Id == n)).ToList();
                _logger.LogWarning($"Twins not found: {string.Join(", ", idsNotFound)}. Count: {idsNotFound.Count}");
            }

            _logger.LogInformation($"GetTwinsByIds - sourceType: {sourceType}, includeRelationships: {includeRelationships}, requestedCount: {ids.Length}, twinsCount: {twins.Content.Count()}");

            return twins;
        }

        /// <summary>
        /// Creates or replaces a twin
        /// </summary>
        /// <param name="twin">Twin information</param>
        /// <remarks>
        /// Sample request
        ///
        ///		PUT
        ///		{
        ///			"twin": {
        ///				"$dtId": "BPY-1MW",
        ///				"$metadata": {
        ///					"$model": "dtmi:com:willowinc:Building;1"
        ///				},
        ///				"type": "Commercial Office",
        ///				"coordinates": {
        ///					"latitude": 40.7528,
        ///					"longitude": -73.997934
        ///				},
        ///				"elevation": 34,
        ///				"height": 995,
        ///				"uniqueID": "4e5fc229-ffd9-462a-882b-16b4a63b2a8a",
        ///				"code": "1MX",
        ///				"name": "One Miami West",
        ///				"siteID": "4e5fc229-ffd9-462a-882b-16b4a63b2a8a",
        ///				"address": {
        ///					 "region": "NY"
        ///				}
        ///			}
        ///		}
        /// </remarks>
        /// <returns>Created or replaced twin</returns>
        /// <response code="200">Twin information</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BasicDigitalTwin>> UpdateTwin(
            [FromBody][Required] BasicDigitalTwin twin,
            [FromQuery] bool includeAdxUpdate = false)
        {

            var digitalTwin = await _azureDigitalTwinWriter.CreateOrReplaceDigitalTwinAsync(twin);

            if (includeAdxUpdate)
            {
                // Update ADX with the ADT twin that've just been updated.
                await _exportService.AppendTwinToAdx(digitalTwin, flagDelete: false);

            }

            return Ok(digitalTwin);

        }

        /// <summary>
        /// Validates incoming twins according to rule templates
        /// </summary>
        /// <remarks>
        /// Sample request
        ///		POST
        ///		[
        ///			{
        ///				"$dtId": "BPY-1MW-Person-1048687360-Total-Count",
        ///				"$metadata": {
        ///					"$model": "dtmi:com:willowinc:PeopleCountSensor-TEST;1"
        ///				},
        ///				"trendID": "allowed-xxx",
        ///				"name": "Total Count",
        ///				"communication": {
        ///					"$metadata": {}
        ///				},
        ///				"categorizationProperties": {
        ///					"$metadata": {}
        ///				}
        ///			}
        ///		]
        ///
        /// Sample response
        ///		[
        ///			{
        ///				"twinId": "BPY-1MW-Person-1048687360-Total-Count-2",
        ///				"results": [
        ///					{
        ///						"ruleId": "test-rule",
        ///						"propertyErrors": {
        ///							"trendID": [
        ///								"InvalidValue"
        ///							],
        ///							"siteID": [
        ///								"RequiredMissing"
        ///							]
        ///						}
        ///					}
        ///				]
        ///			}
        ///		]
        /// </remarks>
        /// <param name="twins">Twins to validate</param>
        /// <returns>Validation results for invalid twins</returns>
        /// <response code="200">Validation results from invalid twins</response>
        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TwinValidationResultResponse>>> ValidateTwins([FromBody][Required] IEnumerable<BasicDigitalTwin> twins)
        {
            var twinsWithRels = twins.Select(t =>
                      new TwinWithRelationships
                      {
                          Twin = t,
                          IncomingRelationships = null,
                          OutgoingRelationships = null,
                          TwinData = new Dictionary<string, object>()
                      }).ToList();

            var results = await _rulesService.GetValidationResults(twinsWithRels);

            return Ok(results.ToApiValidationResults());
        }

        /// <summary>
        /// Delete twins and optionally relationships. Return a MultipleEntityResponse with the status of each twin and relationship deletion.
        /// This API will forceably add a deletion record for the twin if it's not found in ADT, otherwise
        ///   it will depend on the event pipeline to eventually delete from ADX.
        /// </summary>
        /// <param name="twinIds">List of Twin ids to delete</param>
        /// <param name="deleteRelationships">If true, delete all incoming and outgoing relationship linked to each twin before attempting to delete the twin</param>
        [HttpDelete("twinsandrelationships")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status207MultiStatus)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MultipleEntityResponse>> DeleteTwinsAndRelationships(
            [FromBody] string[] twinIds,
            [FromQuery] bool deleteRelationships = false)
        {
            var multipleEntityResponse = await _twinsService.DeleteTwinsAndRelationships(twinIds, deleteRelationships);

            return multipleEntityResponse;
        }

        /// <summary>
        /// Creates an async job to delete twins from the adt instance
        /// </summary>
        /// <response code="200">Returns the newly created async job</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpDelete("delete")]
        public async Task<ActionResult<JobsEntry>> BulkDeleteTwin(
                [FromHeader(Name = "User-Id")][Required] string userId,
                [FromBody] string[] modelIds = null,
                [FromQuery] string locationId = null,
                [FromQuery] string searchString = null)
        {
            var deleteRequest = new BulkDeleteTwinsRequest();
            deleteRequest.SearchString = searchString;
            deleteRequest.LocationId = locationId;
            deleteRequest.ModelIds = modelIds;
            deleteRequest.DeleteAll = true;

            _logger.LogInformation($"BulkDeleteTwin for models: {string.Join(", ", modelIds)}, locationId: {locationId}, searchString: {searchString}");

            var job = await _importService.QueueBulkProcess(deleteRequest, EntityType.Twins, userId, userData: null, delete: true);

            return job;
        }
    }
}
