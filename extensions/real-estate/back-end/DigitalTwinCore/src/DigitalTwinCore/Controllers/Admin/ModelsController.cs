using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Threading.Tasks;
using DigitalTwinCore.Dto;
using DigitalTwinCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalTwinCore.Controllers.Admin
{
    /// <summary>
    /// Models controller manages Azure Digital Twin models
    /// </summary>
    [Route("admin/sites/{siteId}/[controller]")]
    [ApiController]
    public class ModelsController : ControllerBase
    {
        private readonly IDigitalTwinServiceProvider _digitalTwinServiceFactory;

        public ModelsController(IDigitalTwinServiceProvider digitalTwinServiceFactory)
        {
            _digitalTwinServiceFactory = digitalTwinServiceFactory;
        }

        private async Task<IDigitalTwinService> GetDigitalTwinServiceAsync(Guid siteId)
        {
            return await _digitalTwinServiceFactory.GetForSiteAsync(siteId);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ModelDto>>> GetAsync([FromRoute] Guid siteId)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var modelEntities = service.GetModels();
            return Ok(ModelDto.MapFrom(service.SiteAdtSettings.SiteCodeForModelId, modelEntities));
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<string>>> GetIdsByQuery([FromRoute] Guid siteId, [Required, FromQuery] string query)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var modelEntities = service.GetModelIdsByQuery(query);
            return Ok(modelEntities);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ModelDto>> GetAsync([FromRoute] Guid siteId, [FromRoute] string id)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var modelEntity = service.GetModel(id);
            if (modelEntity == null)
            {
                return NotFound();
            }
            return Ok(ModelDto.MapFrom(service.SiteAdtSettings.SiteCodeForModelId, modelEntity));
        }

        [HttpGet("{id}/properties")]
        [Authorize]
        public async Task<IActionResult> GetModelProperties([FromRoute] Guid siteId, [FromRoute] string id)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var modelProps = await service.GetModelProps(id);

            return Ok(modelProps);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ModelDto>> PostAsync([FromRoute] Guid siteId, [FromBody] JsonElement modelJson)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            var entity = await service.AddModel(modelJson.GetRawText());
            return Created($"/admin/sites/{siteId}/models/{entity.Id}",
                            ModelDto.MapFrom(service.SiteAdtSettings.SiteCodeForModelId, entity));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteAsync([FromRoute] Guid siteId, [FromRoute] string id)
        {
            var service = await GetDigitalTwinServiceAsync(siteId);
            await service.DeleteModel(id);
            return NoContent();
        }
    }
}
