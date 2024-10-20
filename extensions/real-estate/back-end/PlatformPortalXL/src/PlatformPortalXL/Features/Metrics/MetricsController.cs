using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Auth.Services;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.ServicesApi.DirectoryApi;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Common;
using Willow.Platform.Models;

namespace PlatformPortalXL.Features.Metrics
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class MetricsController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDateTimeService _dateTimeService;
        private readonly IUserAuthorizedSitesService _userAuthorizedSitesService;
        private readonly ISiteApiService _siteApi;

        public MetricsController(
            IAccessControlService accessControl,
            IDateTimeService dateTimeService,
            IUserAuthorizedSitesService userAuthorizedSitesService,
            ISiteApiService siteApi)
        {
            _accessControl = accessControl;
            _dateTimeService = dateTimeService;
            _userAuthorizedSitesService = userAuthorizedSitesService;
            _siteApi = siteApi;
        }

        [HttpGet("metrics")]
        [Authorize]
        [SwaggerOperation("Gets building metrics for all sites between start and end date time (if dates omitted, data for the last 24 hours is returned)", Tags = new [] { "Metrics" })]
        public async Task<ActionResult<List<SiteMetricsDto>>> GetMetrics([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var userId = this.GetCurrentUserId();

            var sites = await _userAuthorizedSitesService.GetAuthorizedSites(userId, Permissions.ViewSites);

            if (start == null)
            {
                start = _dateTimeService.UtcNow.AddDays(-1);
            }
            else
            {
                start = start.Value.ToUniversalTime();
            }
            if (end == null)
            {
                end = _dateTimeService.UtcNow;
            }
            else
            {
                end = end.Value.ToUniversalTime();
            }

            if (start >= end)
            {
                throw new ArgumentException($"End date must be later than start date");
            }

            var output = SiteMetricsDto.MapFrom(
                    await _siteApi.GetMetricsForSitesAsync(
                            sites.Where(s => s.Status != SiteStatus.Deleted)
                                .Select(s => s.Id),
                            start.Value, 
                            end.Value));

            output.SelectMany(sm => sm.Metrics)
                .ToList()
                .ForEach(m => { 
                    m.Metrics = null; 
                });

            return Ok(output);
        }

        [HttpGet("sites/{siteId}/metrics")]
        [Authorize]
        [SwaggerOperation("Gets building metrics for the site identified by siteId between start and end date time (if dates omitted, data for the last 24 hours is returned)", Tags = new[] { "Metrics" })]
        public async Task<ActionResult<SiteMetricsDto>> GetSiteMetrics([FromRoute] Guid siteId, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            if (start == null)
            {
                start = _dateTimeService.UtcNow.AddDays(-1);
            }
            else
            {
                start = start.Value.ToUniversalTime();
            }
            if (end == null)
            {
                end = _dateTimeService.UtcNow;
            }
            else
            {
                end = end.Value.ToUniversalTime();
            }

            if (start >= end)
            {
                throw new ArgumentException($"End date must be later than start date");
            }

            return Ok(SiteMetricsDto.MapFrom(await _siteApi.GetMetricsForSiteAsync(siteId, start.Value, end.Value)));
        }
    }
}
