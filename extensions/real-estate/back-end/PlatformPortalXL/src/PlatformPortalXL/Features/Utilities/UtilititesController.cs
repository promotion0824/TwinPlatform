using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace PlatformPortalXL.Features.Utilities
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class UtilitiesController : ControllerBase
    {
        private readonly ITimeZoneService _timeZoneService;

        public UtilitiesController(ITimeZoneService timeZoneService)
        {
            _timeZoneService = timeZoneService;
        }

        [HttpGet("timezones")]
        [Authorize]
        [ProducesResponseType(typeof(List<TimeZoneDto>), StatusCodes.Status200OK)]
        [SwaggerOperation("Gets a list of timezones", Tags = ["Utilities"])]
        public async Task<ActionResult> GetTimeZones()
        {
            var dtos = await _timeZoneService.GetTimeZones();

            return Ok(dtos);
        }
    }
}
