using System;
using System.Threading.Tasks;
using DigitalTwinCore.Features.RelationshipMap.Dtos;
using DigitalTwinCore.Features.RelationshipMap.Extensions;
using DigitalTwinCore.Features.RelationshipMap.Services;
using DigitalTwinCore.Features.TwinsSearch.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinCore.Features.RelationshipMap.Controllers
{
    [ApiController]
    public class RelationshipMapController: ControllerBase
    {
        private readonly ITwinSystemService _twinSystemService;
        private readonly IRelationshipMapService _relationshipMapservice;

        public RelationshipMapController(ITwinSystemService twinSystemService, IRelationshipMapService relationshipMapService)
        {
            _twinSystemService = twinSystemService;
            _relationshipMapservice = relationshipMapService;
        }

        [HttpGet("{siteId}/TwinsGraph")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TwinGraphDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        [Authorize]
        public async Task<IActionResult> GetTwinGraph([FromQuery] string[] twinIds, [FromRoute] Guid siteId)
        {
            var graph = await _twinSystemService.GetTwinSystemGraph(twinIds, siteId);
            var result = graph.MapToGraphDto();

            return Ok(result);
        }

        [HttpGet("{siteId}/twins/{twinId}/relatedtwins")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TwinGraphDto))]
        [ProducesDefaultResponseType]
        [Authorize]
        public async Task<IActionResult> GetRelatedTwinsByHopsAsync([FromRoute] Guid siteId, [FromRoute] string twinId)
        {
            var relationships = await _relationshipMapservice.GetRelatedTwinsByHops(siteId, twinId);

            return Ok(relationships);
        }

        [HttpGet("twins/{dtId}/relatedtwins")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TwinGraphDto))]
        [ProducesDefaultResponseType]
        [Authorize]
        public async Task<IActionResult> GetRelatedTwinsByHopsAsync([FromRoute] string dtId)
        {
            // Pass empty siteId here because it only be used to get ADT instance settings from DB when not in the appsettings
            // For single tenant, each individual environment will have ADT/ADX settings configured
            var relationships = await _relationshipMapservice.GetRelatedTwinsByHops(Guid.Empty, dtId);

            return Ok(relationships);
        }
    }
}
