using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using SiteCore.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using SiteCore.Dto;
using SiteCore.Requests;
using SiteCore.Domain;
using Willow.Common;

namespace SiteCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class FloorsController : ControllerBase
    {
        private readonly IFloorService _floorService;

        public FloorsController(IFloorService floorService)
        {
            _floorService = floorService;
        }

		[HttpGet("sites/floors")]
		[Authorize]
		[ProducesResponseType(typeof(List<FloorSimpleDto>), (int)HttpStatusCode.OK)]
		[ProducesResponseType((int)HttpStatusCode.NotFound)]
		[SwaggerOperation("Gets a list of floors for the given site")]
		public async Task<IActionResult> GetFloors([FromQuery] List<Guid> floorIds)
		{
			var floors = await _floorService.GetFloors(floorIds);
			return Ok(FloorSimpleDto.MapFrom(floors));
		}

		[HttpGet("sites/{siteId}/floors")]
        [Authorize]
        [ProducesResponseType(typeof(List<FloorSimpleDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [SwaggerOperation("Gets a list of floors for the given site")]
        public async Task<IActionResult> GetFloors([FromRoute]Guid siteId, [FromQuery] bool? hasBaseModule)
        {
            bool allSites = !(hasBaseModule ?? false);
            var floors = await _floorService.GetFloors(siteId, allSites);
            return Ok(FloorSimpleDto.MapFrom(floors));
        }

        [HttpGet("sites/{siteId}/floors/{floorIdOrCode}")]
        [Authorize]
        [ProducesResponseType(typeof(List<FloorDetailDto>), (int)HttpStatusCode.OK)]
        [SwaggerOperation("Gets a floor by the given floor id or code")]
        public async Task<IActionResult> GetFloor([FromRoute] Guid siteId, [FromRoute] string floorIdOrCode)
        {
            Floor floor;
            if (Guid.TryParse(floorIdOrCode, out Guid floorId))
            {
                floor = await _floorService.GetFloorById(siteId, floorId);
            }
            else
            {
                var floorCode = floorIdOrCode;
                floor = await _floorService.GetFloorByCode(siteId, floorCode);
            }
            return Ok(FloorDetailDto.MapFrom(floor));
        }

        [HttpPost("sites/{siteId}/floors")]
        [Authorize]
        [ProducesResponseType(typeof(FloorDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation("Create new floor")]
        public async Task<IActionResult> CreateFloor(
            [FromRoute] Guid siteId,
            [FromBody] CreateFloorRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                throw new ArgumentNullException("Missing floor code.").WithData( new { SiteId = siteId });
            }

            if (string.IsNullOrEmpty(request.Name))
            {
                throw new ArgumentNullException("Missing floor name.").WithData(new { SiteId = siteId });
            }

            if (!string.IsNullOrEmpty(request.ModelReference) &&
                !Guid.TryParse(request.ModelReference, out _))
            {
	            throw new ArgumentNullException("Model Reference is not valid.").WithData(new
		            { SiteId = siteId, ModelReference = request.ModelReference });
            }

            if (await _floorService.IsFloorExistByCode(siteId, request.Code))
            {
                throw new ArgumentException("Floor code already exists.").WithData(new { SiteId = siteId, FloorCode = request.Code });
            }

            var floor = await _floorService.CreateFloor(siteId, request);
            return Ok(FloorDetailDto.MapFrom(floor));
        }

        [HttpDelete("sites/{siteId}/floors/{floorId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation("Deletes a specified floor")]
        public async Task<ActionResult<FloorDetailDto>> DeleteFloor(
            [FromRoute] Guid siteId,
            [FromRoute] Guid floorId)
        {
            await _floorService.DeleteFloor(siteId, floorId);
            return NoContent();
        }

        [HttpPut("sites/{siteId}/floors/{floorId}")]
        [Authorize]
        [SwaggerOperation("Updates floor")]
        public async Task<ActionResult<FloorDetailDto>> UpdateFloor(
            [FromRoute] Guid siteId,
            [FromRoute] Guid floorId,
            [FromBody] UpdateFloorRequest updateFloorRequest)
        {
	        if (!string.IsNullOrEmpty(updateFloorRequest.ModelReference) &&
	            !Guid.TryParse(updateFloorRequest.ModelReference, out _))
	        {
		        throw new ArgumentNullException("Model Reference is not valid.").WithData(new
			        { SiteId = siteId, FloorId=floorId, ModelReference = updateFloorRequest.ModelReference });
	        }
			return FloorDetailDto.MapFrom(await _floorService.UpdateFloorAsync(floorId, updateFloorRequest));
        }

        [HttpPut("sites/{siteId}/floors/{floorId}/geometry")]
        [Authorize]
        [SwaggerOperation("Updates floor geometry")]
        public async Task<ActionResult<FloorDetailDto>> UpdateFloorGeometry(
            [FromRoute] Guid siteId,
            [FromRoute] Guid floorId,
            [FromBody] UpdateFloorGeometryRequest updateRequest)
        {
            return FloorDetailDto.MapFrom(await _floorService.UpdateFloorGeometryAsync(floorId, updateRequest));
        }

        [HttpPost("/sites/{siteId}/floors/{floorId}/2dmodules")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("Uploads a 2d module for a floor")]
        public async Task<ActionResult<FloorDetailDto>> UploadModule2DAsync(
            [FromRoute] Guid siteId,
            [FromRoute] Guid floorId,
            IFormFileCollection files)
        {
            if (!files.Any() || files.Any(f => f == null))
            {
                throw new ArgumentNullException("Multipart formdata files collection expected").WithData(new { SiteId = siteId, FloorId = floorId });
            }

            var floor = await _floorService.Upload2DFloorModules(siteId, floorId, files);
            return FloorDetailDto.MapFrom(floor);
        }

        [HttpPost("/sites/{siteId}/floors/{floorId}/3dmodules")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("Uploads a 3d module for a floor")]
        public async Task<ActionResult<FloorDetailDto>> UploadModule3DAsync(
            [FromRoute] Guid siteId,
            [FromRoute] Guid floorId,
            [FromBody] CreateUpdateModule3DRequest request)
        {
            var floor = await _floorService.Upload3DFloorModules(siteId, floorId, request);
            return FloorDetailDto.MapFrom(floor);
        }

        [HttpDelete("sites/{siteId}/floors/{floorId}/module/{moduleId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("Uploads a floor plan image for a floor")]
        public async Task<ActionResult<FloorDetailDto>> DeleteFloorPlanAsync(
            [FromRoute]Guid floorId,
            [FromRoute]Guid moduleId)
        {
            var floor = await _floorService.DeleteModule(floorId, moduleId);
            return FloorDetailDto.MapFrom(floor);
        }

        [HttpPut("sites/{siteId}/floors/sortorder")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("Update floors sort order")]
        public async Task<IActionResult> UpdateSortOrder(Guid siteId, Guid[] floorIds)
        {
            if (floorIds.Length == 0)
            {
                return BadRequest("floorIds is empty.");
            }

            await _floorService.UpdateSortOrder(siteId, floorIds);
            return NoContent();
        }
    }
}
