using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using PlatformPortalXL.Services.Sites;
using PlatformPortalXL.Auth.Permissions;

namespace PlatformPortalXL.Features.Dashboard
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class DashboardController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IWidgetApiService _widgetApi;
        private readonly ISiteService _siteService; // temporary needed for scope authorization

        public DashboardController(IAccessControlService accessControl, IWidgetApiService widgetApi, ISiteService siteService)
        {
            _accessControl = accessControl;
            _widgetApi = widgetApi;
            _siteService = siteService;
        }

        [HttpGet("scopes/{scopeId}/dashboard")]
        [Authorize]
        [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets dashboard data", Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<DashboardDto>> GetDashboardForScope([FromRoute] string scopeId)
        {
            await _siteService.GetAuthorizedSiteIds(this.GetCurrentUserId(), scopeId);

            var widgets = await _widgetApi.GetWidgetsByScopeId(scopeId);

            var dashboardDto = new DashboardDto
            {
                Widgets = WidgetDto.Map(widgets)
            };

            return Ok(dashboardDto);
        }

        [HttpGet("sites/{siteId}/dashboard")]
        [Authorize]
        [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets dashboard data", Tags = new [] { "Dashboard" })]
        public async Task<ActionResult<DashboardDto>> GetDashboardForSite([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var widgets = await _widgetApi.GetWidgetsBySiteId(siteId);

            var dashboardDto = new DashboardDto
            {
                Widgets = WidgetDto.Map(widgets)
            };

            return Ok(dashboardDto);
        }

        [HttpGet("portfolios/{portfolioId}/dashboard")]
        [Authorize]
        [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets portfolio dashboard data", Tags = new [] { "Dashboard" })]
        public async Task<ActionResult<DashboardDto>> GetDashboardForPortfolio([FromRoute] Guid portfolioId, [FromQuery]bool? includeSiteWidgets)
        {
            await _accessControl.EnsureAccessPortfolio(this.GetCurrentUserId(), new CanViewDashboards(), Permissions.ViewPortfolios, portfolioId);

            var widgets = await _widgetApi.GetWidgetsByPortfolioId(portfolioId, includeSiteWidgets);

            var dashboardDto = new DashboardDto
            {
                Widgets = WidgetDto.Map(widgets)
            };

            return Ok(dashboardDto);
        }

        [HttpPost("/dashboard")]
        [Authorize]
        [ProducesResponseType(typeof(WidgetDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Creates a widget", Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<WidgetDto>> CreateWidget([FromBody] CreateUpdateWidgetRequest request)
        {
            var widget = await _widgetApi.CreateWidget(request);

            return Ok(WidgetDto.Map(widget));
        }

        [HttpPut("/dashboard/{widgetId}")]
        [Authorize]
        [ProducesResponseType(typeof(WidgetDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Updates a widget", Tags = new[] { "Dashboard" })]
        public async Task<ActionResult<WidgetDto>> UpdateWidget([FromRoute] Guid widgetId,[FromBody] CreateUpdateWidgetRequest request)
        {
            var widget = await _widgetApi.UpdateWidget(widgetId, request);

            return Ok(WidgetDto.Map(widget));
        }

        [HttpDelete("/dashboard/{widgetId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Deletes a widget", Tags = new[] { "Dashboard" })]
        public async Task<ActionResult> DeleteWidget([FromRoute] Guid widgetId, [FromQuery] bool? resetLinked)
        {
            await _widgetApi.DeleteWidget(widgetId, resetLinked);

            return NoContent();
        }
    }
}
