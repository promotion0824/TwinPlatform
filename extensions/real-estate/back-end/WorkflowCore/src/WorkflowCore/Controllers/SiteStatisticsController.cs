using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.Infrastructure.Exceptions;
using WorkflowCore.Dto;
using WorkflowCore.Services;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class SiteStatisticsController :ControllerBase
    {
        private readonly ISiteStatisticsService _siteStatisticsService;

        public SiteStatisticsController(ISiteStatisticsService siteStatisticsService)
        {
            _siteStatisticsService = siteStatisticsService;
        }

        [HttpGet("siteStatistics")]
        [Authorize]
        [ProducesResponseType(typeof(List<SiteStatisticsDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSiteStatistics([FromQuery] Guid[] siteIds)
        {
            if (siteIds == null || siteIds.Length <= 0)
            {
                throw new ArgumentNullException($"The siteIds are empty");
            }
            var siteStatisticsList = await _siteStatisticsService.GetSiteStatisticsList(siteIds);
            return Ok(SiteStatisticsDto.MapFromModels(siteStatisticsList));
        }

        [HttpGet("statistics/site/{siteId}")]
        [Authorize]
        [ProducesResponseType(typeof(List<SiteStatisticsDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSiteTicketStatistics(Guid siteId, [FromQuery] string floorId = null)
        {
            var siteStatistics = await _siteStatisticsService.GetSiteStatistics(siteId, floorId);

            return Ok(SiteStatisticsDto.MapFromModel(siteStatistics));
        }

        [HttpGet("ticketStatisticsByStatus/sites/{siteId}")]
        [Authorize]
        [ProducesResponseType(typeof(List<SiteTicketStatisticsByStatusDto>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSiteTicketStatisticsByStatus(Guid siteId)
        {
            var siteTicketStatisticsByStatus = await _siteStatisticsService.GetSiteTicketStatisticsByStatus(siteId);

            return Ok(SiteTicketStatisticsByStatusDto.MapFromModel(siteTicketStatisticsByStatus));
        }
    }
}
