using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Controllers.Responses;
using WorkflowCore.Dto;
using WorkflowCore.Repository;
using WorkflowCore.Services;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class InsightStatisticsController :ControllerBase
    {
        private readonly IInsightStatisticsService _insightStatisticsService;
		private readonly IAuditTrailService _auditTrailService;
		private readonly ICommentsService _commentsService;

		public InsightStatisticsController(IInsightStatisticsService insightStatisticsService, IAuditTrailRepository auditTrailRepository, ICommentsService commentsService, IAuditTrailService auditTrailService)
		{
			_insightStatisticsService = insightStatisticsService;
			_commentsService = commentsService;
			_auditTrailService = auditTrailService;
		}

		[HttpPost("insightStatistics")]
        [Authorize]
        [ProducesResponseType(typeof(List<InsightStatisticsDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsightStatistics(GetInsightStatisticsRequest request)
        {
            if (request.InsightIds == null || request.InsightIds.Count <= 0)
            {
                throw new ArgumentNullException($"The insightIds are empty");
            }
            var insightStatisticsList = await _insightStatisticsService.GetInsightStatisticsList(request.InsightIds, request.Statuses, request.Scheduled);
            return Ok(InsightStatisticsDto.MapFromModels(insightStatisticsList));
        }

        [HttpPost("siteinsightStatistics")]
        [Authorize]
        [ProducesResponseType(typeof(List<InsightStatisticsDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSiteInsightStatistics(GetSiteInsightStatisticsRequest request)
        {
            if (request.SiteIds == null || request.SiteIds.Count <= 0)
            {
                throw new ArgumentNullException($"The siteIds are empty");
            }
            var siteInsightStatisticsList = await _insightStatisticsService.GetSiteInsightStatisticsList(request.SiteIds, request.Statuses, request.Scheduled);
            return Ok(InsightStatisticsDto.MapFromModels(siteInsightStatisticsList));
        }

        /// <summary>
        /// Returns true if the insight has open ticket
        /// </summary>
        /// <param name="insightId">the id for the insight</param>
        /// <returns>returns true if the insight's tickets that are not closed</returns>
        [HttpGet("insights/{insightId}/tickets/open")]
        [Authorize]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsightOpenTicketStatistics([FromRoute] Guid insightId)
        {
	        return Ok(await _insightStatisticsService.HasInsightOpenTicketsAsync(insightId));
        }
		/// <summary>
		/// Get ticket activities for an insight
		/// </summary>
		/// <param name="insightId"></param>
		/// <returns></returns>
		[HttpGet("insights/{insightId}/tickets/activities")]
		[Authorize]
		[ProducesResponseType(typeof(List<TicketActivityResponse>), (int)HttpStatusCode.OK)]
		[SwaggerOperation("Get insight's tickets activities", Tags = new[] { "InsightStatistics" })]
		public async Task<IActionResult> GetInsightTicketsActivities([FromRoute] Guid insightId)
		{
			var ticketLog = await _auditTrailService.GetInsightTicketActivitiesAsync(insightId);
			var ticketComments = await _commentsService.GetInsightTicketCommentsAsync(insightId);
			var result = TicketActivityResponse.MapFromTicketActivities(ticketLog);
			result.AddRange(TicketActivityResponse.MapFromTicketActivities(ticketComments));

			return Ok(result);
		}
    }
}
