using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinCore.Features.SiteAdmin
{
    [Route("admin/sites")]
    [ApiController]
    public class SiteController : ControllerBase
    {
        private readonly ISiteSettingsService _siteSettingsService;

        public SiteController(ISiteSettingsService siteSettingsService)
        {
            _siteSettingsService = siteSettingsService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> NewAdtSite(NewAdtSiteRequest request, CancellationToken cancellationToken)
        {
            var siteSettings = await _siteSettingsService.AddSiteSettings(request, cancellationToken);

            if (siteSettings != null)
            {
                return CreatedAtAction(nameof(GetAdtSite), new { siteId = siteSettings.SiteId }, siteSettings);
            }

            return BadRequest($"Site {request.SiteId} is already configured");
        }

        [HttpGet]
        [Route("{siteId}")]
        [Authorize]
        public async Task<ActionResult> GetAdtSite(Guid siteId)
        {
            var siteSettings = await _siteSettingsService.GetSiteSettings(siteId);

            if (siteSettings != null)
            {
                return Ok(siteSettings);
            }

            return NotFound();
        }
    }
}
