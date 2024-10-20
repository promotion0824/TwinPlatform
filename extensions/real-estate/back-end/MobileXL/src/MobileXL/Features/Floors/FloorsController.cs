using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MobileXL.Dto;
using MobileXL.Services;
using MobileXL.Services.Apis.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MobileXL.Features.Directory
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class FloorsController : ControllerBase
    {
        private readonly IAccessControlService _accessControl;
        private readonly ISiteApiService _siteApi;

        public FloorsController(IAccessControlService accessControl, ISiteApiService siteApi)
        {
            _accessControl = accessControl;
            _siteApi = siteApi;
        }

        [HttpGet("sites/{siteId}/floors")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation("Gets a list of floors for the given site", Tags = new [] { "Floors" })]
        public async Task<ActionResult<IList<FloorSimpleDto>>> GetFloors([FromRoute] Guid siteId)
        {
            var currentUserId = this.GetCurrentUserId();
            var currentUserType = this.GetCurrentUserType();
            await _accessControl.EnsureAccessSite(currentUserType, currentUserId, siteId);

            var floors = await _siteApi.GetFloors(siteId);

            return FloorSimpleDto.MapFrom(floors);
        }
    }
}
