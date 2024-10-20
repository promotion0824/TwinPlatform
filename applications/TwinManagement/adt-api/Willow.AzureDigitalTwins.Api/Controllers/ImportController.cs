using Azure.DigitalTwins.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.Model.Adt;
using Willow.Model.Requests;

namespace Willow.AzureDigitalTwins.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ImportController : Controller
    {
        private readonly IBulkImportService _importService;

        public ImportController(IBulkImportService importService)
        {
            _importService = importService;
        }

        /// <summary>
        /// Creates an async job to import twins (with optional relationships) into the adt instance
        /// </summary>
        /// <param name="importTwins">Twins request with optional relationships</param>
        /// <param name="userId">User Id</param>
        /// <param name="userData">User data to be stored in the async job</param>
        /// <returns>A newly created async job</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST twins
        ///     {
        ///			"twins": [
        ///				{
        ///					"$dtId": "BPY-XX1",
        ///					"$metadata": {
        ///						"$model": "dtmi:com:willowinc:Building;1"
        ///					},
        ///					"type": "Commercial Office",
        ///					"code": "XCODE",
        ///					"name": "One Manhattan West"
        ///				}
        ///			],
        ///			"relationships": [
        ///				{
        ///					"$relationshipId": "includedIn_Portfolio-XX1_BPY-XX1",
        ///					"$targetId": "Portfolio-XX1",
        ///					"$sourceId": "BPY-XX1",
        ///					"$relationshipName": "includedIn"
        ///				}
        ///			]
        ///		}
        ///
        /// Sample response:
        ///
        ///		{
        ///			"jobId": "TLM Import.Twins.user@domain.com.2024.07.29.17.29.46.0279",
        ///			"status": "Queued"
        ///			"timeCreated": "2022-08-17T14:21:49.286354Z",
        ///			"timeLastUpdated": "2022-08-17T14:21:49.286354Z",
        ///			"userId": "user@domain.com",
        ///			"jobsEntryDetail":{}
        ///		}
        ///
        /// </remarks>
        /// <response code="200">Returns the newly created async job</response>
        /// <response code="400">If missing twins data in the request body</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("twins")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<JobsEntry>> TriggerTwinsImport([Required][FromBody] BulkImportTwinsRequest importTwins,
                [FromHeader(Name = "User-Id")][Required] string userId,
                [FromHeader(Name = "User-Data")] string userData = null)
        {
            if (importTwins?.Twins?.Any() == false)
                return BadRequest(new ValidationProblemDetails { Detail = "Twins are required" });

           var importJob =  await _importService.QueueBulkProcess(importTwins, EntityType.Twins, userId, userData);
            return importJob;

        }

        /// <summary>
        /// Creates an async job to import relationships into the adt instance
        /// </summary>
        /// <param name="relationships">Collection of relationships to import</param>
        /// <param name="userId">User Id</param>
        /// <param name="userData">User data to be stored in the async job</param>
        /// <returns>A newly created async job</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST relationships
        ///		[
        ///			{
        ///				"$relationshipId": "includedIn_Portfolio-XX1_BPY-XX1",
        ///				"$targetId": "Portfolio-XX1",
        ///				"$sourceId": "BPY-XX1",
        ///				"$relationshipName": "includedIn"
        ///			}
        ///		]
        ///
        /// Sample response:
        ///
        ///		{
        ///			"jobId": "TLM Import.Relationships.user@domain.com.2024.07.29.17.29.46.0279",
        ///			"status": "Queued"
        ///			"timeCreated": "2022-08-17T14:21:49.286354Z",
        ///			"timeLastUpdated": "2022-08-17T14:21:49.286354Z",
        ///			"userId": "user@domain.com",
        ///			"jobsEntryDetail":{}
        ///		}
        ///
        /// </remarks>
        /// <response code="200">Returns the newly created async job</response>
        /// <response code="400">If missing relationships data in the request body</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("relationships")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<JobsEntry>> TriggerRelationshipsImport([Required][FromBody] BasicRelationship[] relationships,
            [FromHeader(Name = "User-Id")][Required] string userId,
            [FromHeader(Name = "User-Data")] string userData = null)
        {
            if (relationships?.Any() == false)
                return BadRequest(new ValidationProblemDetails { Detail = "Relationships are required" });

            var importJob = await _importService.QueueBulkProcess(relationships, EntityType.Relationships, userId, userData);

            return importJob;
        }

        /// <summary>
        /// Creates an async job to import models into the adt instance
        /// </summary>
        /// <param name="models">Collection of models to import</param>
        /// <param name="userId">User Id</param>
        /// <param name="userData">User data to be stored in the async job</param>
        /// <returns>A newly created async job</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST models
        ///		[
        ///			{
        ///				"@id": "dtmi:com:willowinc:Component;1",
        ///				"@type": "Interface",
        ///				"displayName": {
        ///					"en": "Component"
        ///				},
        ///				"@context": [
        ///					"dtmi:dtdl:context;2"
        ///				]
        ///			}
        ///		]
        ///
        /// Sample response:
        ///
        ///		{
        ///			"jobId": "TLM Import.Models.user@domain.com.2024.07.29.17.29.46.0279",
        ///			"status": "Queued"
        ///			"timeCreated": "2022-08-17T14:21:49.286354Z",
        ///			"timeLastUpdated": "2022-08-17T14:21:49.286354Z",
        ///			"userId": "user@domain.com",
        ///			"jobsEntryDetail":{}
        ///		}
        ///
        /// </remarks>
        /// <response code="200">Returns the newly created async job</response>
        /// <response code="400">If missing models data in the request body or missing @id property in models</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("models")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<JobsEntry>> TriggerModelsImport([FromBody][Required] IEnumerable<JsonDocument> models,
                [FromHeader(Name = "User-Id")][Required] string userId,
                [FromHeader(Name = "User-Data")] string userData = null)
        {
            if (!models.Any())
                return BadRequest(new ValidationProblemDetails { Detail = "Models are required" });

            if (models.Any(x => !x.RootElement.TryGetProperty("@id", out JsonElement id)))
            {
                return BadRequest(new ValidationProblemDetails { Detail = "Missing ids in models" });
            }

            var importJob = await _importService.QueueModelsImport(models, userId, userData);

            return importJob;
        }

        /// <summary>
        /// Creates an async job to delete twins from the adt instance
        /// </summary>
        /// <param name="deleteRequest">Delete request</param>
        /// <param name="userId">User Id</param>
        /// <param name="userData">User data to be stored in the async job</param>
        /// <returns>A newly created async job</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE twins
        ///		{
        ///			"deleteAll": false,
        ///			"twinIds": ["BPW-1MW-Person-1048693609", "BPY-1MW-L01-021"],
        ///			"filters": {
        ///				"siteID": "122324-34343-4434"
        ///			}
        ///		}
        ///
        /// Sample response:
        ///
        ///		{
        ///			"jobId": "TLM Delete.Twins.user@domain.com.2024.07.29.17.29.46.0279",
        ///			"status": "Queued"
        ///			"timeCreated": "2022-08-17T14:21:49.286354Z",
        ///			"timeLastUpdated": "2022-08-17T14:21:49.286354Z",
        ///			"userId": "user@domain.com",
        ///			"jobsEntryDetail":{}
        ///		}
        ///
        /// </remarks>
        /// <response code="200">Returns the newly created async job</response>
        /// <response code="400">If invalid delete configuration provided</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("twins")]
        public async Task<ActionResult<JobsEntry>> BulkDeleteTwin(
                [FromBody][Required] BulkDeleteTwinsRequest deleteRequest,
                [FromHeader(Name = "User-Id")][Required] string userId,
                [FromHeader(Name = "User-Data")] string userData = null)
        {
            if (!deleteRequest.DeleteAll && !deleteRequest.TwinIds.Any())
            {
                return BadRequest(new ValidationProblemDetails { Detail = "Provide target twins to delete" });
            }

            if (deleteRequest.DeleteAll && deleteRequest.TwinIds.Any())
            {
                return BadRequest(new ValidationProblemDetails { Detail = "Delete all and twin ids can not be combined" });
            }

            var job = await _importService.QueueBulkProcess(deleteRequest, EntityType.Twins, userId, userData, delete: true);

            return job;
        }

        /// <summary>
        /// Creates an async job to delete models from the adt instance
        /// </summary>
        /// <param name="deleteRequest">Delete request</param>
        /// <param name="userId">User Id</param>
        /// <param name="userData">User data to be stored in the async job</param>
        /// <returns>A newly created async job</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE models
        ///		{
        ///			"modelIds": ["dtmi:com:willowinc:Generator;1"],
        ///			"includeDependencies": true
        ///		}
        ///
        /// Sample response:
        ///
        ///		{
        ///			"jobId": "TLM Delete.Models.user@domain.com.2024.07.29.17.29.46.0279",
        ///			"status": "Queued"
        ///			"timeCreated": "2022-08-17T14:21:49.286354Z",
        ///			"timeLastUpdated": "2022-08-17T14:21:49.286354Z",
        ///			"userId": "user@domain.com",
        ///			"jobsEntryDetail":{}
        ///		}
        ///
        /// </remarks>
        /// <response code="200">Returns the newly created async job</response>
        /// <response code="400">If invalid delete configuration provided</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("models")]
        public async Task<ActionResult<JobsEntry>> BulkDeleteModel(
                [FromBody][Required] BulkDeleteModelsRequest deleteRequest,
                [FromHeader(Name = "User-Id")][Required] string userId,
                [FromHeader(Name = "User-Data")] string userData = null)
        {
            if (!deleteRequest.DeleteAll && !deleteRequest.ModelIds.Any())
            {
                return BadRequest(new ValidationProblemDetails { Detail = "Provide target models to delete" });
            }

            if (deleteRequest.DeleteAll && deleteRequest.ModelIds.Any())
            {
                return BadRequest(new ValidationProblemDetails { Detail = "Delete all and model ids can not be combined" });
            }

            var job = await _importService.QueueBulkProcess(deleteRequest, EntityType.Models, userId, userData,delete: true);

            return job;
        }

        /// <summary>
        /// Creates an async job to delete relationships from the adt instance
        /// </summary>
        /// <param name="deleteRequest">Delete request</param>
        /// <param name="userId">User Id</param>
        /// <param name="userData">User data to be stored in the async job</param>
        /// <returns>A newly created async job</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE relationships
        ///		{
        ///			"deleteAll": false,
        ///			"twinIds": ["BPW-1MW-Person-1048693609", "BPY-1MW-L01-021"],
        ///			"relationshipIds": ["includedIn_Portfolio-XX1_BPY-XX1"]
        ///		}
        ///
        /// Sample response:
        ///
        ///		{
        ///			"jobId": "TLM Delete.Relationships.user@domain.com.2024.07.29.17.29.46.0279",
        ///			"status": "Queued"
        ///			"timeCreated": "2022-08-17T14:21:49.286354Z",
        ///			"timeLastUpdated": "2022-08-17T14:21:49.286354Z",
        ///			"userId": "user@domain.com",
        ///			"jobsEntryDetail":{}
        ///		}
        ///
        /// </remarks>
        /// <response code="200">Returns the newly created async job</response>
        /// <response code="400">If invalid delete configuration provided</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpDelete("relationships")]
        public async Task<ActionResult<JobsEntry>> BulkDeleteRelationships(
            [FromBody][Required] BulkDeleteRelationshipsRequest deleteRequest,
            [FromHeader(Name = "User-Id")][Required] string userId,
            [FromHeader(Name = "User-Data")] string userData = null)
        {
            if (!deleteRequest.DeleteAll && !deleteRequest.RelationshipIds.Any() && !deleteRequest.TwinIds.Any())
            {
                return BadRequest(new ValidationProblemDetails { Detail = "Provide target relationships to delete" });
            }

            if (deleteRequest.DeleteAll && (deleteRequest.RelationshipIds.Any() || deleteRequest.TwinIds.Any()))
            {
                return BadRequest(new ValidationProblemDetails { Detail = "Delete all and target relationships can not be combined" });
            }

            var job = await _importService.QueueBulkProcess(deleteRequest, EntityType.Relationships, userId, userData, delete: true);

            return job;
        }
    }
}
