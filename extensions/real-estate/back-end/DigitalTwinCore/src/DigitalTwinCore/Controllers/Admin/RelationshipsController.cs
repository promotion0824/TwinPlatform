using System;
using System.Threading.Tasks;
using DigitalTwinCore.Dto;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinCore.Controllers.Admin
{
    [Route("admin/sites/{siteId}/twins/{twinId}/[controller]")]
    [ApiController]
    public class RelationshipsController : ControllerBase
    {
        private readonly IDigitalTwinServiceProvider _digitalTwinServiceFactory;

        public RelationshipsController(IDigitalTwinServiceProvider digitalTwinServiceFactory)
        {
            _digitalTwinServiceFactory = digitalTwinServiceFactory;
        }

        private async Task<IDigitalTwinService> GetDigitalTwinServiceAsync(Guid siteId)
        {
            return await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<RelationshipDto>> GetAsync(
            [FromRoute] Guid siteId, 
            [FromRoute] string twinId, 
            [FromRoute] string id)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var entity = await service.GetRelationshipAsync(twinId, id);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(RelationshipDto.MapFrom(entity));
        }

        [HttpGet("{id}/nocache")]
        [Authorize]
        public async Task<ActionResult<RelationshipDto>> GetUncachedAsync(
            [FromRoute] Guid siteId, 
            [FromRoute] string twinId, 
            [FromRoute] string id)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var entity = await service.GetRelationshipUncachedAsync(twinId, id);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(RelationshipDto.MapFrom(entity));
        }


        [HttpPost("{id}")]
        [Authorize]
        public async Task<ActionResult<RelationshipDto>> PostAsync(
            [FromRoute] Guid siteId, 
            [FromRoute] string twinId, 
            [FromRoute] string id, 
            [FromBody] RelationshipDto value)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var entity = await service.AddRelationshipAsync(twinId, id, Relationship.MapFrom(value));
            return Created($"/admin/sites/{siteId}/twins/{twinId}/relationships/{entity.Id}", RelationshipDto.MapFrom(entity));
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<ActionResult<RelationshipDto>> PatchAsync(
            [FromRoute] Guid siteId, 
            [FromRoute] string twinId, 
            [FromRoute] string id, 
            [FromBody] JsonPatchDocument value)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var entity = await service.UpdateRelationshipAsync(twinId, id, value);
            return Ok(RelationshipDto.MapFrom(entity));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(
            [FromRoute] Guid siteId, 
            [FromRoute] string twinId, 
            [FromRoute] string id)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            await service.DeleteRelationshipAsync(twinId, id);
            return NoContent();
        }
    }
}
