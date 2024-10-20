using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Logging;
using Willow.Platform.Statistics;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.InsightApi;
using Willow.Platform.Models;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Features.Insights;

namespace PlatformPortalXL.Features.Controllers
{
    [ApiController]
    [Authorize]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class StatisticsController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IControllerHelper _controllerHelper;
        private readonly IStatisticsService _service;
        private readonly ITwinStatisticsService _twinStatisticsService;
        private readonly IInsightApiService _insightApiService;
        private readonly IInsightService _insightService;
        private readonly ITicketService _ticketService;
        private readonly ILogger<StatisticsController> _logger;

        public StatisticsController(IAccessControlService accessControl,
                                    IControllerHelper controllerHelper,
                                    IStatisticsService statsService,
                                    ITwinStatisticsService twinStatisticsService,
                                    IInsightApiService insightApiService,
                                    IInsightService insightService,
                                    ITicketService ticketService,
	                                ILogger<StatisticsController> logger)
        {
            _accessControl     = accessControl ?? throw new ArgumentNullException(nameof(accessControl));
            _controllerHelper  = controllerHelper ?? throw new ArgumentNullException(nameof(controllerHelper));
            _service           = statsService ?? throw new ArgumentNullException(nameof(statsService));
            _twinStatisticsService = twinStatisticsService ?? throw new ArgumentNullException(nameof(twinStatisticsService));
			_insightApiService = insightApiService ?? throw new ArgumentNullException(nameof(insightApiService));
			_logger            = logger ?? throw new ArgumentNullException(nameof(logger));
            _insightService = insightService ?? throw new ArgumentNullException(nameof(insightService));
            _ticketService = ticketService ?? throw new ArgumentNullException(nameof(ticketService));
        }

		[Obsolete]
        [HttpGet("statistics/insights/site/{siteId}")]
        [ProducesResponseType(typeof(InsightsStats), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns insights statistics for a site", Tags = new [] { "Statistics" })]
        public async Task<InsightsStats> GetSiteInsightsStatistics(Guid siteId, [FromQuery] string floorId = null)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ViewSites, siteId);

            try
            {
                var siteStats = await _insightApiService.GetInsightStatisticsBySiteIds(new List<Guid>{ siteId} );
                return SiteInsightStatistics.MapTo(siteStats?.StatisticsByPriority?.FirstOrDefault(c => c.Id == siteId));
            }
            catch(Exception ex)
            {
                _logger.LogError("Unable to retrieve site insights statistics", ex, new { SiteId = siteId });
                throw;
            }
        }

        [Obsolete("Not used in Willow App, to be deprecated")]
        [HttpGet("statistics/insights/customer/{customerId}/portfolio/{portfolioId}")]
        [ProducesResponseType(typeof(InsightsStats), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns insights statistics for a portfolio", Tags = new [] { "Statistics" })]
        public async Task<InsightsStats> GetPortfolioInsightsStatistics(Guid customerId, Guid portfolioId)
        {
            await _accessControl.EnsureAccessPortfolio(_controllerHelper.GetCurrentUserId(this), Permissions.ViewPortfolios, portfolioId);

            try
            {
                return await _service.GetPortfolioInsights(customerId, portfolioId);
            }
            catch(Exception ex)
            {
                _logger.LogError("Unable to retrieve portfolio insights statistics", ex, new { CustomerId = customerId, PortfolioId = portfolioId });
                throw;
            }
        }

        [HttpGet("statistics/tickets/site/{siteId}")]
        [ProducesResponseType(typeof(TicketStats), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns ticket statistics for a site", Tags = new [] { "Statistics" })]
        public async Task<TicketStats> GetSiteTicketStatistics(Guid siteId, [FromQuery] string floorId = null)
        {
            await _accessControl.EnsureAccessSite(_controllerHelper.GetCurrentUserId(this), Permissions.ViewSites, siteId);

            try
            {
                return await _service.GetSiteTickets(new SiteStatisticsRequest { SiteId = siteId, FloorId = floorId } );
            }
            catch(Exception ex)
            {
                _logger.LogError("Unable to retrieve site ticket statistics", ex, new { SiteId = siteId });
                throw;
            }
        }

        [Obsolete("Not used in Willow App, to be deprecated")]
        [HttpGet("statistics/tickets/customer/{customerId}/portfolio/{portfolioId}")]
        [ProducesResponseType(typeof(TicketStats), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns ticket statistics for a portfolio", Tags = new [] { "Statistics" })]
        public async Task<TicketStats> GetPortfolioTicketStatistics(Guid customerId, Guid portfolioId)
        {
            await _accessControl.EnsureAccessPortfolio(_controllerHelper.GetCurrentUserId(this), Permissions.ViewPortfolios, portfolioId);

            try
            {
                return await _service.GetPortfolioTickets(customerId, portfolioId);
            }
            catch(Exception ex)
            {
                _logger.LogError("Unable to retrieve portfolio ticket statistics", ex, new { CustomerId = customerId, PortfolioId = portfolioId });
                throw;
            }
        }

        /// <summary>
        /// Returns Insight statistics for Twins with geometryViewerIds for a given site/floor/moduleTypeName(s)
        /// </summary>
        /// <returns>Returns Insight statistics for Twins with geometryViewerIds for a given site/floor/moduleTypeName(s)</returns>
        [HttpPost("statistics/assets/insight")]
        [Authorize]
        [ProducesResponseType(typeof(TwinInsightStatisticsResponseDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns geometryViewerIds of the insights and tickets for the given floor", Tags = new[] { "Floors" })]
        public async Task<ActionResult<List<TwinInsightStatisticsResponseDto>>> GetTwinInsightStatistics([FromBody] InsightTwinStatisticsRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, request.SiteId);

            var statistics = await _insightService.GetTwinsWithGeometryIdInsightStatisticsAsync(request);

            return Ok(statistics);
        }

        /// <summary>
        /// Returns Tickets statistics for Twins with geometryViewerIds for a given site/floor/moduleTypeName(s)
        /// </summary>
        /// <returns>Returns Tickets statistics for Twins with geometryViewerIds for a given site/floor/moduleTypeName(s)</returns>
        [HttpPost("statistics/assets/tickets")]
        [Authorize]
        [ProducesResponseType(typeof(TwinInsightStatisticsResponseDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns geometryViewerIds of the insights and tickets for the given floor", Tags = new[] { "Floors" })]
        public async Task<ActionResult<List<TwinTicketStatisticsResponseDto>>> GetTwinTicketsStatistics([FromBody] TicketTwinStatisticsRequest request)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, request.SiteId);

            var statistics = await _ticketService.GetTwinTicketStatisticsAsync(request);

            return Ok(statistics);
        }

        [HttpPost("statistics/twins")]
        [Authorize]
        [ProducesResponseType(typeof(StatisticsResponse), StatusCodes.Status200OK)]
        [SwaggerOperation("Returns selected statistics for the specified twins", Tags = new[] { "Statistics" })]
        public async Task<ActionResult<List<StatisticsResponse>>> GetTwinTicketsStatistics([FromBody] StatisticsRequest request)
        {
            var statistics = await _twinStatisticsService.GetTwinStatisticsAsync(request);
            return Ok(statistics);
        }
    }
}
