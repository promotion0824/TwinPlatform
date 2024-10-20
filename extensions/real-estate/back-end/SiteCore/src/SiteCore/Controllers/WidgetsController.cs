using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SiteCore.Domain;
using SiteCore.Dto;
using SiteCore.Requests;
using SiteCore.Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SiteCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class WidgetsController : ControllerBase
    {
        private readonly IWidgetService _widgetService;
        private readonly ISiteService _siteService;

        public WidgetsController(IWidgetService widgetService, ISiteService siteService)
        {
            _widgetService = widgetService;
            _siteService = siteService;
        }

        #region Scope Widget

        [HttpGet("scopes/{scopeId}/widgets")]
        [Authorize]
        [ProducesResponseType(typeof(List<Widget>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets scope widgets")]
        public async Task<IActionResult> GetScopeWidgets(string scopeId)
        {
            var widgets = await _widgetService.GetWidgetsByScopeId(scopeId);
            return Ok(widgets);
        }

        [HttpPost("scopes/{scopeId}/widgets")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Adds a widget to scope")]
        public async Task<IActionResult> AddWidgetToScope([FromRoute] string scopeId, [FromBody] AddWidgetRequest request)
        {
            await _widgetService.AddWidgetToScope(scopeId, request.WidgetId, request.Position.ToString());
            return Ok();
        }

        [HttpDelete("scopes/{scopeId}/widgets/{widgetId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Removes a widget from scope")]
        public async Task<IActionResult> DeleteWidgetFromScope([FromRoute] string scopeId, [FromRoute] Guid widgetId)
        {
            await _widgetService.DeleteWidgetFromScope(scopeId, widgetId);
            return Ok();
        }

        #endregion

        #region Site Widget
        [HttpGet("sites/{siteId}/widgets")]
        [Authorize]
        [ProducesResponseType(typeof(List<Widget>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets site widgets")]
        public async Task<IActionResult> GetSiteWidgets(Guid siteId)
        {
            var widgets = await _widgetService.GetWidgetsBySiteId(siteId);
            return Ok(widgets);
        }

        [HttpPost("sites/{siteId}/widgets")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Adds a widget to site")]
        public async Task<IActionResult> AddWidgetToSite([FromRoute] Guid siteId, [FromBody] AddWidgetRequest request)
        {
            await _widgetService.AddWidgetToSite(siteId, request.WidgetId, request.Position.ToString());
            return Ok();
        }

        [HttpDelete("sites/{siteId}/widgets/{widgetId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Removes a widget from site")]
        public async Task<IActionResult> DeleteWidgetFromSite([FromRoute] Guid siteId, [FromRoute] Guid widgetId)
        {
            await _widgetService.DeleteWidgetFromSite(siteId, widgetId);
            return Ok();
        }
        #endregion

        #region Portfolio Widget
        [HttpGet("portfolios/{portfolioId}/widgets")]
        [Authorize]
        [ProducesResponseType(typeof(List<Widget>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets portfolio widgets")]
        public async Task<IActionResult> GetPortfolioWidgets([FromRoute] Guid portfolioId, [FromQuery] bool? includeSiteWidgets)
        {
            var widgets = await _widgetService.GetWidgetsByPortfolioId(portfolioId);

            if (includeSiteWidgets ?? false)
            {
                var sites = await _siteService.GetSitesForPortfolio(portfolioId);
                var siteWidgets = new List<Widget>();
                foreach (var site in sites)
                {
                    siteWidgets.AddRange(await _widgetService.GetWidgetsBySiteId(site.Id));
                }

                widgets.AddRange(siteWidgets.GroupBy(x => x.Id).Select(x => new Widget()
                {
                    Id = x.Key,
                    Metadata = x.First().Metadata,
                    Type = x.First().Type,
                    Positions = x.SelectMany(y => y.Positions)
                }).ToList());
            }

            return Ok(widgets);
        }

        [HttpPost("portfolios/{portfolioId}/widgets")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Adds a widget to portfolio")]
        public async Task<IActionResult> AddWidgetToPortfolio([FromRoute] Guid portfolioId, [FromBody] AddWidgetRequest request)
        {
            await _widgetService.AddWidgetToPortfolio(portfolioId, request.WidgetId, request.Position);
            return Ok();
        }

        [HttpDelete("portfolios/{portfolioId}/widgets/{widgetId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Removes a widget from portfolio")]
        public async Task<IActionResult> DeleteWidgetFromPortfolio([FromRoute] Guid portfolioId, [FromRoute] Guid widgetId)
        {
            await _widgetService.DeleteWidgetFromPortfolio(portfolioId, widgetId);
            return Ok();
        }
        #endregion

        #region Widget CRUD
        [HttpGet("internal-management/widgets")]
        [Authorize]
        [ProducesResponseType(typeof(List<Widget>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets all widgets")]
        public async Task<IActionResult> GetAllWidgets([FromQuery] Guid[] widgetIds)
        {
            List<Widget> widgets;
            if (widgetIds == null || widgetIds.Length <= 0)
            {
                widgets = await _widgetService.GetAllWidgets();
            }
            else
            {
                widgets = new List<Widget>();
                foreach (var widgetId in widgetIds)
                {
                    var widget = await _widgetService.GetWidget(widgetId);
                    widgets.Add(widget);
                }
            }

            return Ok(widgets);
        }

        [HttpGet("internal-management/widgets/{widgetId}")]
        [Authorize]
        [ProducesResponseType(typeof(Widget), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Gets a widget by id")]
        public async Task<ActionResult<Widget>> Get([FromRoute] Guid widgetId)
        {
            var widget = await _widgetService.GetWidget(widgetId);
            return Ok(widget);
        }

        [HttpPost("internal-management/widgets")]
        [Authorize]
        [ProducesResponseType(typeof(Widget), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation("Creates a new widget")]
        public async Task<IActionResult> Create([FromBody] CreateUpdateWidgetRequest request)
        {
            var widget = await _widgetService.CreateWidget(request);
            return Ok(widget);
        }

        [HttpPut("internal-management/widgets/{widgetId}")]
        [Authorize]
        [ProducesResponseType(typeof(Widget), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Updates a widget")]
        public async Task<IActionResult> Update([FromRoute] Guid widgetId, [FromBody] CreateUpdateWidgetRequest request)
        {
            var widget = await _widgetService.UpdateWidget(widgetId, request);
            return Ok(widget);
        }

        [HttpDelete("internal-management/widgets/{widgetId}")]
        [Authorize]
        [SwaggerOperation("Deletes a widget")]
        public async Task<IActionResult> Delete([FromRoute] Guid widgetId, [FromQuery] bool? resetLinked)
        {
            await _widgetService.DeleteWidget(widgetId, resetLinked);
            return Ok();
        }
        #endregion
    }
}
