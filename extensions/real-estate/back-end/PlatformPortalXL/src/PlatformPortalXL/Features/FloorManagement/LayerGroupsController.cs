using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace PlatformPortalXL.Features.FloorManagement
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class LayerGroupsController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IFloorManagementService _floorManagementService;

        public LayerGroupsController(IAccessControlService accessControl, IFloorManagementService floorManagementService)
        {
            _accessControl = accessControl;
            _floorManagementService = floorManagementService;
        }

        [HttpGet("/sites/{siteId}/floors/{floorId}/layerGroups")]
        [Authorize]
        [SwaggerOperation("Gets a list of LayerGroups", Tags = new [] { "Floors" })]
        public async Task<ActionResult<LayerGroupListDto>> Get(Guid siteId, Guid floorId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var groups = await _floorManagementService.GetLayerGroupsAsync(siteId, floorId);
            return LayerGroupListDto.MapFrom(groups);
        }

        [HttpPost("/sites/{siteId}/floors/{floorId}/layerGroups")]
        [Authorize]
        [SwaggerOperation("Creates a LayerGroup", Tags = new [] { "Floors" })]
        public async Task<ActionResult<LayerGroupDto>> Post(Guid siteId, Guid floorId, [FromBody] CreateLayerGroupRequest createRequest)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageFloors, siteId);

            var group = await _floorManagementService.CreateLayerGroupAsync(siteId, floorId, createRequest);
            return LayerGroupDto.MapFrom(group);
        }

        [HttpDelete("/sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}")]
        [Authorize]
        [SwaggerOperation("Deletes the given LayerGroup", Tags = new [] { "Floors" })]
        public async Task<IActionResult> Delete(Guid siteId, Guid floorId, Guid layerGroupId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageFloors, siteId);

            await _floorManagementService.DeleteLayerGroupAsync(siteId, floorId, layerGroupId);
            return Ok();
        }

        [HttpPut("/sites/{siteId}/floors/{floorId}/layerGroups/{layerGroupId}")]
        [Authorize]
        [SwaggerOperation("Updates a LayerGroup", Tags = new [] { "Floors" })]
        public async Task<ActionResult<LayerGroupDto>> Put(Guid siteId, Guid floorId, Guid layerGroupId, [FromBody] UpdateLayerGroupRequest updateRequest)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageFloors, siteId);

            var group = await _floorManagementService.UpdateLayerGroupAsync(siteId, floorId, layerGroupId,  updateRequest);
            return LayerGroupDto.MapFrom(group);
        }
    }
}
