using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AdminPortalXL.Dto;
using AdminPortalXL.Security;
using Willow.Infrastructure.MultiRegion;
using System.Collections.Generic;
using System.Linq;

namespace AdminPortalXL.Features.Region
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class RegionsController : ControllerBase
    {
        private readonly IMultiRegionSettings _regionSettings;

        public RegionsController(IMultiRegionSettings regionSettings)
        {
            _regionSettings = regionSettings;
        }

        [HttpGet("regions")]
        [AuthorizeForSupervisor]
        [ProducesResponseType(typeof(List<RegionDto>), StatusCodes.Status200OK)]
        public IActionResult GetRegions()
        {
            var regions = _regionSettings.RegionIds.Select(regionId => new RegionDto { Id = regionId }).ToList();
            return Ok(regions);
        }

    }
}
