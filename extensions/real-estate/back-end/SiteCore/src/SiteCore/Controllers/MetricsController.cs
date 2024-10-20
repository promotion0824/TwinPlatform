using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteCore.Domain;
using SiteCore.Dto;
using SiteCore.Requests;
using SiteCore.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SiteCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class MetricsController : ControllerBase
    {
        private readonly IMetricsService _metricsService;

        public MetricsController(
            IMetricsService metricsService)
        {
            _metricsService = metricsService;
        }

        [HttpGet("metrics/current")]
        [Authorize]
        [ProducesResponseType(typeof(List<SiteMetricsDto>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets current metrics for all sites, or the provided list of siteIds")]
        public async Task<IActionResult> GetCurrentMetricsForSites([FromQuery] Guid[] siteIds)
        {
            var metrics = new List<SiteMetrics>();
            if (siteIds == null || siteIds.Length <= 0)
            {
                metrics = await _metricsService.GetCurrentMetricsForAllSitesAsync();
            }
            else
            {
                foreach (var siteId in siteIds)
                {
                    metrics.Add(await _metricsService.GetCurrentMetricsForSiteAsync(siteId));
                }
            }

            return Ok(SiteMetricsDto.MapFrom(metrics));
        }

        [HttpGet("metrics")]
        [Authorize]
        [ProducesResponseType(typeof(List<SiteMetricsDto>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets metrics between start and end datetime for all sites, or the provided list of siteIds")]
        public async Task<IActionResult> GetMetricsForSites([FromQuery] Guid[] siteIds, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            start = start.ToUniversalTime();
            end = end.ToUniversalTime();
            var metrics = new List<SiteMetrics>();
            if (siteIds == null || siteIds.Length <= 0)
            {
                metrics = await _metricsService.GetMetricsForAllSitesAsync(start, end);
            }
            else
            {
                foreach (var siteId in siteIds)
                {
                    metrics.Add(await _metricsService.GetMetricsForSiteAsync(siteId, start, end));
                }
            }
            return Ok(SiteMetricsDto.MapFrom(metrics));
        }

        [HttpGet("sites/{siteId}/metrics/current")]
        [Authorize]
        [ProducesResponseType(typeof(SiteMetricsDto), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets current metrics for the site identified by siteId")]
        public async Task<IActionResult> GetMetricsForSite([FromRoute] Guid siteId)
        {
            var metrics = await _metricsService.GetCurrentMetricsForSiteAsync(siteId);
            return Ok(SiteMetricsDto.MapFrom(metrics));
        }

        [HttpGet("sites/{siteId}/metrics")]
        [Authorize]
        [ProducesResponseType(typeof(SiteMetricsDto), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets metrics between start and end datetime for the site identified by siteId")]
        public async Task<IActionResult> GetMetricsForSite([FromRoute] Guid siteId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            start = start.ToUniversalTime();
            end = end.ToUniversalTime();
            var metrics = await _metricsService.GetMetricsForSiteAsync(siteId, start, end);
            return Ok(SiteMetricsDto.MapFrom(metrics));
        }

        [HttpPost("sites/{siteId}/metrics")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation("Import new metrics data points for the site identified by siteId")]
        public async Task<IActionResult> PostMetricsForSite([FromRoute] Guid siteId, [FromBody] ImportSiteMetricsRequest request)
        {
            await _metricsService.ImportSiteMetricsAsync(siteId, request);
            return NoContent();
        }
    }
}
