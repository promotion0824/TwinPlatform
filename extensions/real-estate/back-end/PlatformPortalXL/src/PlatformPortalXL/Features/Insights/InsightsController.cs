using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.InsightApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using PlatformPortalXL.Extensions;
using PlatformPortalXL.Services.Twins;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using Willow.Common;
using Willow.Batch;
using PlatformPortalXL.Requests.InsightCore;
using PlatformPortalXL.Services.Sites;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Features.Insights
{
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class InsightsController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly ILogger<InsightsController> _logger;
        private readonly IInsightApiService _insightApi;
        private readonly IDigitalTwinApiService _digitalTwinApiService;
        private readonly ISiteApiService _siteApi;
        private readonly IDirectoryApiService _directoryApi;
        private readonly IInsightService _insightService;
        private readonly ITwinService _twinService;
        private readonly ISiteService _siteService;
        private readonly IUserAuthorizedSitesService _userAuthorizedSitesService;

        public InsightsController(
            IAccessControlService accessControl,
            ILogger<InsightsController> logger,
            IInsightApiService insightApi,
            ISiteApiService siteApi,
            IDirectoryApiService directoryApi,
            IDigitalTwinApiService digitalTwinApiService,
            IInsightService insightService,
            ITwinService twinService,
            ISiteService siteService,
            IUserAuthorizedSitesService userAuthorizedSitesService)
        {
            _accessControl = accessControl;
            _logger = logger;
            _insightApi = insightApi;
            _digitalTwinApiService = digitalTwinApiService;
            _directoryApi = directoryApi;
            _siteApi = siteApi;
            _insightService = insightService;
            _twinService = twinService;
            _siteService = siteService;
            _userAuthorizedSitesService = userAuthorizedSitesService;
        }

        [Obsolete(message: "Use POST /insights with BatchRequestDto")]
        [MapToApiVersion("1.0")]
        [HttpGet("insights")]
        [Authorize]
        [ProducesResponseType(typeof(List<InsightSimpleDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of insights for all sites", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightsAsync([FromQuery] GetInsightsRequest request)
        {
            var insightsDto = new List<InsightSimpleDto>();

            var userId = this.GetCurrentUserId();
            var sites = (await _userAuthorizedSitesService.GetAuthorizedSites(userId, Permissions.ViewSites))
                .Where(x => (!request.SiteId.HasValue || request.SiteId.Value == x.Id) && !x.Features.IsInsightsDisabled).ToList();

            if (sites.Any())
            {
                var insightBatchRequest = new BatchRequestDto
                {
                    FilterSpecifications = (new List<FilterSpecificationDto>()
                            .Upsert(nameof(Insight.SiteId), sites.Select(x => x.Id))
                            .Upsert(nameof(Insight.Status), request.Tab.MapTo())
                            .Upsert(nameof(Insight.SourceType), request.SourceType)
                            .Upsert(nameof(Insight.Type), FilterOperators.NotEquals, InsightType.Diagnostic)
                            .Upsert(nameof(Insight.CreatedDate), FilterOperators.GreaterThanOrEqual,
                                request.CreatedDateFrom)
                            .Upsert("LastOccurredDate", FilterOperators.GreaterThanOrEqual, request.LastOccurredDateFrom))
                        .ToArray()
                };
                var insights = await _insightApi.GetInsights(insightBatchRequest, true);

                insightsDto = await MapInsightsToDtos(insights?.Items?.ToList(), true);
                insightsDto = await _insightService.GetInsightStatistics(insightsDto);
            }

            return Ok(insightsDto);
        }

        [Obsolete(message: "Use POST /insights with BatchRequestDto")]
        [MapToApiVersion("1.0")]
        [HttpGet("sites/{siteId}/insights")]
        [Authorize]
        [ProducesResponseType(typeof(List<InsightSimpleDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of insights", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightsAsync([FromRoute] Guid siteId, [FromQuery, BindRequired] InsightListTab tab, [FromQuery] InsightSourceType? sourceType = null)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var insightsDto = new List<InsightSimpleDto>();

            var userId = this.GetCurrentUserId();
            var sites = (await _userAuthorizedSitesService.GetAuthorizedSites(userId, Permissions.ViewSites))
                .Where(x => x.Id == siteId && !x.Features.IsInsightsDisabled);

            if (sites.Any())
            {
                var insightBatchRequest = new BatchRequestDto
                {
                    FilterSpecifications = (new List<FilterSpecificationDto>()
                        .Upsert(nameof(Insight.SiteId), sites.Select(x => x.Id))
                        .Upsert(nameof(Insight.Status), tab.MapTo())
                        .Upsert(nameof(Insight.SourceType), sourceType)
                        .Upsert(nameof(Insight.Type), FilterOperators.NotEquals, InsightType.Diagnostic)
                        .ToArray())
                };
                var insights = await _insightApi.GetInsights(insightBatchRequest, true);

                insightsDto = await MapInsightsToDtos(insights?.Items?.ToList(), true);
            }

            return Ok(insightsDto);
        }

        [MapToApiVersion("1.0")]
        [HttpPost("insights")]
        [Authorize]
        [ProducesResponseType(typeof(BatchDto<InsightSimpleDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a batch of insights for all sites", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightsAsync([FromBody] BatchRequestDto request)
        {
            var siteIds = await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), request.FilterSpecifications.GetScopeId(), predicate: x => !x.Features.IsInsightsDisabled);

            request.FilterSpecifications = await request.FilterSpecifications
                .Upsert(nameof(Insight.Type), FilterOperators.NotEquals, InsightType.Diagnostic)
                .UpsertScopeIdFilter(siteIds);

            var insights = await _insightApi.GetInsights(request, true);

            var insightsDto = await MapInsightsToDtos(insights?.Items?.ToList(), true);
            insightsDto = await _insightService.GetInsightStatistics(insightsDto);

            return Ok(new BatchDto<InsightSimpleDto>
            {
                Items = insightsDto?.ToArray(),
                After = insights?.After ?? 0,
                Before = insights?.Before ?? 0,
                Total = insights?.Total ?? 0
            });
        }

        [MapToApiVersion("1.0")]
        [HttpPost("insights/all")]
        [Authorize]
        [ProducesResponseType(typeof(BatchDto<InsightsDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a batch of insight that match the given filter specification.", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightsAllAsync([FromBody] BatchRequestDto request)
        {
            var siteIds = await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), request.FilterSpecifications.GetScopeId(), predicate: x => !x.Features.IsInsightsDisabled);

            request.FilterSpecifications = await request.FilterSpecifications
                .RenameFilter(InsightFilterNames.DetailedStatus, nameof(Insight.Status))
                .Upsert(nameof(Insight.Type), FilterOperators.NotEquals, InsightType.Diagnostic)
                .UpsertScopeIdFilter(siteIds)
                .UpsertActivityFilter(_insightService.GetSiteInsightStatistics(siteIds));

            var insightsTask = _insightApi.GetInsights(request);
            var impactScoreSummaryTask = _insightApi.GetImpactScoresSummary(request);

            await Task.WhenAll(insightsTask, impactScoreSummaryTask);

            var insights = await insightsTask;
            var impactScoreSummary = await impactScoreSummaryTask;

            var insightSimpleDto = await MapInsightsToDtos(insights?.Items?.ToList());
            insightSimpleDto = await _insightService.GetInsightStatistics(insightSimpleDto);

            var batchedInsightSimpleDto = new BatchDto<InsightSimpleDto>
            {
                Items = insightSimpleDto?.ToArray(),
                After = insights?.After ?? 0,
                Before = insights?.Before ?? 0,
                Total = insights?.Total ?? 0
            };

            var insightsDto = new InsightsDto()
            {
                Insights = batchedInsightSimpleDto,
                ImpactScoreSummary = impactScoreSummary
            };

            return Ok(insightsDto);
        }

        [MapToApiVersion("1.0")]
        [HttpPost("insights/filters")]
        [Authorize]
        [ProducesResponseType(typeof(InsightFilterDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets insight filters.", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightsFilterAsync([FromBody] GetInsightFilterRequest request)
        {
            var userId = this.GetCurrentUserId();
            var siteIds = await _siteService.GetAuthorizedSiteIds(userId, request.ScopeId, siteIds: request.SiteIds, x => !x.Features.IsInsightsDisabled);
            var insightFilter = await _insightApi.GetInsightsFilterAsync(new GetInsightFilterApiRequest
            {
                SiteIds = siteIds,
                StatusList = request.StatusList
            });
            return Ok(insightFilter);
        }

        [MapToApiVersion("1.0")]
        [HttpPost("insights/cards")]
        [Authorize]
        [ProducesResponseType(typeof(InsightsCardsDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a batch of insight cards that match the given filter specification.", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightCardsAsync([FromBody] BatchRequestDto request)
        {
            var siteIds = await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), request.FilterSpecifications.GetScopeId(), predicate: x => !x.Features.IsInsightsDisabled);

            request.FilterSpecifications = await request.FilterSpecifications
                .Upsert(nameof(Insight.Type), FilterOperators.NotEquals, InsightType.Diagnostic)
                .UpsertScopeIdFilter(siteIds);

            var insightCards = await _insightApi.GetInsightCards(request);
            var impactScoreSummary = new List<ImpactScore>();

            if (insightCards.Items?.Any(x => x.ImpactScores.Any()) ?? false)
            {
                impactScoreSummary = insightCards.Items
                    .SelectMany(x => x.ImpactScores)
                    .GroupBy(x => x.FieldId)
                    .ToDictionary(x => x.Key, x => new ImpactScore
                    {
                        FieldId = x.Key,
                        Name = x.Max(y => y.Name),
                        Value = x.Sum(y => y.Value),
                        Unit = x.Max(y => y.Unit)
                    }).Select(x => x.Value).ToList();
            }

            var insightCardsDto = new InsightsCardsDto()
            {
                Cards = insightCards,
                ImpactScoreSummary = impactScoreSummary
            };

            return Ok(insightCardsDto);
        }

        [Obsolete("Use the endpoint without SiteId, insights/{id}")]
        [MapToApiVersion("1.0")]
        [HttpGet("sites/{siteId}/insights/{insightId}")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDetailDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a specific insight", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsight([FromRoute] Guid siteId, [FromRoute] Guid insightId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var insight = await _insightApi.GetInsight(siteId, insightId);
            var insightDetailDto = await MapInsightAndGetFloorDetails(siteId, insight);
            if (insight.CreatedUserId.HasValue)
            {
                insightDetailDto.CreatedUser = await _directoryApi.GetUser(insight.CreatedUserId.Value);
            }

            return Ok(insightDetailDto);
        }

        [MapToApiVersion("1.0")]
        [HttpGet("insights/{insightId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDetailDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a specific insight", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsight([FromRoute] Guid insightId, [FromQuery] string scopeId = "")
        {
            var insight = await _insightApi.GetInsight(insightId);

            await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), scopeId, new List<Guid>() { insight.SiteId }, x => !x.Features.IsInsightsDisabled);

            var insightDetailDto = await MapInsightAndGetFloorDetails(insight.SiteId, insight);
            if (insight.CreatedUserId.HasValue)
            {
                insightDetailDto.CreatedUser = await _directoryApi.GetUser(insight.CreatedUserId.Value);
            }

            return Ok(insightDetailDto);
        }

        /// <summary>
        /// Updates the status of the given insight
        /// </summary>
        /// <param name="siteId">the site id for the insight</param>
        /// <param name="insightId">the id for the insight</param>
        /// <param name="request"> the requested status for the insight</param>
        /// <returns></returns>
        [MapToApiVersion("2.0")]
        [HttpPut("v{version:apiVersion}/sites/{siteId}/insights/{insightId}/status")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDetailDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Updates the status of the given insight", Tags = new[] { "Insights" })]
        public async Task<ActionResult> UpdateInsightStatus([FromRoute] Guid siteId, [FromRoute] Guid insightId, [FromBody] UpdateInsightStatusRequest request)
        {
            var currentUserId = this.GetCurrentUserId();
            await CanUpdateInsightStatus(currentUserId, siteId, new List<Guid> { insightId }, request.Status.Value, request.ScopeId);

            var insight = await _insightApi.UpdateInsightAsync(siteId, insightId, new UpdateInsightRequest
            {
                UpdatedByUserId = currentUserId,
                LastStatus = request.Status,
                Reason = request.Reason
            });

            var insightDetailDto = InsightDetailDto.MapFromModel(insight);
            return Ok(insightDetailDto);
        }

        /// <summary>
        /// Updates the status of the given insights
        /// </summary>
        /// <param name="siteId">the id for the insights' site</param>
        /// <param name="request">list of the insight Ids and the requested status</param>
        /// <returns></returns>
        [MapToApiVersion("2.0")]
        [HttpPost("v{version:apiVersion}/sites/{siteId}/insights/status")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Updates the status of the given insights", Tags = new[] { "Insights" })]
        public async Task<ActionResult> UpdateInsightsStatus([FromRoute] Guid siteId, [FromBody] UpdateInsightStatusRequest request)
        {
            var currentUserId = this.GetCurrentUserId();
            await CanUpdateInsightStatus(currentUserId, siteId, request.Ids, request.Status.Value, request.ScopeId);
            await _insightApi.UpdateBatchInsightStatusAsync(siteId, new BatchUpdateInsightStatusRequest
            {
                Ids = request.Ids,
                Status = request.Status.Value,
                Reason = request.Reason,
                UpdatedByUserId = currentUserId
            });
            return NoContent();
        }

        [Obsolete("The insight status has changed, please use the V2")]
        [MapToApiVersion("1.0")]
        [HttpPost("sites/{siteId}/insights/status")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Updates the status of the given insights", Tags = new[] { "Insights" })]
        public async Task<ActionResult> UpdateInsightsStatus([FromRoute] Guid siteId, [FromBody] UpdateOldInsightsStatusRequest request)
        {
            var currentUserId = this.GetCurrentUserId();
            await _accessControl.EnsureAccessSite(currentUserId, Permissions.ViewSites, siteId);
            await _insightApi.UpdateBatchInsightStatusAsync(siteId, new BatchUpdateInsightStatusRequest
            {
                Ids = request.Ids,
                Status = (InsightStatus)((int)request.Status),
                UpdatedByUserId = currentUserId
            });
            return NoContent();
        }

        /// <summary>
        /// Get Insight's Occurrences
        /// </summary>
        /// <param name="siteId"> the id of the requested site</param>
        /// <param name="insightId">the id for the requested insight</param>
        /// <returns>Returns the occurrences of the given insights</returns>
        [MapToApiVersion("1.0")]
        [HttpGet("sites/{siteId}/insights/{insightId}/occurrences")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation("Returns the occurrences of the given insights", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightOccurrences([FromRoute] Guid siteId, [FromRoute] Guid insightId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);
            return Ok(await _insightService.GetInsightOccurrencesAsync(insightId));
        }

        [MapToApiVersion("1.0")]
        [Authorize]
        [HttpGet("sites/{siteId}/insights/{insightId}/activities")]
        [ProducesResponseType(typeof(List<InsightActivityDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
        [SwaggerOperation("Get insight activities", Tags = new[] { "Insights" })]
        public async Task<IActionResult> GetInsightActivities([FromRoute] Guid siteId, [FromRoute] Guid insightId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);
            var activities = await _insightService.GetInsightActivitiesAsync(siteId, insightId);
            return Ok(activities);

        }

        [MapToApiVersion("1.0")]
        [HttpGet("insights/sources")]
        [Authorize]
        [ProducesResponseType(typeof(List<InsightSourceDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of all available insight sources", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightsSourcesAsync([FromQuery] string scopeId = null)
        {
            var siteIds = await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), scopeId,predicate: x => !x.Features.IsInsightsDisabled);
            return Ok(await _insightApi.GetInsightSourcesAsync(siteIds));
        }

        [MapToApiVersion("1.0")]
        [HttpGet("insights/types")]
        [Authorize]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of all available insight types", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightTypesAsync([FromQuery] string scopeId = null)
        {
            await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), scopeId,predicate: x => !x.Features.IsInsightsDisabled);
            return Ok(Enum.GetNames(typeof(InsightType)).ToList());
        }

        [MapToApiVersion("1.0")]
        [HttpGet("insights/statuses")]
        [Authorize]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of all available insight statuses", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightStatusesAsync([FromQuery] string scopeId = null)
        {
            await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), scopeId, predicate: x => !x.Features.IsInsightsDisabled);
            return Ok(Enum.GetNames(typeof(InsightStatus)).ToList());
        }

        [MapToApiVersion("1.0")]
        [HttpGet("sites/{siteId}/insights/{insightId}/points")]
        [Authorize]
        [ProducesResponseType(typeof(InsightPointsDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get insight points", Tags = new[] { "Insights" })]
        public async Task<IActionResult> GetInsightPoints([FromRoute] Guid siteId, [FromRoute] Guid insightId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);
            var result = await _insightService.GetInsightPointsAsync(siteId, insightId);
            return Ok(result);
        }

        /// <summary>
        /// Get Insight impact scores live data by external id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="externalId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="selectedInterval"></param>
        /// /// <param name="scopeId"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/livedata/impactScores/{externalId}")]
        [Authorize]
        [ProducesResponseType(typeof(ImpactScoresLiveData), StatusCodes.Status200OK)]
        [SwaggerOperation("Get Insight impact scores live data by external id", Tags = new[] { "Insights" })]
        public async Task<ActionResult<ImpactScoresLiveData>> GetLiveDataByExternalId(
         [FromRoute] Guid siteId,
         [FromRoute] string externalId,
         [FromQuery(Name = "start"), BindRequired] DateTime start,
         [FromQuery(Name = "end"), BindRequired] DateTime end,
         [FromQuery(Name = "interval")] string selectedInterval,
         [FromQuery] string scopeId = null)
        {
            var siteIds = await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), scopeId, new List<Guid>() { siteId }, x => !x.Features.IsInsightsDisabled);
            var site = await _siteApi.GetSite(siteIds.First());
            var result = await _insightService.GetLiveDataByExternalId(site.CustomerId, externalId, start, end, selectedInterval);

            return Ok(result);
        }
        /// <summary>
        /// Get list of the Insights for map view - It is for DFW demo
        /// </summary>
        /// <returns>Insight list for DFW demo</returns>
        [MapToApiVersion("1.0")]
        [HttpGet("insights/mapview")]
        [Authorize]
        [ProducesResponseType(typeof(InsightMapViewDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Get Insight list for the map view", Tags = new[] { "Insights" })]
        public async Task<IActionResult> GetInsightListForMapView([FromQuery] string scopeId = null)
        {
            var siteIds = await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), scopeId, predicate: x => !x.Features.IsInsightsDisabled);

            var insights = await _insightApi.GetInsightListForMapViewAsync(siteIds);

            var result = new List<InsightMapViewDto>();

            if (insights != null && insights.Any())
            {
                result.AddRange(insights.Select(model => InsightMapViewDto.MapFromModel(model, model.GetEcmDependency(insights))));
            }

            return Ok(result);
        }

        /// <summary>
        /// Return diagnostic data for a given insight
        /// </summary>
        /// <param name="insightId">The id for the given insight</param>
        /// <param name="start">Start date filter</param>
        /// <param name="end">End date filter</param>
        /// <param name="interval">the requested timeseries interval</param>
        /// <param name="scopeId">the id for the scope</param>
        /// <returns>List of insight's diagnostic data</returns>
        [HttpGet("insights/{insightId}/occurrences/diagnostics")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDiagnosticDto), StatusCodes.Status200OK)]
        [SwaggerOperation("diagnostic data for a given insight", Tags = new[] { "Insights" })]
        public async Task<IActionResult> GetInsightDiagnosticAsync([FromRoute] Guid insightId, [FromQuery][Required] DateTime start,
            [FromQuery][Required] DateTime end, [FromQuery] string interval, [FromQuery] string scopeId = null)
        {
            await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), scopeId, predicate: x => !x.Features.IsInsightsDisabled);

            var result = await _insightApi.GetInsightDiagnosticAsync(insightId, start, end, interval);
            return Ok(result);
        }

        /// <summary>
        /// Return diagnostic snapshot data for a given insight
        /// </summary>
        /// <param name="insightId">The id for the given insight</param>
        /// <returns>List of insight's diagnostic data</returns>
        [HttpGet("insights/{insightId}/diagnostics/snapshot")]
        [Authorize]
        [ProducesResponseType(typeof(DiagnosticsSnapshotDto), StatusCodes.Status200OK)]
        [SwaggerOperation("diagnostic snapshot for a given insight", Tags = new[] { "Insights" })]
        public async Task<IActionResult> GetDiagnosticsSnapshot([FromRoute] Guid insightId)
        {
            await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), predicate: x => !x.Features.IsInsightsDisabled);

            var result = await _insightApi.GetDiagnosticsSnapshot(insightId);

            return Ok(result);
        }

        /// <summary>
        /// returns insight priority and status statistics by user's siteIds
        /// </summary>
        /// <param name="scopeIds">Ids of the requested insight's locations</param>
        /// <returns>List of status and priority insight statistics</returns>
        [HttpPost("insights/statistics")]
        [Authorize]
        [ProducesResponseType(typeof(InsightStatisticsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserInsightStatistics([FromBody]  List<string> scopeIds)
        {
            var siteIds = await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), scopeIds, predicate: x => !x.Features.IsInsightsDisabled);
            return Ok(await _insightApi.GetInsightStatisticsBySiteIds(siteIds));
        }

        /// <summary>
        /// returns insight priority and status statistics by user's siteIds
        /// </summary>
        /// <returns>List of status and priority insight statistics</returns>
        [HttpPost("insights/snackbars/status")]
        [Authorize]
        [ProducesResponseType(typeof(List<InsightSnackbarByStatus>), StatusCodes.Status200OK)]
        [SwaggerOperation("Retrieves insights status snackbars", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightSnackbarsByStatus([FromBody] IEnumerable<FilterSpecificationDto> filters)
        {
            var siteIds = await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), predicate: x => !x.Features.IsInsightsDisabled);

            filters = filters
                .Upsert(nameof(Insight.SiteId), siteIds);

            // limit query to the last 24 hours unless otherwise specified
            if (filters.FirstOrDefault(nameof(Insight.UpdatedDate)) == null)
            {
                filters = filters.Upsert(nameof(Insight.UpdatedDate), FilterOperators.GreaterThanOrEqual, DateTime.UtcNow.AddDays(-1));
            }

            return Ok(await _insightApi.GetInsightSnackbarsByStatus(filters));
        }

        /// <summary>
        /// An array containing one entry for each date in the range requested, and the count of all insight occurrences that occurred on each date.
        /// </summary>
        /// <param name="spaceTwinId">the space/location twinId</param>
        /// <param name="startDate">the start date range for insight occurrences</param>
        /// <param name="endDate">the end date range for insight occurrences</param>
        /// <returns>An array containing one entry for each date in the range requested</returns>
        [HttpGet("insights/twin/{spaceTwinId}/insightOccurrencesByDate")]
        [Authorize]
        [ProducesResponseType(typeof(InsightOccurrencesCountByDateResponse), StatusCodes.Status200OK)]
        [SwaggerOperation("Get Insight occurrences by date", Tags = new[] { "Insights" })]
        public async Task<ActionResult> GetInsightOccurrencesByDate([FromRoute] string spaceTwinId, DateTime startDate, DateTime endDate)
        {
            if (startDate == default(DateTime) || endDate == default(DateTime))
            {
                return BadRequest("Start date and end date are required");
            }
            if (startDate > endDate)
            {
                return BadRequest("Start date cannot be greater than end date");
            }
            return Ok(await _insightApi.GetInsightOccurrencesByDateAsync(spaceTwinId, startDate, endDate));
        }

        /// <summary>
        ///  returns most active insights by twin model
        /// </summary>
        /// <param name="spaceTwinId">the space/location twinId</param>
        /// <param name="limit">the limit of number of items(models)</param>
        [HttpGet("insights/twin/{spaceTwinId}/activeInsightCountsByTwinModel")]
        [Authorize]
        [ProducesResponseType(typeof(List<ActiveInsightCountByModelIdDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Get active Insight count by model id", Tags = new[] { "Insights" })]
        public async Task<IActionResult> GetActiveInsightByModelId([FromRoute] string spaceTwinId, [FromQuery] int limit = 5)
        {
            return Ok(await _insightApi.GetActiveInsightByModelId(spaceTwinId, limit <= 0 ? 5 : limit));
        }

        private async Task<List<InsightSimpleDto>> MapInsightsToDtos(List<Insight> models, bool addFloor = false)
        {
            var result = new List<InsightSimpleDto>();

            if (models == null || !models.Any())
                return result;

            result.AddRange(models.Select(model => InsightSimpleDto.MapFromModel(model, model.GetEcmDependency(models))));
            if(addFloor)
                await _insightService.MapFloorData(result);

            return result;
        }

        private async Task<InsightDetailDto> MapInsightAndGetFloorDetails(Guid siteId, Insight insight)
        {
            var insightDetailDto = InsightDetailDto.MapFromModel(insight);
			if (insightDetailDto.FloorId == null && !string.IsNullOrWhiteSpace(insight.FloorCode))
            {
                var floor = await _siteApi.GetSiteFloorByIdOrCode(siteId, insight.FloorCode);
                insightDetailDto.FloorId = floor?.Id;
            }
            else if (insightDetailDto.FloorId != null && string.IsNullOrWhiteSpace(insight.FloorCode))
            {
                var floor = await _siteApi.GetSiteFloorByIdOrCode(siteId, insightDetailDto.FloorId.ToString());
                insightDetailDto.FloorCode = floor?.Code;
            }

            return insightDetailDto;
        }

        private async Task CanUpdateInsightStatus(Guid currentUserId, Guid siteId, List<Guid> insightIds, InsightStatus requestStatus,string scopeId)
        {
	        if (requestStatus == InsightStatus.Deleted)
	        {
		        var canDelete = await _accessControl.IsWillowUser(currentUserId);
		        if (!canDelete)
			        throw new UnauthorizedAccessException().WithData(new { currentUserId, siteId, insightIds, requestStatus });
	        }

            await _siteService.GetAuthorizedSiteIds(currentUserId, scopeId, new List<Guid>() { siteId }, x => !x.Features.IsInsightsDisabled);
        }
    }
}
