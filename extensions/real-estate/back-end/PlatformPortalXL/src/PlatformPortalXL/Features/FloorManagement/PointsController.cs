using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace PlatformPortalXL.Features.FloorManagement
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class PointsController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IFloorManagementService _floorManagementService;

        public PointsController(IAccessControlService accessControl, IFloorManagementService floorManagementService)
        {
            _accessControl = accessControl;
            _floorManagementService = floorManagementService;
        }

        [HttpGet("/sites/{siteId}/floors/{floorId}/layerGroup/{layerGroupId}/Layers/{layerId}/points")]
        [Authorize]
        [SwaggerOperation("Gets a list of points associated to the given layer", Tags = new [] { "Floors" })]
        public async Task<ActionResult<IEnumerable<PointSimpleDto>>> GetPointsByLayer([FromRoute] Guid siteId,[FromRoute] Guid floorId,[FromRoute] Guid layerGroupId,[FromRoute] Guid layerId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var points = await _floorManagementService.GetPointsByLayerAsync(siteId, floorId, layerGroupId, layerId);
            return PointSimpleDto.MapFrom(points);
        }

        [HttpGet("/sites/{siteId}/floors/{floorId}/layerGroup/{layerGroupId}/Layers/{layerId}/points/livedata")]
        [Authorize]
        [SwaggerOperation("Gets live data of points which are associated to the given layer", Tags = new [] { "Floors" })]
        public async Task<ActionResult<IEnumerable<PointLivedataDto>>> GetPointsLivedataByLayer([FromRoute] Guid siteId,[FromRoute] Guid floorId,[FromRoute] Guid layerGroupId,[FromRoute] Guid layerId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ViewSites, siteId);

            var pointsLive = await _floorManagementService.GetPointsLiveDataByLayerAsync(siteId, floorId, layerGroupId, layerId);

            return PointLivedataDto.MapFrom(pointsLive);
        }
    }
}
