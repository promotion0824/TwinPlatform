using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using Json.Patch;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Configs;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.CognitiveSearch;
using PlatformPortalXL.Services.Sites;
using PlatformPortalXL.Services.Twins;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using Willow.Api.DataValidation;
using Willow.Proxy;

namespace PlatformPortalXL.Features.Twins
{
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class TwinsController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IHttpClientFactory _clientFactory;
        private readonly IDigitalTwinApiService _digitalTwinApiService;
        private readonly IDirectoryApiService _directoryApi;
        private readonly ITwinService _twinService;
        private readonly ISiteService _siteService;
        private readonly IFileService _fileService;
        private readonly CustomerInstanceConfigurationOptions _customerInstanceConfigurationOptions;
        private readonly IFloorsService _floorService;
        private readonly ISearchService _searchService;
        private readonly IAuthFeatureFlagService _featureFlagService;
        private readonly ITwinTreeAuthEvaluator _twinTreeAuthEvaluator;
        private readonly IUserAuthorizedSitesService _userAuthorizedSitesService;

        public TwinsController(
            IAccessControlService accessControl,
            IHttpClientFactory clientFactory,
            IDigitalTwinApiService digitalTwinApiService,
            IDirectoryApiService directoryApi,
            ITwinService twinService,
            ISiteService siteService,
            IFileService fileService,
            IOptions<CustomerInstanceConfigurationOptions> customerInstanceConfigurationOptions,
            IFloorsService floorService,
            IEnumerable<ISearchService> searchServices,
            IAuthFeatureFlagService featureFlagService,
            ITwinTreeAuthEvaluator twinTreeAuthEvaluator,
            IUserAuthorizedSitesService userAuthorizedSitesService
        )
        {
            _accessControl = accessControl;
            _clientFactory = clientFactory;
            _digitalTwinApiService = digitalTwinApiService;
            _directoryApi = directoryApi;
            _twinService = twinService;
            _siteService = siteService;
            _fileService = fileService;
            _customerInstanceConfigurationOptions = customerInstanceConfigurationOptions.Value;
            _floorService = floorService;
            _featureFlagService = featureFlagService;
            _twinTreeAuthEvaluator = twinTreeAuthEvaluator;
            _userAuthorizedSitesService = userAuthorizedSitesService;

            var requiredSearchServiceType = featureFlagService.IsFineGrainedAuthEnabled
                ? typeof(ScopedTwinSearchService)
                : typeof(SearchService);
            _searchService = searchServices.Single(s => s.GetType() == requiredSearchServiceType);
        }

        /// <summary>
        /// Call DigitalTwinCore Twin query as a generic proxy call so we don't need to
        ///    redefine all the request and return types here in PPXL.
        /// TODO: We should do throughout this service, using one of the declarative proxy libraries
        /// </summary>
        /// <returns></returns>
        [HttpPost("sites/{siteId}/twins/query")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task PostTwinQuery([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var proxyUri = $"admin/sites/{siteId}/twins/query/realestate";
            using var adtClient = _clientFactory.CreateClient(ApiServiceNames.DigitalTwinCore);
            using var streamContent = new StreamContent(Request.Body);

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(adtClient.BaseAddress + proxyUri),
                Method = HttpMethod.Post,
                Content = streamContent
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content.Headers.Add("Content-Type", "application/json");
            var response = await adtClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            Response.StatusCode = (int)response.StatusCode;
            Response.ContentType = response.Content.Headers.ContentType?.ToString();
            Response.ContentLength = response.Content.Headers.ContentLength;

            // TODO: pipe stream directly to response w/o reading as string?
            await Response.WriteAsync(responseContent);
        }

        /// <summary>
        /// Get Twin information.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="twinId"></param>
        /// <param name="includeModel"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/twins/{twinId}")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> GetTwin([FromRoute] Guid siteId, [FromRoute] string twinId, [FromQuery] bool includeModel)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var twin = await RetrieveTwin(siteId, twinId, includeModel);

            return Content(twin.ToString(), MediaTypeNames.Application.Json);
        }

        /// <summary>
        /// Get Twin information.
        /// </summary>
        /// <param name="dtId"></param>
        /// <param name="includeModel"></param>
        /// <returns></returns>
        [MapToApiVersion("2.0")]
        [HttpGet("v{version:apiVersion}/twins/{dtId}")]
        [Authorize]
        public async Task<IActionResult> GetTwinV2([FromRoute] string dtId, [FromQuery] bool includeModel)
        {
            var accessLevel = await GetTwinAccessLevel(dtId);

            if (accessLevel == TwinAccessLevel.Forbidden)
            {
                throw new UnauthorizedAccessException();
            }

            var twin = (JObject) await _digitalTwinApiService.GetTwin<object>(dtId);

            if (includeModel)
            {
                var modelId = twin["metadata"]["modelId"];
                var modelProperties = (JToken)await _digitalTwinApiService.GetModelPropertiesV2(modelId.ToString());
                twin.Add("model", modelProperties);
            }

            if (twin["etag"] != null)
            {
                Response.Headers.Append("etag", twin["etag"].ToString());
            }

            var response = new JObject
            {
                ["twin"] = twin,
                ["permissions"] = new JObject
                {
                    ["edit"] = accessLevel == TwinAccessLevel.Write
                }
            };

            return Content(response.ToString(), MediaTypeNames.Application.Json);
        }

        /// <summary>
        /// Get Twin information.
        /// </summary>
        /// <param name="dtId">Twin Id</param>
        /// <param name="request">Relationship request conditions</param>
        /// <returns>Twin relationships</returns>
        [MapToApiVersion("2.0")]
        [HttpGet("v{version:apiVersion}/twins/{dtId}/relationships")]
        [Authorize]
        public async Task<ActionResult<TwinRelationshipDto[]>> GetTwinRelationshipsV2(
            [FromRoute] string dtId,
            [FromQuery] TwinRelationshipsRequest request)
        {
            var accessLevel = await GetTwinAccessLevel(dtId);

            if (accessLevel == TwinAccessLevel.Forbidden)
            {
                throw new UnauthorizedAccessException();
            }

            return Ok(await _twinService.GetTwinRelationships(dtId, request));
        }

        /// <summary>
        /// Determine the access level for a twin.
        /// If twin has a SiteId, then check if the user has the ViewSites permission or TwinEdit permission
        /// If twin has no SiteId, then check if the user have ViewPortfolios permission or ManagePortfolios permission
        /// And when user has TwinEdit or ManagePortfolio permission, then return Write twin access level
        /// , if only ViewSites or ViewPortfolio permission, then return Read twin access level
        /// , otherwise, return Forbidden twin access level.
        /// </summary>
        /// <param name="dtId">Twin Id</param>
        /// <returns>The access level for the twin.</returns>
        private async Task<TwinAccessLevel> GetTwinAccessLevel(string dtId)
        {
            var twin = (JObject) await _digitalTwinApiService.GetTwin<object>(dtId);
            var userId = this.GetCurrentUserId();
            var siteId = twin.GetValue("siteID", StringComparison.InvariantCultureIgnoreCase)?.ToString();
            bool canEdit;
            if (siteId != null)
            {
                var siteIdGuid = Guid.Parse(siteId);
                if (!await _accessControl.CanAccessSite(userId, Permissions.ViewSites, siteIdGuid))
                {
                    return TwinAccessLevel.Forbidden;
                }
                canEdit = await _accessControl.CanAccessSite(userId, Permissions.ManageSites, siteIdGuid);
            }
            else
            {
                var customerId = Guid.Parse(_customerInstanceConfigurationOptions.Id);
                if (!await _accessControl.CanAccessCustomer(userId, Permissions.ViewPortfolios, customerId))
                {
                    return TwinAccessLevel.Forbidden;
                }
                canEdit = await _accessControl.CanAccessCustomer(userId, Permissions.ManagePortfolios, customerId);
            }
            return canEdit ? TwinAccessLevel.Write : TwinAccessLevel.Read;
        }

        /// <summary>
        /// Get Twin information.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="twinId"></param>
        /// <param name="includeModel"></param>
        /// <returns></returns>
        [MapToApiVersion("2.0")]
        [HttpGet("v{version:apiVersion}/sites/{siteId}/twins/{twinId}")]
        [Authorize]
        public async Task<IActionResult> GetTwinV2([FromRoute] Guid siteId, [FromRoute] string twinId, [FromQuery] bool includeModel)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var twin = await RetrieveTwin(siteId, twinId, includeModel);

            if (!siteId.Equals(Guid.Parse(twin.GetValue("siteID", StringComparison.InvariantCultureIgnoreCase).ToString())))
            {
                return BadRequest("Could not find this twin.");
            }

            var canEdit = await _accessControl.CanAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var response = new JObject
            {
                ["twin"] = twin,
                ["permissions"] = new JObject
                {
                    ["edit"] = canEdit
                }
            };

            return Content(response.ToString(), MediaTypeNames.Application.Json);
        }

        private async Task<JObject> RetrieveTwin(Guid siteId, string twinId, bool includeModel)
        {
            var twin = (JObject)await _digitalTwinApiService.GetTwin<object>(siteId, twinId);
            if (includeModel)
            {
                var modelId = twin["metadata"]["modelId"];
                var modelProperties = (JToken)await _digitalTwinApiService.GetModelProperties(siteId, modelId.ToString());
                twin.Add("model", modelProperties);
            }

            if (twin["etag"] != null)
            {
                Response.Headers.Add("etag", twin["etag"].ToString());
            }

            TwinHelper.Sanitize(twin);

            return twin;
        }

        /// <summary>
        /// Update Twin.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="twinId"></param>
        /// <param name="twin"></param>
        /// <returns></returns>
        [HttpPut("sites/{siteId}/twins/{twinId}")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> UpdateTwin([FromRoute] Guid siteId, [FromRoute] string twinId, [FromBody] TwinDto twin)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            twin.UserId = this.GetCurrentUserId().ToString();

            return Ok(await _digitalTwinApiService.UpdateTwin(siteId, twinId, twin));
        }

        /// <summary>
        /// Patch Twin.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="twinId"></param>
        /// <param name="jsonPatch"></param>
        /// <param name="ifMatch"></param>
        /// <returns></returns>
        [HttpPatch("sites/{siteId}/twins/{twinId}")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> PatchTwin(
            [FromRoute] Guid siteId,
            [FromRoute] string twinId,
            [FromBody] JsonPatch jsonPatch,
            [FromHeader(Name = "If-Match")] string ifMatch)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageSites, siteId);

            var validationErrors = ValidateJsonPatch(jsonPatch);
            if (validationErrors.Count != 0)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new ValidationError { Items = validationErrors });
            }

            await _digitalTwinApiService.PatchTwin(siteId, twinId, jsonPatch, ifMatch, this.GetCurrentUserId().ToString());

            return NoContent();
        }

        [HttpGet("sites/{siteId}/twins/readOnlyProperties")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> GetTwinsReadOnlyProperties([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            return Ok(TwinHelper.ReadOnly);
        }


        [HttpGet("sites/{siteId}/twins/{twinId}/restrictedFields")]
        [HttpGet("sites/{siteId}/twins/restrictedFields")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<RestrictedFieldsDto> GetTwinsRestrictedFields([FromRoute] Guid siteId, [FromRoute] string twinId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            TwinFieldsDto twinFields = null;
            if (!string.IsNullOrWhiteSpace(twinId))
            {
                twinFields = await _digitalTwinApiService.GetTwinFieldsAsync(siteId, twinId);
            }

            return RestrictedFieldsDto.MapFrom(twinFields);
        }

        /// <summary>
        /// Search for Twins across all sites the user has access to.
        /// </summary>
        /// <param name="request" cref="TwinSearchRequest">Search options. The scopeId is optional and only applies when the siteIds not provided</param>
        /// <returns>List of twins and a link to the next page</returns>
        [HttpGet("twins/search")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> SearchTwins([FromQuery] TwinSearchRequest request)
        {
            var scopeId = request.SiteIds?.Any() ?? false ? null : request.ScopeId; // if siteIds are requested, ignore scopeId
            request.SiteIds = (await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), scopeId, request.SiteIds)).ToArray();

            var result = await _digitalTwinApiService.Search(request);

            string nextPageUrl = null;
            if (result.NextPage > 0)
            {
                nextPageUrl = Url.Action("SearchTwins", new TwinSearchRequest
                {
                    Page = result.NextPage,
                    QueryId = result.QueryId,
                    Term = request.Term,
                    ModelId = request.ModelId,
                    SiteIds = request.SiteIds,
                    IsCapabilityOfModelId = request.IsCapabilityOfModelId
                });
            }

            var twinFloorIds = result.Twins.Where(x => x.FloorId.HasValue).Select(x => x.FloorId.Value).Distinct().ToList();
            if (twinFloorIds.Any())
            {
                var floors = await _floorService.GetFloorsAsync(twinFloorIds);
                foreach (var twin in result.Twins)
                {
                    twin.FloorName = floors.FirstOrDefault(x => x.Id == twin.FloorId)?.Name;
                }
            }

            var outRelationshipsFloorIds = result.Twins
                .SelectMany(x => x.OutRelationships)
                .Where(x => x.FloorId.HasValue)
                .Select(x => x.FloorId.Value)
                .Distinct()
                .ToList();

            if (outRelationshipsFloorIds.Any())
            {
                var outRelationshipsFloors = await _floorService.GetFloorsAsync(outRelationshipsFloorIds);

                foreach (var twin in result.Twins)
                {
                    foreach (var relationship in twin.OutRelationships)
                    {
                        relationship.FloorName = outRelationshipsFloors.FirstOrDefault(x => x.Id == relationship.FloorId)?.Name;
                    }
                }
            }

            return Ok(new
            {
                result.Twins,
                result.QueryId,
                nextPage = nextPageUrl
            });
        }

        /// <summary>
        /// Search for Twins using cognitive search across sites.
        /// </summary>
        /// <param name="request" cref="TwinCognitiveSearchRequest">Search
        /// options. The scopeId is optional and only applies when the
        /// siteIds not provided</param>
        /// <returns>List of twins</returns>
        [HttpGet("twins/cognitiveSearch")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> CognitiveSearchTwins(
            [FromQuery] TwinCognitiveSearchRequest request
        )
        {
            if (!_featureFlagService.IsFineGrainedAuthEnabled)
            {
                request.ScopeId = request.SiteIds?.Any() ?? false ? null : request.ScopeId;
                request.SiteIds = (
                    await _siteService.GetAuthorizedSiteIds(
                        this.GetCurrentUserId(),
                        request.ScopeId,
                        request.SiteIds
                    )
                ).ToArray();
            }

            var (twins, nextPage) = await _searchService.Search(request);

            if (twins.Count > 0 && request.SensorSearchEnabled)
            {
                var searchRequest = new CognitiveSearchRequest
                {
                    SensorSearchEnabled = true,
                    SiteIds = twins.Select(x => x.SiteId).Distinct(),
                    TwinIds = twins.Select(x => x.Id)
                };

                twins = [.. await _digitalTwinApiService.GetCognitiveSearchTwins(searchRequest)];
            }

            string nextPageUrl = null;
            if (nextPage > 0)
            {
                nextPageUrl = Url.Action(
                    "CognitiveSearchTwins",
                    new TwinCognitiveSearchRequest
                    {
                        Page = nextPage,
                        Term = request.Term,
                        ModelId = request.ModelId,
                        ScopeId = request.ScopeId,
                        SiteIds = request.SiteIds,
                        SensorSearchEnabled = request.SensorSearchEnabled
                    }
                );
            }

            return Ok(new { twins, nextPage = nextPageUrl });
        }

        [HttpGet("sites/{siteId}/twins/{twinId}/relationships")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<ActionResult<TwinRelationshipDto[]>> GetTwinRelationships(
            [FromRoute] Guid siteId,
            [FromRoute] string twinId,
            [FromQuery] TwinRelationshipsRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            return await _twinService.GetTwinRelationships(siteId, twinId, request);
        }

        /// <summary>
        /// Get all models
        /// </summary>
        /// <param name="siteId"></param>
        [HttpGet("sites/{siteId}/models")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public Task GetAllModels([FromRoute] Guid siteId)
        {
            _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            return this.ProxyToDownstreamService(
                ApiServiceNames.DigitalTwinCore,
                $"/admin/sites/{siteId}/models"
            );
        }

        /// <summary>
        /// Get model by Id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/models/{modelId}")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public Task GetModelById([FromRoute] Guid siteId, [FromRoute] string modelId)
        {
            _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            return this.ProxyToDownstreamService(ApiServiceNames.DigitalTwinCore, $"/admin/sites/{siteId}/models/{modelId}");
        }

        /// <summary>
        /// Export Twins data by twin ids
        /// </summary>
        /// <returns></returns>
        [HttpPost("twins/export")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> TwinsExport([FromBody] TwinExportRequest twinExportRequest)
        {
            twinExportRequest.Validate();

            var siteIds = twinExportRequest.Twins.Select(x => x.SiteId).Distinct().ToArray();

            await _accessControl.EnsureAccessSites(this.GetCurrentUserId(), Permissions.ViewSites, siteIds);

            var userSites = await _userAuthorizedSitesService.GetAuthorizedSites(this.GetCurrentUserId(), Permissions.ViewSites);

            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            await using var ms = new MemoryStream();

            await _twinService.ExportTwins(ms, twinExportRequest.QueryId, twinExportRequest.Twins, userSites.ToArray());

            return File(ms.ToArray(), "text/csv", fileName);
        }

        private List<ValidationErrorItem> ValidateJsonPatch(JsonPatch jsonPatch)
        {
            var validationErrors = new List<ValidationErrorItem>();
            var sources = jsonPatch.Operations.Select(x => x.Path.ToString()).ToList();

            sources.ForEach(source =>
            {
                var rootElement = source.Split("/", StringSplitOptions.RemoveEmptyEntries).First();

                if (TwinHelper.ReadOnly.Contains($"/{rootElement}") || TwinHelper.ReadOnly.Contains(source))
                {
                    validationErrors.Add(new ValidationErrorItem
                    {
                        Name = nameof(PatchOperation.Path),
                        Message = $"This property {source} is read-only."
                    });
                }
            });

            return validationErrors;
        }

        /// <summary>
        /// Retrieves the points for a twin
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="twinId"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/twins/{twinId}/points")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<ActionResult<List<PointDto>>> GetPoints([FromRoute] Guid siteId, [FromRoute] Guid twinId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var twinPoints = await _twinService.GetTwinPointsAsync(siteId, twinId);

            return twinPoints;
        }

        /// <summary>
        /// Download a twin file with the given twin id
        /// </summary>
        /// <param name="siteId">Site Id</param>
        /// <param name="twinId">Twin Id</param>
        /// <param name="inline">Determines the disposition type, True to show file as a web page, False to download
        /// as an attachment</param>
        /// <returns>Returns the file</returns>
        [HttpGet("sites/{siteId}/twins/{twinId}/download")]
        [Authorize]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> DownloadTwin([FromRoute] Guid siteId, [FromRoute] string twinId, [FromQuery] bool inline)
        {
	        await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

	        var twin = await RetrieveTwin(siteId, twinId, false);

	        var twinUrl = twin["url"]?.ToString() ?? throw new InvalidOperationException("url property missing");
	        var urlFilename = twinUrl.Split('/').Last();
	        var twinName = twin["name"]?.ToString() ?? throw new InvalidOperationException("name property missing");

	        var file = await _fileService.GetFileAsync(urlFilename);

	        var fileName = twinName
		        .Replace("\"", "", StringComparison.OrdinalIgnoreCase)
		        .Replace("\r", " ", StringComparison.OrdinalIgnoreCase)
		        .Replace("\n", " ", StringComparison.OrdinalIgnoreCase)
		        .Replace("\t", " ", StringComparison.OrdinalIgnoreCase)
		        .Replace("  ", " ", StringComparison.OrdinalIgnoreCase)
		        .Trim();
	        var fileNameNoExt = Path.GetFileNameWithoutExtension(fileName).Trim();

	        // Occasionally the twin name property is missing a file extension, although it is present on the url.
	        var extension = Path.GetExtension(urlFilename).Trim();

	        if (extension.StartsWith("."))
	        {
		        extension = $".{extension[1..].Trim()}";
	        }

            // Remove all the characters after and including the $ sign from the extension (the datetime stamp).
            // For example, if the extension is ".pdf$2405030859481862", it will be changed to ".pdf".
            var dollarIndex = extension.IndexOf('$');
            bool datetimeStampFound = dollarIndex > -1;
            if (datetimeStampFound)
            {
                extension = extension[..dollarIndex];
            }

	        var contentDisposition = new ContentDisposition
	        {
		        FileName = Uri.EscapeDataString(fileNameNoExt + extension),
		        Inline = inline
	        };

	        Response.Headers.Append("Content-Disposition", $"{contentDisposition}");
	        Response.Headers.Append("X-Content-Type-Options", "nosniff");

	        return File(file.Content, file.ContentType.MediaType ?? "application/octet-stream");
        }

        /// <summary>
        /// Returns the graph containing the given twin and its in and out edges, also the related nodes including the total number of edges in and out of it
        /// </summary>
        /// <param name="siteId">Site Id</param>
        /// <param name="twinId">Twin Id</param>
        /// <param name="useAdx">Flag to determine using which relationshipMap endpoint: true - relatedtwins, false - TwinsGraph</param>
        /// <returns>Related twins and edges traversed by hops</returns>
        [HttpGet("/sites/{siteId}/twins/{twinId}/relatedTwins")]
        [Authorize]
        public async Task<ActionResult<TwinGraphDto>> GetRelatedTwins([FromRoute] Guid siteId, [FromRoute] string twinId, [FromQuery] bool? useAdx)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            return (useAdx ?? true) ?
                await _digitalTwinApiService.GetRelatedTwins(siteId, twinId) :
                await _digitalTwinApiService.GetTwinsGraph(siteId, new [] { twinId });
        }

        [MapToApiVersion("2.0")]
        [HttpGet("v{version:apiVersion}/twins/{dtId}/relatedTwins")]
        [Authorize]
        public async Task<ActionResult<TwinGraphDto>> GetRelatedTwinsV2([FromRoute] string dtId)
        {
            var accessLevel = await GetTwinAccessLevel(dtId);

            if (accessLevel == TwinAccessLevel.Forbidden)
            {
                throw new UnauthorizedAccessException();
            }

            return await _digitalTwinApiService.GetRelatedTwins(dtId);
        }

        /// <summary>
        /// Retrieves all the revisions of the specified twin.
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="twinId"></param>
        /// <returns></returns>
        [HttpGet("/sites/{siteId}/twins/{twinId}/history")]
        [Authorize]
        public async Task<ActionResult<TwinHistoryDto>> GetTwinHistory([FromRoute] Guid siteId, [FromRoute] string twinId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            return await _digitalTwinApiService.GetTwinHistory(siteId, twinId);
        }

        /// <summary>
        /// Export Twins data for cognitive search
        /// </summary>
        /// <returns></returns>
        [HttpPost("twins/cognitiveSearch/export")]
        [MapToApiVersion("1.0")]
        [Authorize]
        public async Task<IActionResult> ExportCognitiveSearchTwins([FromBody] TwinCognitiveSearchRequest request)
        {
            request.ScopeId = request.SiteIds?.Any() ?? false ? null : request.ScopeId;
            request.SiteIds = (await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), request.ScopeId, siteIds: request.SiteIds)).ToArray();

            var searchRequest = new CognitiveSearchRequest();

            if (request.TwinIds.Any())
            {
                searchRequest.SiteIds = request.SiteIds;
                searchRequest.TwinIds = request.TwinIds;
            }
            else
            {
                request.Export = true;
                var result = await _searchService.Search(request);
                searchRequest.SiteIds = result.twins.Select(x => x.SiteId).Distinct();
                searchRequest.TwinIds = result.twins.Select(x => x.Id);
            }

            var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

            await using var streamToWrite = new MemoryStream();

            await _twinService.ExportCognitiveSearchTwins(streamToWrite, searchRequest);

            return File(streamToWrite.ToArray(), "text/csv", fileName);
        }

        /// <summary>
        /// Get twins in tree form.
        /// </summary>
        /// <remarks>
        /// If run in single tenant, uses the v2 version of the DigitalTwinCore endpoint which will always return
        /// cached data. In multi tenant, uses the old version of the DigitalTwinCore endpoint, so the behaviour will
        /// be the same, except we don't have any configurable parameters.
        /// </remarks>
        [HttpGet("v{version:apiVersion}/twins/tree")]
        [MapToApiVersion("2.0")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<NestedTwinDto>>> GetTreeV2()
        {
            if (_featureFlagService.IsFineGrainedAuthEnabled)
            {
                var fullTree = await _digitalTwinApiService.GetTreeV2Async();
                var prunedTree = await _twinTreeAuthEvaluator.GetPrunedTree(fullTree);
                return Ok(prunedTree);
            }

            var currentUserId = this.GetCurrentUserId();
            var userSites = await _directoryApi.GetUserSites(currentUserId, Permissions.ViewSites);
            var siteIds = userSites.Select(s => s.Id);

            return await _digitalTwinApiService.GetTreeV2Async(siteIds);
        }
    }
}
