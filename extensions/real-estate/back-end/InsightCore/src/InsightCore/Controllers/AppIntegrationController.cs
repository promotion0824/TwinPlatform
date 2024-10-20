using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using InsightCore.Dto;
using InsightCore.Services;
using System;
using InsightCore.Controllers.Requests;
using InsightCore.Models;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Http;
using Willow.Batch;
using InsightCore.Infrastructure.Extensions;
using System.Linq;

namespace InsightCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class AppIntegrationController : ControllerBase
    {
        private readonly IInsightService _insightService;
        public AppIntegrationController(IInsightService insightService)
        {
            _insightService = insightService;
        }

        [HttpGet("apps/{appId}/sites/{siteId}/insights")]
        [Authorize]
		[Obsolete]
        [ProducesResponseType(typeof(List<InsightDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSiteInsights([FromRoute] Guid appId, [FromRoute] Guid siteId, [FromQuery] OldInsightStatus[] statuses = null, [FromQuery] InsightState[] states = null)
        {
            var batchRequest = new BatchRequestDto()
            {
                FilterSpecifications = (new List<FilterSpecificationDto>()
                    .Upsert(nameof(Insight.SiteId), siteId)
                    .Upsert(nameof(Insight.Status), (statuses?.Length ?? 0) > 0 ? statuses.Convert() : null)
                    .Upsert(nameof(Insight.State), (states?.Length ?? 0) > 0 ? states.ToList() : null)
                    .Upsert(nameof(Insight.SourceId), appId)).ToArray()
            };

            // note that ignoring the global db filter will return insights with status Deleted and OccurrenceCount == 0
			var batchInsight = await _insightService.GetInsights(batchRequest, ignoreQueryFilters: true);

            // we want to keep the non-faulty insight (OccurrenceCount == 0) but exclude the deleted ones
            return Ok(batchInsight.Items.Where(x => x.LastStatus != InsightStatus.Deleted));
        }

        [HttpGet("apps/{appId}/sites/{siteId}/insights/{insightId}")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsight([FromRoute] Guid appId, [FromRoute] Guid siteId, [FromRoute] Guid insightId)
        {

            return Ok(await _insightService.GetInsight(insightId, ignoreQueryFilters: true));
        }

        [HttpPost("apps/{appId}/sites/{siteId}/insights")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDto), (int)HttpStatusCode.OK)]
		[SwaggerOperation("Create Insight", Tags = new[] { "AppIntegration" })]
        public async Task<IActionResult> CreateInsightViaApp([FromRoute] Guid appId, [FromRoute] Guid siteId, [FromBody] AppCreateInsightRequest appRequest)
        {
            var request = new CreateInsightRequest
            {
                CustomerId = appRequest.CustomerId,
                SequenceNumberPrefix = appRequest.SequenceNumberPrefix,
                Type = appRequest.Type,
                Name = appRequest.Name,
                Description = appRequest.Description,
                Recommendation = appRequest.Recommendation,
                ImpactScores = appRequest.ImpactScores,
                Priority = appRequest.Priority,
                Status = appRequest.Status,
                State = appRequest.State,
                OccurredDate = appRequest.OccurredDate,
                DetectedDate = appRequest.DetectedDate,
                SourceType = SourceType.App,
                SourceId = appId,
                ExternalId = appRequest.ExternalId,
                ExternalStatus = appRequest.ExternalStatus,
                ExternalMetadata = appRequest.ExternalMetadata,
                OccurrenceCount = appRequest.OccurrenceCount,
                AnalyticsProperties = appRequest.AnalyticsProperties,
				TwinId = appRequest.TwinId,
				RuleId= appRequest.RuleId,
				RuleName= appRequest.RuleName,
				PrimaryModelId = appRequest.PrimaryModelId,
				InsightOccurrences = appRequest.InsightOccurrences,
				Dependencies = appRequest.Dependencies,
                Points = appRequest.Points,
                Locations = appRequest.Locations,
                Tags = appRequest.Tags
		    };
			var insight = await _insightService.CreateInsight(siteId, request);
            return Ok(insight);
        }

        [HttpPut("apps/{appId}/sites/{siteId}/insights/{insightId}")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateInsight([FromRoute] Guid appId, [FromRoute] Guid siteId, [FromRoute] Guid insightId, [FromBody] UpdateInsightRequest request)
        {
	        request.SourceId = appId;
            return  Ok( await _insightService.UpdateInsightFromAppAsync(siteId, insightId, request));
        }

        [HttpDelete("apps/{appId}/sites/{siteId}/insights/{insightId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        public async Task<IActionResult> DeleteInsight([FromRoute] Guid appId, [FromRoute] Guid siteId, [FromRoute] Guid insightId)
         {
			await _insightService.UpdateInsight(siteId, insightId, new UpdateInsightRequest()
			{
				SourceId = appId,
				LastStatus = InsightStatus.Deleted
			}, ignoreQueryFilters: true);

            return NoContent();
        }

        [HttpPut("apps/{appId}/sites/{siteId}/insights/{insightId}/state")]
        [Authorize]
        [ProducesResponseType(typeof(InsightDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpdateInsightState([FromRoute] Guid appId, [FromRoute] Guid siteId, [FromRoute] Guid insightId, [FromBody] UpdateInsightStateRequest request)
        {
			var insight = await _insightService.UpdateInsight(siteId, insightId, new UpdateInsightRequest()
			{
				SourceId = appId,
				State = request.State
			},
            ignoreQueryFilters: true);

            return Ok(insight);
        }

		/// <summary>
		/// Get Status Log for an Insight
		/// </summary>
		/// <param name="appId"></param>
		/// <param name="siteId"></param>
		/// <param name="insightId"></param>
		/// <returns></returns>
		[HttpGet("apps/{appId}/sites/{siteId}/insights/{insightId}/StatusLog")]
		[Authorize]
		[ProducesResponseType(typeof(List<BaseStatusLogEntryDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
		[SwaggerOperation("Get Status Log for an Insight", Tags = new[] { "AppIntegration" })]
		public async Task<IActionResult> GetInsightStatusLog([FromRoute] Guid siteId, [FromRoute] Guid insightId)
		{
			var statusLog = await _insightService.GetInsightStatusLog(insightId, siteId);
			if (statusLog is null)
			{
				return NotFound("Insight not found");
			}
			else
			{
				return Ok(BaseStatusLogEntryDto.MapFromModels(statusLog));
			}
		}
	}
}
