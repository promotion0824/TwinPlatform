using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SiteCore.Dto;
using SiteCore.Requests;
using SiteCore.Services;

namespace SiteCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class LayerGroupsController : Controller
    {
        private readonly ILayerGroupsService _layerGroupsService;
        private readonly IModulesService _modulesService;
        private readonly IFloorService _floorService;

        public LayerGroupsController(
            ILayerGroupsService layerGroupsService, 
            IModulesService modulesService, 
            IFloorService floorService)
        {
            _layerGroupsService = layerGroupsService;
            _modulesService = modulesService;
            _floorService = floorService;
        }

        [HttpGet("/sites/{siteId}/floors/{floorId}/layerGroups")]
        [Authorize]
        public async Task<ActionResult<LayerGroupListDto>> Get(Guid siteId, Guid floorId)
        {
            var groups = await _layerGroupsService.GetLayerGroupsAsync(floorId);
            var modules = await _modulesService.GetModulesByFloorAsync(siteId, floorId);
            var floor = await _floorService.GetFloorById(siteId, floorId);

            return LayerGroupListDto.MapFrom(groups, modules, floor);
        }

        [HttpGet("/sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}")]
        [Authorize]
        public async Task<ActionResult<LayerGroupDto>> Get(Guid siteId, Guid floorId, Guid layerGroupId)
        {
            var group = await _layerGroupsService.GetLayerGroupAsync(floorId, layerGroupId);
            return LayerGroupDto.MapFrom(group);
        }

        [HttpPost("/sites/{siteId}/floors/{floorId}/layerGroups")]
        [Authorize]
        public async Task<ActionResult<LayerGroupDto>> Post(
            Guid siteId, 
            Guid floorId, 
            [FromBody] CreateLayerGroupRequest createRequest)
        {
            var group = await _layerGroupsService.CreateLayerGroupAsync(floorId, createRequest);
            return LayerGroupDto.MapFrom(group);
        }

        [HttpDelete("/sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid siteId, Guid floorId, Guid layerGroupId)
        {
            await _layerGroupsService.DeleteLayerGroupAsync(floorId, layerGroupId);
            return Ok();
        }

        [HttpPut("/sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}")]
        [Authorize]
        public async Task<ActionResult<LayerGroupDto>> Put(
            Guid siteId, 
            Guid floorId, 
            Guid layerGroupId, 
            [FromBody] UpdateLayerGroupRequest updateRequest)
        {
            var group = await _layerGroupsService.UpdateLayerGroupAsync(floorId, layerGroupId, updateRequest);
            return LayerGroupDto.MapFrom(group);
        }
    }
}
