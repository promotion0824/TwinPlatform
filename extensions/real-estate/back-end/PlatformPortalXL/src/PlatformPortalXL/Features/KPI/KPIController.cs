using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using KPI.Service.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Auth.Permissions;
using PlatformPortalXL.Features.Pilot;
using PlatformPortalXL.Http;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DigitalTwinApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using Willow.ExceptionHandling.Exceptions;
using Willow.KPI.Service;

namespace PlatformPortalXL.Features.KPI;

[ApiController]
[Authorize]
[Produces("application/json")]
public class KPIController : ControllerBase
{
    private readonly IKPIServiceFactory _svcFactory;
    private readonly IAccessControlService _accessControl;
    private readonly IResiliencePipelineService _resiliencePipelineService;
    private readonly IHttpRequestHeaders _headers;
    private readonly IDigitalTwinApiService _dtApiService;
    private readonly ISiteApiService _siteApiService;

    public KPIController(
        IKPIServiceFactory svcFactory,
        IAccessControlService accessControl,
        IResiliencePipelineService resiliencePipelineService,
        IHttpRequestHeaders headers,
        IDigitalTwinApiService dtApiService,
        ISiteApiService siteApiService)
    {
        _svcFactory = svcFactory;
        _accessControl = accessControl;
        _resiliencePipelineService = resiliencePipelineService;
        _headers = headers;
        _dtApiService = dtApiService;
        _siteApiService = siteApiService;
    }

    [HttpPost("kpi/{viewName}")]
    [ProducesResponseType(typeof(List<Willow.KPI.Service.Metric>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<List<Willow.KPI.Service.Metric>> Get([FromRoute] string viewName, [FromBody] KPIRequest filter = null)
    {
        var portfolioIdHdr = _headers.Get(HttpContext, "PortfolioId", true);
        var customerIdHdr  = _headers.Get(HttpContext, "CustomerId", true);

        if(!Guid.TryParse(portfolioIdHdr, out Guid portfolioId))
            throw new Exception("PortfolioId header is not a valid guid");

        if(!Guid.TryParse(customerIdHdr, out Guid customerId))
            throw new Exception("CustomerId header is not a valid guid");

        this.TryGetCurrentUserId(out Guid userId);

        await _accessControl.EnsureAccessPortfolio(userId, new CanViewDashboards(), Permissions.ViewPortfolios, portfolioId);

        var svc = _svcFactory.GetService(customerId);

        var result = await _resiliencePipelineService.ExecuteAsync(async _ =>
            await svc.GetByMetric(portfolioId, viewName, filter?.ToDictionary()));

        return result.ToList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="spaceTwinId">The target building twinId</param>
    /// <param name="kpiScore"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost("twin/{spaceTwinId}/kpi/{kpiScore}")]
    [ProducesResponseType(typeof(List<Willow.KPI.Service.Metric>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [SwaggerOperation("Retrieves the target kpiScores for the targeted twin (building only).", Tags = new[] { "KPI" })]
    public async Task<DatedKPIValuesWithTrendResponse> GetBuildingDatedComfortScoresWithTrend([FromRoute] string spaceTwinId, [FromRoute] string kpiScore, [FromBody] KPIRequest filter = null)
    {
        var portfolioIdHdr = _headers.Get(HttpContext, "PortfolioId", true);
        var customerIdHdr = _headers.Get(HttpContext, "CustomerId", true);

        if (!Guid.TryParse(portfolioIdHdr, out Guid portfolioId))
            throw new Exception("PortfolioId header is not a valid guid");

        if (!Guid.TryParse(customerIdHdr, out Guid customerId))
            throw new Exception("CustomerId header is not a valid guid");
        if (filter==null ||  !filter.StartDate.HasValue || !filter.EndDate.HasValue)
        {
            throw new BadRequestException("The startDate and the endDate are required.");
        }

        this.TryGetCurrentUserId(out Guid userId);

        await _accessControl.EnsureAccessPortfolio(userId, Permissions.ViewPortfolios, portfolioId);

        var svc = _svcFactory.GetService(customerId);

        var result = await _resiliencePipelineService.ExecuteAsync(async _ =>
            await svc.GetDatedBuildingScore(portfolioId, spaceTwinId, kpiScore, filter));

        return result;
    }

    /// <summary>
    ///  Demo endpoint to Explore BI Solutions in React 
    /// </summary>
    /// <param name="scopeId">The target building twinId</param>
    /// <param name="viewName"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost("twin/{scopeId}/Bi/kpi/{viewName}")]
    [ProducesResponseType(typeof(List<Willow.KPI.Service.Metric>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    [SwaggerOperation("Retrieves the target kpiScores for the targeted twin For BI demo.", Tags = new[] { "KPI" })]
    public async Task<ActionResult> GetBuildingDashboardData([FromRoute] string scopeId, [FromRoute] string viewName, [FromBody] KPIRequest filter = null)
    {
        var customerIdHdr = _headers.Get(HttpContext, "CustomerId", true);

        if (!Guid.TryParse(customerIdHdr, out Guid customerId))
            throw new Exception("CustomerId header is not a valid guid");
        if (!Enum.TryParse(viewName, true, out KPIViewNames kpiView))
        {
            throw new BadRequestException("The viewName is not valid.");
        }

        var svc = _svcFactory.GetService(customerId);

        var result = await _resiliencePipelineService.ExecuteAsync(async _ =>
            await svc.GetBuildingDashboardData( scopeId, kpiView, filter));

        return Ok(result);
    }


    /// <summary>
    /// Get the building performance scores
    /// </summary>
    /// <param name="scopeId"> scopeId is the building twinId</param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpGet("twin/{scopeId}/kpi/performanceScoresByDate")]
    [ProducesResponseType(typeof(List<BuildingPerformanceScoresResponse>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetPerformanceScoresByDate([FromRoute] string scopeId, [FromQuery]DateTime? startDate, [FromQuery]DateTime? endDate)
    {

        //To do: Add unit test https://dev.azure.com/willowdev/Unified/_workitems/edit/137160
        // To do : Add cache, refactor and clean up the code https://dev.azure.com/willowdev/Unified/_workitems/edit/137161
        var twin = await _dtApiService.GetTwin<TwinDto>(twinId: scopeId);
        if (twin is null)
        {
            throw new NotFoundException("Site not found");
        }
        if (!_dtApiService.IsBuildingScopeModelId(twin?.Metadata?.ModelId))
        {
            throw new ArgumentOutOfRangeException(scopeId, "Scope Id must be a 'Building' model type.");
        }
        var siteId = twin.SiteId.Value;
        await _accessControl.EnsureAccessSite(this.GetCurrentUserId() , Permissions.ViewSites, siteId );

        if (startDate is null || endDate is null)
        {
            return BadRequest("Both startDate and endDate are required.");
        }
        if (startDate >= endDate)
        {
            return BadRequest("startDate must be less than endDate.");
        }
        var site = await _siteApiService.GetSite(siteId);

        // for now we want to see if the query will work with the portfolioId then it will be required
        if (!site.PortfolioId.HasValue)
        {
           throw new NotFoundException("Site doesn't have PortfolioId");
        }

        var svc = _svcFactory.GetService(site.CustomerId);
        var result = await _resiliencePipelineService.ExecuteAsync(async _ =>
            await svc.GetPerformanceScoresByDate(new KPIPerformanceScoresRequest {
                SiteId = siteId,
                PortfolioId = site.PortfolioId.Value,
                StartDate = startDate.Value,
                EndDate = endDate.Value
            }));

        return Ok(result);
    }
}
