using DigitalTwinCore.Dto;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Services.Cacheless;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace DigitalTwinCore.Controllers.Twins
{
    [ApiController]
    public class TwinsController : ControllerBase
    {
        private readonly IDigitalTwinService _digitalTwinService;
        private readonly AzureDigitalTwinsSettings _adtSettings;
        /// <summary>
        /// Number of array items in an IN / NOT IN clause in ADT query
        /// limitation imposed by ADT https://learn.microsoft.com/en-us/azure/digital-twins/reference-service-limits
        /// </summary>
        const int Array_Max_Number = 50;

        public TwinsController(
            IDigitalTwinService digitalTwinService,
            IOptions<AzureDigitalTwinsSettings> adtSettings)
        {
            _digitalTwinService = digitalTwinService;
            _adtSettings = adtSettings.Value;
            _digitalTwinService.SiteAdtSettings = SiteAdtSettings.CreateInstance(Guid.Empty, null, _adtSettings);
        }

        [HttpGet("twins/{dtId}")]
        [Authorize]
        public async Task<ActionResult<TwinDto>> GetTwin([FromRoute] string dtId)
        {
            var service = (_digitalTwinService as CachelessAdtService);
            var twin = await service.GetTwinByIdAsync(dtId);
            return Ok(TwinDto.MapFrom(twin));
        }

        [HttpGet("twins/{dtId}/relationships")]
        [Authorize]
        public async Task<ActionResult<RelationshipDto>> GetTwinRelationships([FromRoute] string dtId)
        {
            var service = (_digitalTwinService as CachelessAdtService);
            var relationships = await service.GetTwinRelationshipsAsync(dtId);
            return Ok(RelationshipDto.MapFrom(relationships));
        }

        [HttpGet("twins/{dtId}/relationships/query")]
        [Authorize]
        public async Task<ActionResult<RelationshipDto>> GetTwinRelationshipsByQuery([FromRoute] string dtId,
            [FromQuery] string[] relationshipNames,
            [FromQuery] string[] targetModels,
            [FromQuery] int hops,
            [FromQuery] string sourceDirection,
            [FromQuery] string targetDirection)
        {
            var service = (_digitalTwinService as CachelessAdtService);
            var relationships = await service.GetTwinRelationshipsByQuery(dtId, relationshipNames, targetModels, hops, sourceDirection, targetDirection);
            return Ok(RelationshipDto.MapFrom(relationships));
        }

       
        /// <summary>
        /// Retrieves the building twins based on the provided external IDs.
        /// </summary>
        /// <param name="request">The request object containing the external ID values and name.</param>
        /// <returns>A list of building twins matching the provided external IDs.</returns>
        [HttpPost("twins/buildings")]
        [Authorize]
        [ProducesResponseType(typeof(List<BuildingsTwinDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetBuildingTwinsByExternalIds([FromBody] GetBuildingTwinsByExternalIdsRequest request)
        {
            var uniqueExternalIdValues = request.ExternalIdValues.Distinct().ToList();
            if (uniqueExternalIdValues.Count > Array_Max_Number)
            {
                return BadRequest($"External ID values should not exceed {Array_Max_Number} items.");
            }

            var result = await _digitalTwinService.GetBuildingTwinsByExternalIds(uniqueExternalIdValues, request.ExternalIdName);
            return Ok(result);
        }
    }
}
