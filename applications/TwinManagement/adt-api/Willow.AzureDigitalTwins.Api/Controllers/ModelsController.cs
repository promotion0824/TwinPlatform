using DTDLParser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.AzureDigitalTwins.Api.Extensions;
using Willow.AzureDigitalTwins.Api.Persistence.Models.TwinsApi;
using Willow.AzureDigitalTwins.Api.Services;
using Willow.AzureDigitalTwins.Api.Telemetry;
using Willow.AzureDigitalTwins.Services.Interfaces;
using Willow.Exceptions.Exceptions;
using Willow.Extensions.Logging;
using Willow.Model.Adt;
using Willow.Model.Async;
using Willow.Model.Requests;
using Willow.Model.Responses;
using Willow.Storage.Repositories;

namespace Willow.AzureDigitalTwins.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ModelsController : Controller
    {
        private readonly IAzureDigitalTwinReader _azureDigitalTwinReader;
        private readonly IAzureDigitalTwinModelParser _azureDigitalTwinModelParser;
        private readonly IAzureDigitalTwinWriter _azureDigitalTwinWriter;
        private readonly IExportService _exportService;
        private readonly IRepositoryService _repositoriesService;
        private readonly IBulkImportService _importService;
        private readonly IAdxService _adxService;
        private readonly ITwinsService _twinService;
        private readonly ITelemetryCollector _telemetryCollector;
        private readonly ILogger<ModelsController> _logger;

        public ModelsController(IAzureDigitalTwinReader azureDigitalTwinReader,
            IAzureDigitalTwinWriter azureDigitalTwinWriter,
            IExportService exportService,
            IRepositoryService repositoriesService,
            IBulkImportService importService,
            IAdxService adxService,
            IAzureDigitalTwinModelParser azureDigitalTwinModelParser,
            ITwinsService twinService,
            ITelemetryCollector telemetryCollector,
            ILogger<ModelsController> logger)
        {
            _azureDigitalTwinReader = azureDigitalTwinReader;
            _azureDigitalTwinWriter = azureDigitalTwinWriter;
            _exportService = exportService;
            _repositoriesService = repositoriesService;
            _importService = importService;
            _adxService = adxService;
            _azureDigitalTwinModelParser = azureDigitalTwinModelParser;
            _twinService = twinService;
            _telemetryCollector = telemetryCollector;
            _logger = logger;
        }

        /// <summary>
        /// Get models
        /// </summary>
        /// <param name="rootModel">Root model to get dependencies from</param>
        /// <param name="includeModelDefinitions">Indicates if full model definition must be returned</param>
        /// <param name="includeTwinCount">Returns calculated stats for twin count</param>
        /// <param name="locationId">Location Id</param>
        /// <param name="sourceType">Indicate which source type to query from</param>
        /// <remarks>
        ///	Sample reponse:
        ///
        ///	[
        ///		{
        ///			"id": "dtmi:com:willowinc:CompressorRunState;1",
        ///			"displayName": {
        ///				"en": "Compressor Run State"
        ///			},
        ///			"decommissioned": false,
        ///			"model": {
        ///				"@id": "dtmi:com:willowinc:CompressorRunState;1",
        ///				"@type": "Interface",
        ///				"displayName": {
        ///					"en": "Compressor Run State"
        ///				},
        ///				"extends": [
        ///					"dtmi:com:willowinc:RunState;1"
        ///				],
        ///				"contents": [],
        ///				"@context": [
        ///					"dtmi:dtdl:context;2"
        ///				]
        ///			},
        ///			"twinCount": {
        ///				"exact": 7,
        ///				"total": 7
        ///			}
        ///		}
        ///		{
        ///			...
        ///		}
        ///	]
        /// </remarks>
        /// <returns>Model definitions along with stats if requested</returns>
        /// <response code="200">Returns models from instance</response>
        /// <response code="400">If twin count is not requested and location id is sent</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModelResponse>>> GetModels(
        [FromQuery] string rootModel = null,
        [FromQuery] bool includeModelDefinitions = false,
        [FromQuery] bool includeTwinCount = false,
        [FromQuery] string locationId = null,
        [FromQuery] SourceType sourceType = SourceType.Adx)
        {
            if (!includeTwinCount && locationId != null)
                return BadRequest(new ValidationProblemDetails { Detail = "Location filter only applies to twin count" });

            var models = await MeasureExecutionTime.ExecuteTimed(async () => (await _azureDigitalTwinReader.GetModelsAsync(rootModel)).ToList(),
                (res, ms) =>
                {
                    _logger.LogInformation($"GetModels: azureDigitalTwinReader.GetModelsAsync took :{ms} milliseconds");
                    _telemetryCollector.TrackGetModelExecutionTime(ms);
                });

            if (!models.Any())
                return Ok(Enumerable.Empty<ModelResponse>());

            var twinCountByModel = new Dictionary<string, int>();

            List<string> modelIds = null;
            if (includeTwinCount)
            {
                modelIds = models.Select(x => x.Id).ToList();
                twinCountByModel = await MeasureExecutionTime.ExecuteTimed(async () => await _twinService.GetTwinCountByModelAsync(modelIds, locationId, sourceType),
                    (res, ms) =>
                    {
                        _logger.LogInformation($"GetModels: twinService.GetTwinCountByModelAsync took :{ms} milliseconds");
                        _telemetryCollector.TrackGetTwinCountExecutionTime(ms);
                    },
                    (ex) =>
                    {
                        _logger.LogInformation($"GetModels: twinService.GetTwinCountByModelAsync EXCEPTION {ex}");
                        _telemetryCollector.TrackGetTwinCountByModelException(1);
                    });
            }


            var responseModels = new ConcurrentBag<ModelResponse>();

            await MeasureExecutionTime.ExecuteTimed(
                async () =>
                Parallel.ForEach(models, x =>
                {
                    var modelResponse = GetModelResponse(includeModelDefinitions, includeTwinCount, x, twinCountByModel);
                    responseModels.Add(modelResponse);
                }),
                (res, ms) =>
                {
                    _logger.LogInformation($"GetModels: GetModelResponse took :{ms} milliseconds");
                    _telemetryCollector.TrackGetModelResponseExecutionTime(ms);
                });

            return responseModels.ToList();
        }

        private ModelResponse GetModelResponse(
            bool includeModelDefinitions,
            bool includeTwinCount,
            DigitalTwinsModelBasicData model,
            Dictionary<string, int> twinCountByModel)
        {
            var interfaceInfo = _azureDigitalTwinModelParser.GetInterfaceInfo(model.Id);

            var modelResponse = new ModelResponse
            {
                Id = model.Id,
                Model = includeModelDefinitions ? model.DtdlModel : null,
                DisplayName = interfaceInfo?.DisplayName,
                Description = interfaceInfo?.Description,
                UploadTime = model.UploadedOn
            };
            if (includeTwinCount)
            {
                twinCountByModel.TryGetValue(model.Id, out int exactCount);
                var descendants = _azureDigitalTwinModelParser.GetInterfaceDescendants(new List<string> { model.Id });
                var totalCount = descendants.Sum(v => twinCountByModel.GetValueOrDefault(v.Key));
                if (exactCount > 0 || totalCount > 0)
                {
                    modelResponse.TwinCount = new ModelStatsResponse(exactCount, totalCount);
                }
            }
            return modelResponse;
        }

        /// <summary>
        /// Get model by id
        /// </summary>
        /// <param name="id">Id of the target model</param>
        /// <param name="includeModelDefinitions">Indicates if full model definition must be returned</param>
        /// <param name="includeTwinCount">Returns calculated stats for twin count</param>
        /// <param name="locationId">Location Id</param>
        /// <remarks>
        ///	Sample reponse:
        ///
        ///		{
        ///			"id": "dtmi:com:willowinc:CompressorRunState;1",
        ///			"displayName": {
        ///				"en": "Compressor Run State"
        ///			},
        ///			"decommissioned": false,
        ///			"model": {
        ///				"@id": "dtmi:com:willowinc:CompressorRunState;1",
        ///				"@type": "Interface",
        ///				"displayName": {
        ///					"en": "Compressor Run State"
        ///				},
        ///				"extends": [
        ///					"dtmi:com:willowinc:RunState;1"
        ///				],
        ///				"contents": [],
        ///				"@context": [
        ///					"dtmi:dtdl:context;2"
        ///				]
        ///			},
        ///			"twinCount": {
        ///				"exact": 7,
        ///				"total": 7
        ///			}
        ///		}
        /// </remarks>
        /// <returns>Model from instance</returns>
        /// <response code="200">Returns model from instance</response>
        /// <response code="404">If model not found</response>
        /// <response code="400">If twin count is not requested and location id is sent</response>
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ModelResponse>> GetModel(
            [FromRoute][Required(AllowEmptyStrings = false)] string id,
            [FromQuery] bool includeModelDefinitions = false,
            [FromQuery] bool includeTwinCount = false,
            [FromQuery] string locationId = null)
        {
            if (!includeTwinCount && locationId != null)
                return BadRequest(new ValidationProblemDetails { Detail = "Location filter only applies to twin count" });

            var model = await _azureDigitalTwinReader.GetModelAsync(id);
            if (model == null)
                return NotFound(new ValidationProblemDetails { Detail = "Model not found" });

            var twinCount = new Dictionary<string, int>();
            if (includeTwinCount)
                twinCount = await _adxService.GetTwinCountByModelAsync(locationId, model.Id);

            var modelResponse = GetModelResponse(includeModelDefinitions, includeTwinCount, model, twinCount);

            return modelResponse;
        }

        /// <summary>
        /// Creates models
        /// </summary>
        /// <param name="models">Model definitions</param>
        /// <remarks>
        ///	Sample request:
        ///
        ///		POST
        ///		[
        ///			{
        ///				"@id": "dtmi:com:willowinc:InductanceSensor;1",
        ///				"@type": "Interface",
        ///				"displayName": {
        ///					"en": "Inductance Sensor"
        ///				},
        ///				"extends": [
        ///					"dtmi:com:willowinc:QuantitySensor;1"
        ///				],
        ///				"contents": [],
        ///				"@context": [
        ///					"dtmi:dtdl:context;2"
        ///				]
        ///			}
        ///		]
        /// </remarks>
        /// <response code="200">Returns ok when models created</response>
        /// <response code="400">If no models provided</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult> CreateModels([FromBody][Required] IEnumerable<JsonDocument> models)
        {
            if (!models.Any())
                return BadRequest();

            var parsedModels = models.Select(x => x.ToJsonString()).ToList();

            try
            {
                var modelParser = new ModelParser();
                modelParser.Parse(parsedModels);
            }
            catch (ParsingException pex)
            {
                return BadRequest(new { title = "Parsing error", pex.Errors });
            }
            catch (Exception ex)
            {
                return BadRequest(new ValidationProblemDetails { Detail = ex.Message });
            }

            var createdModels = await _azureDigitalTwinWriter.CreateModelsAsync(models.Select(x => new DigitalTwinsModelBasicData { Id = x.RootElement.GetProperty("@id").ToString(), DtdlModel = x.ToJsonString() }));

            await _exportService.SyncModelsCreate(createdModels.Select(x => x.Id));

            return Ok();
        }

        /// <summary>
        /// Delete model by id
        /// </summary>
        /// <param name="id">Model id to delete</param>
        /// <response code="204">When model is successfully delete</response>
        /// <response code="404">When model is not found</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteModel([FromRoute][Required(AllowEmptyStrings = false)] string id)
        {
            var model = await _azureDigitalTwinReader.GetModelAsync(id);
            if (model == null)
                return NotFound();

            await _azureDigitalTwinWriter.DeleteModelAsync(id);

            await _exportService.SyncModelDelete(model);

            return NoContent();
        }

        /// <summary>
        /// Creates an async job to load models from repositories
        /// </summary>
        /// <param name="repositories">Repositories information</param>
        /// <param name="userId">User Id</param>
        /// <param name="userData">User Data</param>
        /// <returns>A newly created async job</returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST upgrade/repo
        ///     [
        ///			{
        ///				"owner": "willowinc",
        ///				"repository": "opendigitaltwins-building",
        ///				"path": "ontology",
        ///				"submodules": ["Ontology/opendigitaltwins-building"]
        ///			}
        ///		]
        ///
        /// Sample response:
        ///
        ///		{
        ///			"jobId": "user@domain.com.Models.2022.08.10.15.08.16",
        ///			"details": {
        ///				"status": "Queued"
        ///			},
        ///			"createTime": "2022-08-10T15:08:16.8065658Z",
        ///			"lastUpdateTime": "2022-08-10T15:08:22.1577255Z",
        ///			"userId": "user@domain.com",
        ///			"target": [
        ///				"Models"
        ///			]
        ///		}
        ///
        /// </remarks>
        /// <response code="200">Returns the newly created async job</response>
        /// <response code="400">If missing repositories data, no models are retrieved from the repos or submodules incorrect path (submodules path is case sensitive)</response>
        [HttpPost("upgrade/repo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<JobsEntry>> UpgradeFromRepos(
                [Required][FromBody] IEnumerable<UpgradeModelsRepoRequest> repositories,
                [FromHeader(Name = "User-Id")][Required] string userId,
                [FromHeader(Name = "User-Data")] string userData = null)
        {
            if (!repositories.Any())
                return BadRequest(new ValidationProblemDetails { Detail = "Missing repositories" });

            var models = new ConcurrentBag<JsonDocument>();
            var readRepos = repositories.Select(async x =>
            {
                var files = await _repositoriesService.GetRepositoryContent(x.Owner, x.Repository, x.Ref, x.Submodules, x.Path, ".json");
                foreach (var file in files)
                    models.Add(JsonDocument.Parse(file.Value));
            });

            try
            {
                await Task.WhenAll(readRepos);
            }
            catch (NotFoundException notFoundException)
            {
                return BadRequest(new ValidationProblemDetails { Detail = notFoundException.Message });
            }
            catch (HttpRequestException httpException) when (httpException.StatusCode == HttpStatusCode.NotFound)
            {
                return BadRequest(new ValidationProblemDetails { Detail = "Repository not found" });
            }

            if (!models.Any())
                return BadRequest(new ValidationProblemDetails { Detail = "No models found in repositories" });

            var importJob = await _importService.QueueModelsImport(models, userId, userData, isFullOverlay: true);
            return importJob;
        }

        /// <summary>
        /// Creates an async job to load models from zip files
        /// </summary>
        /// <param name="zipFiles">Zip files containing model definitions</param>
        /// <param name="userId">User Id</param>
        /// <param name="path">Path to folder that contains model definition</param>
        /// <returns>A newly created async job</returns>
        /// <remarks>
        /// Sample response:
        ///
        ///		{
        ///			"jobId": "user@domain.com.Models.2022.08.10.15.08.16",
        ///			"details": {
        ///				"status": "Queued"
        ///			},
        ///			"createTime": "2022-08-10T15:08:16.8065658Z",
        ///			"lastUpdateTime": "2022-08-10T15:08:22.1577255Z",
        ///			"userId": "user@domain.com",
        ///			"target": [
        ///				"Models"
        ///			]
        ///		}
        ///
        /// </remarks>
        /// <response code="200">Returns the newly created async job</response>
        /// <response code="400">If no files provided or no models in files</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("upgrade/zip")]
        [Consumes("multipart/form-data")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<JobsEntry>> UpgradeFromZipFiles(
            [Required] IEnumerable<IFormFile> zipFiles,
            [FromHeader(Name = "User-Id")][Required] string userId,
            [FromQuery] string path = null)
        {
            if (!zipFiles.Any())
                return BadRequest(new ValidationProblemDetails { Detail = "Missing files" });

            var models = new List<JsonDocument>();
            foreach (var zipFile in zipFiles)
            {
                var files = _repositoriesService.ReadContent(zipFile.OpenReadStream(), path, ".json");
                foreach (var item in files)
                    models.Add(JsonDocument.Parse(item.Value));
            }

            if (!models.Any())
                return BadRequest(new ValidationProblemDetails { Detail = "No models found in provided files" });

            var importJob = await _importService.QueueModelsImport(models, userId);

            return importJob;
        }
    }
}
