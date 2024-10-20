using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Dto;
using WorkflowCore.Services;

namespace WorkflowCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class SettingsController : ControllerBase
    {
        private readonly ISettingsService _settingsService;

        public SettingsController(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        [HttpGet("sites/{siteId}/settings")]
        [Authorize]
        [ProducesResponseType(typeof(SiteExtensionsDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSiteExtensions([FromRoute] Guid siteId)
        {
            var siteExtensions = await _settingsService.GetSiteExtensions(siteId);
            return Ok(SiteExtensionsDto.Map(siteExtensions));
        }

        [HttpPut("sites/{siteId}/settings")]
        [Authorize]
        [ProducesResponseType(typeof(SiteExtensionsDto), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> UpsertSiteSettingsRequest([FromRoute] Guid siteId, [FromBody] UpsertSiteSettingsRequest request)
        {
            var siteExtensions = await _settingsService.UpsertSiteSettingsRequest(siteId, request);
            return Ok(SiteExtensionsDto.Map(siteExtensions));
        }
    }
}
