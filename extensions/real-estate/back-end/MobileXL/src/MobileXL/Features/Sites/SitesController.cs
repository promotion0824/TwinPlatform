using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileXL.Dto;
using MobileXL.Services;
using MobileXL.Services.Apis.DirectoryApi;
using MobileXL.Services.Apis.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace MobileXL.Features.Sites
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class SitesController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly IDirectoryApiService _directoryApi;
        private readonly ISiteApiService _siteApi;

        public SitesController(IAccessControlService accessControl, IDirectoryApiService directoryApi, ISiteApiService siteApi)
        {
            _accessControl = accessControl;
            _directoryApi = directoryApi;
            _siteApi = siteApi;
        }

        [HttpGet("sites/{siteId}")]
        [Authorize]
        [ProducesResponseType(typeof(SiteSimpleDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets the site by specified siteid", Tags = new[] { "Sites" })]
        public async Task<ActionResult> GetSite([FromRoute] Guid siteId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), this.GetCurrentUserId(), siteId);

            var site = await _siteApi.GetSite(siteId);
            var siteFeatures = await _directoryApi.GetSiteFeatures(siteId);
            var siteDto = SiteSimpleDto.MapFrom(site);
            siteDto.Features = SiteFeaturesDto.Map(siteFeatures);
            return Ok(siteDto);
        }
    }
}
