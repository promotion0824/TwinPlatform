using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Annotations;
using PlatformPortalXL.Services;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Features.SiteStructure
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class FloorsController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IFloorsService _floorsService;


        public FloorsController(IAccessControlService accessControl, IFloorsService floorsService)
        {
            _accessControl = accessControl;
            _floorsService = floorsService;
        }

        [HttpGet("sites/{siteId}/floors")]
        [ProducesResponseType(typeof(List<FloorSimpleDto>), StatusCodes.Status200OK)]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Gets a list of floors for the given site", Tags = new [] { "Floors" })]
        public async Task<ActionResult<IEnumerable<FloorSimpleDto>>> GetFloors([FromRoute]Guid siteId, [FromQuery] bool? hasBaseModule)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var floors = await _floorsService.GetFloorsAsync(siteId, hasBaseModule);

            return FloorSimpleDto.MapFrom(floors);
        }

    }
}
