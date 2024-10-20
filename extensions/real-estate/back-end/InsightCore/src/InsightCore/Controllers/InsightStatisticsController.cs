using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InsightCore.Dto;
using InsightCore.Services;
using Willow.Batch;
using InsightCore.Controllers.Requests;

namespace InsightCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class InsightStatisticsController : ControllerBase
    {
        private readonly IInsightStatisticsService _insightStatisticsService;
        public InsightStatisticsController(IInsightStatisticsService insightStatisticsService)
        {
            _insightStatisticsService = insightStatisticsService;
        }


        /// <summary>
        /// returns insight priority and status statistics by siteIds
        /// </summary>
        /// <param name="siteIds"></param>
        /// <returns>List of status and priority insight statistics</returns>
        [HttpPost("insights/statistics")]
        [Authorize]
        [ProducesResponseType(typeof(List<InsightStatisticsResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsightStatisticsBySiteIds([FromBody] List<Guid> siteIds)
        {
            return Ok(await _insightStatisticsService.GetInsightStatisticsBySiteIds(siteIds));
        }

        /// <summary>
        /// returns insight priority and count statistics by twinIds
        /// </summary>
        /// <param name="twinIds"></param>
        /// <returns>List of insight priority and count statistics by twinIds</returns>
        [HttpPost("insights/twins/statistics")]
        [Authorize]
        [ProducesResponseType(typeof(List<TwinInsightStatisticsDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsightStatisticsByTwinIds([FromBody] TwinInsightStatisticsRequest request)
        {
            return Ok(await _insightStatisticsService.GetInsightStatisticsByTwinIds(request));
        }

        /// <summary>
        /// returns insight priority and status statistics
        /// </summary>
        /// <param name="filters"></param>
        /// <returns>List of status and priority insight statistics</returns>
        [HttpPost("insights/snackbars/status")]
        [Authorize]
        [ProducesResponseType(typeof(List<InsightStatisticsResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsightSnackbarsByStatus([FromBody] IEnumerable<FilterSpecificationDto> filters)
        {
            return Ok(await _insightStatisticsService.GetInsightSnackbarsByStatus(filters));
        }


        /// <summary>
        /// returns count of all insight occurrences that occurred on each date
        /// </summary>
        /// <param name="spaceTwinId">the space/location twinId</param>
        /// <param name="startDate">the start date range for insight occurrences</param>
        /// <param name="endDate">the end date range for insight occurrences</param>
        /// <returns>An array containing one entry for each date in the range requested</returns>
        [HttpGet("insights/twin/{spaceTwinId}/insightOccurrencesByDate")]
        [Authorize]
        [ProducesResponseType(typeof(InsightOccurrencesCountByDateResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetInsightOccurrencesByDate([FromRoute] string spaceTwinId,DateTime startDate, DateTime endDate)
        {
            if(startDate == default(DateTime) || endDate == default(DateTime))
            {
                return BadRequest("Start date and end date are required");
            }
            if(startDate > endDate)
            {
                return BadRequest("Start date cannot be greater than end date");
            }
            return Ok(await _insightStatisticsService.GetInsightOccurrencesByDate(spaceTwinId,startDate,endDate));
        }

        /// <summary>
        ///  returns most active insights by twin model
        /// </summary>
        /// <param name="spaceTwinId">the space/location twinId</param>
        /// <param name="limit">the limit of number of items(models)</param>
        /// <returns>An array containing the model id, and a count of how many active insights are associated with this twin model.</returns>
        [HttpGet("insights/twin/{spaceTwinId}/activeInsightCountsByTwinModel")]
        [Authorize]
        [ProducesResponseType(typeof(List<ActiveInsightByModelIdDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetActiveInsightByModelId([FromRoute] string spaceTwinId, [FromQuery]int limit)
        {
            return Ok(await _insightStatisticsService.GetActiveInsightByModelId(spaceTwinId, limit));
        }
    }
}
