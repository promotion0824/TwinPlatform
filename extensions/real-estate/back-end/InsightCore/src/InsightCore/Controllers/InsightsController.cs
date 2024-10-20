using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using InsightCore.Dto;
using InsightCore.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using InsightCore.Controllers.Requests;
using Microsoft.AspNetCore.Authorization;
using Willow.Batch;
using Swashbuckle.AspNetCore.Annotations;
using InsightCore.Models;

namespace InsightCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class InsightsController : ControllerBase
    {
        private readonly IInsightService _insightService;
        public InsightsController(IInsightService insightService)
        {
            _insightService = insightService;
        }


        [HttpPost("sites/insights/impactscores/summary")]
        [HttpPost("insights/impactscores/summary")]
        [Authorize]
        public async Task<ActionResult<List<ImpactScore>>> GetImpactScoresSummary([FromBody] BatchRequestDto request)
        {
             return await _insightService.GetImpactScoresSummary(request);
        }

        [HttpPost("sites/insights/cards")]
        [HttpPost("insights/cards")]
        [Authorize]
        public async Task<ActionResult<BatchDto<InsightCardDto>>> GetInsightsCards([FromBody] BatchRequestDto request)
        {
           return await _insightService.GetInsightCards(request);


        }

        [HttpPost("sites/insights")]
        [HttpPost("insights")]
        [Authorize]
        public async Task<ActionResult<BatchDto<InsightDto>>> GetInsights([FromBody] BatchRequestDto request, [FromQuery] bool addFloor=false)
        {
            return await _insightService.GetInsights(request,addFloor);
           
        }

        /// <summary>
        /// Returns list of the insight occurrences
        /// </summary>
        /// <param name="insightId">the id of the insight</param>
        /// <returns>List of the insightOccurrencesDto</returns>
        [HttpGet("insights/{insightId}/occurrences")]
        [Authorize]
        [ProducesResponseType(typeof(List<InsightOccurrenceDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsightOccurrencesAsync([FromRoute] Guid insightId)
        {

            return Ok(await _insightService.GetInsightOccurrencesAsync(insightId));

        }

        [Obsolete("Use the endpoint without SiteId, insights/{id}")]
        [HttpGet("sites/{siteId}/insights/{insightId}")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsight([FromRoute] Guid siteId, [FromRoute] Guid insightId)
        {
            return Ok( await _insightService.GetInsight(insightId));
            
        }

        [HttpGet("insights/{insightId}")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsight([FromRoute] Guid insightId)
        {
            return Ok(await _insightService.GetInsight(insightId));
            
        }

        [HttpPost("sites/{siteId}/insights")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateInsight([FromRoute] Guid siteId, [FromBody] CreateInsightRequest request)
        {
            return Ok(await _insightService.CreateInsight(siteId, request));
        }

        [HttpPut("sites/{siteId}/insights/{insightId}")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateInsight([FromRoute] Guid siteId, [FromRoute] Guid insightId, [FromBody] UpdateInsightRequest request)
        {
            var insight = await _insightService.UpdateInsight(siteId, insightId, request, ignoreQueryFilters: true);
            if (insight == null || insight.LastStatus == InsightStatus.Deleted)
            {
                return NoContent();
            }
            return Ok(insight);
        }

        /// <summary>
        /// Batch endpoint to update status for list of insights
        /// </summary>
        /// <param name="siteId">the site id for the insights</param>
        /// <param name="request">the requested status and list of the insights</param>
        /// <returns>Update the insights</returns>
        [HttpPut("sites/{siteId}/insights/status")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> BatchUpdateInsightStatus([FromRoute] Guid siteId, [FromBody] BatchUpdateInsightStatusRequest request)
        {
            await _insightService.BatchUpdateInsightStatusAsync(siteId, request);
            return NoContent();
        }

        /// <summary>
        /// Get Status Log for an Insight
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="insightId"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/insights/{insightId}/StatusLog")]
        [Authorize]
        [ProducesResponseType(typeof(List<StatusLogDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(List<StatusLogDto>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetInsightStatusLog([FromRoute] Guid siteId, [FromRoute] Guid insightId)
        {
            var statusLog = await _insightService.GetInsightStatusLog(insightId, siteId);
            if (statusLog is null)
            {
                return NotFound("The insight does not exist");
            }
            return Ok(StatusLogDto.MapFrom(statusLog));

        }
        /// <summary>
        /// Get points for an insight
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="insightId"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/insights/{insightId}/points")]
        [Authorize]
        [ProducesResponseType(typeof(InsightPointsDto), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Get points for an insight", Tags = new[] { "Insights" })]
        public async Task<IActionResult> GetInsightPoints([FromRoute] Guid siteId, [FromRoute] Guid insightId)
        {
            var insightPoints = await _insightService.GetPointsAsync(siteId, insightId);

            return Ok(insightPoints);

        }

        /// <summary>
        /// Get insight activities
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="insightId"></param>
        /// <returns></returns>
        [HttpGet("sites/{siteId}/insights/{insightId}/activities")]
        [Authorize]
        [ProducesResponseType(typeof(List<InsightActivityDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(List<InsightActivityDto>), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetInsightActivities([FromRoute] Guid siteId, [FromRoute] Guid insightId)
        {
            var activities = await _insightService.GetInsightActivities(siteId, insightId);
            if (activities is null)
            {
                return NotFound("The insight does not exist");
            }
            var activitiesDto = InsightActivityDto.MapFrom(activities, _insightService.GetSourceName);
            return Ok(activitiesDto);

        }

        /// <summary>
        /// Get list of the Insights for map view - It is for DFW demo
        /// </summary>
        /// <returns>Insight list for DFW demo</returns>
        [HttpGet("insights/mapview")]
        [Authorize]
        [ProducesResponseType(typeof(InsightMapViewDto), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Get Insight list for the map view", Tags = new[] { "Insights" })]
        public async Task<IActionResult> GetInsightListForMapView([FromQuery] List<Guid> siteIds)
        {
            List<InsightMapViewDto> result = null;
            if (siteIds.Any())
            {
                result = await _insightService.GetInsightListForMapViewAsync(siteIds);
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
        /// <returns>List of insight's diagnostic data</returns>
        [HttpGet("insights/{insightId}/occurrences/diagnostics")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDiagnosticDto), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Get diagnostic data for a given insight", Tags = new[] { "Insights" })]
        public async Task<IActionResult> GetInsightDiagnosticAsync([FromRoute] Guid insightId, [FromQuery][Required] DateTime start,
            [FromQuery][Required] DateTime end, [FromQuery] string interval)
        {

            var requestedInterval = string.IsNullOrWhiteSpace(interval) ||
                           !TimeSpan.TryParse(interval, CultureInfo.InvariantCulture, out var parsedInterval)
                ?(end>start?(end-start).TotalMinutes:1)
                : parsedInterval.TotalMinutes;
            return Ok(await _insightService.GetInsightDiagnosticAsync(insightId, start, end, requestedInterval));
        }

        /// <summary>
        /// Return diagnostics snapshot given an insight - name, last faulty occurrence timestamp, and dependencies
        /// </summary>
        /// <param name="insightId"></param>
        /// <returns></returns>
        [HttpGet("insights/{insightId}/diagnostics/snapshot")]
        [Authorize]
        public async Task<ActionResult<DiagnosticsSnapshotDto>> GetDiagnosticsSnapshot([FromRoute] Guid insightId)
        {
            return await _insightService.GetDiagnosticsSnapshot(insightId);
        }

        /// <summary>
        /// Get list of the Insights for filter
        /// </summary>
        /// <returns>Insight filters for requested sites</returns>
        [HttpPost("insights/filters")]
        [Authorize]
        [ProducesResponseType(typeof(InsightFilterDto), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Get Insight filter for sites", Tags = new[] { "Insights" })]
        public async Task<IActionResult> GetInsightFilters([FromBody] GetInsightFilterRequest request)
        {
           InsightFilterDto result = null;
            if (request.SiteIds!=null && request.SiteIds.Any())
            {
                result = await _insightService.GetInsightFiltersAsync(request);
            }
            return Ok(result);
        }

        [HttpGet("sources")]
        [Authorize]
        [ProducesResponseType(typeof(List<InsightSourceDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsightSources()
        {
             return Ok(await _insightService.GetInsightSources());
           
        }
    }
}
