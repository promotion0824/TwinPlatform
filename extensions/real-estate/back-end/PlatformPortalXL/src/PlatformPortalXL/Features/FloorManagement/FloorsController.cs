using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Dto;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PlatformPortalXL.Requests.SiteCore;
using PlatformPortalXL.ServicesApi.SiteApi;
using Swashbuckle.AspNetCore.Annotations;
using PlatformPortalXL.Services;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services.Forge;
using System.Collections.Generic;

using Willow.Api.DataValidation;
using PlatformPortalXL.Features.Pilot;

namespace PlatformPortalXL.Features.FloorManagement
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class FloorsController : Controller
    {
        private readonly IAccessControlService _accessControl;
        private readonly IFloorsService _floorsService;
        private readonly IFloorsApiService _floorsApiService;
        private readonly IForgeService _forgeService;

        public FloorsController(IAccessControlService accessControl, IFloorsService floorsService, IFloorsApiService floorsApiService, IForgeService forgeService)
        {
            _accessControl = accessControl;
            _floorsService = floorsService;
            _floorsApiService = floorsApiService;
            _forgeService = forgeService;
        }

        [HttpPost("sites/{siteId}/floors")]
        [Authorize]
        [ProducesResponseType(typeof(FloorDetailDto), StatusCodes.Status200OK)]
        [SwaggerOperation("Creates a new floor", Tags = new[] { "Floors" })]
        public async Task<IActionResult> CreateFloor(
            [FromRoute] Guid siteId,
            [FromBody] CreateFloorRequest createFloorRequest)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageFloors, siteId);

            var validationError = new ValidationError();
          
            if (!string.IsNullOrEmpty(createFloorRequest.ModelReference) &&
                !Guid.TryParse(createFloorRequest.ModelReference, out _))
            {
                validationError.Items.Add(new ValidationErrorItem(nameof(createFloorRequest.ModelReference), "Model Reference is not valid"));
            }
            if (validationError.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }

            return Ok(FloorDetailDto.MapFrom(await _floorsService.CreateFloorAsync(siteId, createFloorRequest)));
        }

        [HttpPut("sites/{siteId}/floors/{floorId}")]
        [Authorize]
        [SwaggerOperation("Updates a specific floor", Tags = new [] { "Floors" })]
        public async Task<ActionResult<FloorDetailDto>> UpdateFloor(
            [FromRoute] Guid siteId,
            [FromRoute] Guid floorId,
            [FromBody] UpdateFloorRequest updateFloorRequest)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageFloors, siteId);

            var validationError = new ValidationError();
          
            if (!string.IsNullOrEmpty(updateFloorRequest.ModelReference) &&
                !Guid.TryParse(updateFloorRequest.ModelReference, out _))
            {
                validationError.Items.Add(new ValidationErrorItem(nameof(updateFloorRequest.ModelReference), "Model Reference is not valid"));
            }
            if (validationError.Items.Any())
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }
            return FloorDetailDto.MapFrom(await _floorsService.UpdateFloorAsync(siteId, floorId, updateFloorRequest));
        }

        [HttpDelete("sites/{siteId}/floors/{floorId}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation("Deletes a specific floor", Tags = new[] { "Floors" })]
        public async Task<IActionResult> DeleteFloor(
            [FromRoute] Guid siteId,
            [FromRoute] Guid floorId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageFloors, siteId);

            await _floorsService.DeleteFloorAsync(siteId, floorId);
            return NoContent();
        }

        [HttpPut("sites/{siteId}/floors/{floorId}/geometry")]
        [Authorize]
        [SwaggerOperation("Updates a specific floor's geometry", Tags = new [] { "Floors" })]
        public async Task<ActionResult<FloorDetailDto>> UpdateFloorGeometry(
            [FromRoute] Guid siteId,
            [FromRoute] Guid floorId,
            [FromBody] UpdateFloorGeometryRequest updateRequest)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageFloors, siteId);

            return FloorDetailDto.MapFrom(await _floorsService.UpdateFloorGeometryAsync(siteId, floorId, updateRequest));
        }

        [HttpPost("sites/{siteId}/floors/{floorId}/2dmodules")]
        [Authorize]
        [ProducesResponseType(typeof(FloorDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Uploads 2d modules for the given floor", Tags = new [] { "Floors" })]
        public async Task<ActionResult<FloorDetailDto>> Upload2DModuleAsync(
            [FromRoute] Guid siteId,
            [FromRoute]Guid floorId,
            [FromForm] IFormFileCollection files)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageFloors, siteId);

            if (!files.Any() || files.Any(f => f == null))
            {
                throw new ArgumentNullException("Multipart formdata files collection expected");
            }

            var (floor, validationError) = await _floorsApiService.UpdateFloorModules2DAsync(siteId, floorId, files);
            if (validationError != null)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
            }
            return FloorDetailDto.MapFrom(floor);
        }

        [HttpPost("sites/{siteId}/floors/{floorId}/3dmodules")]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [Authorize]
        [Consumes("multipart/form-data")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(FloorDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
        [SwaggerOperation("Uploads 3d modules for the given floor", Tags = new [] { "Floors" })]
        public async Task<ActionResult<FloorDetailDto>> Upload3DModuleAsync(
            [FromRoute] Guid siteId,
            [FromRoute]Guid floorId,
            List<IFormFile> files)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageFloors, siteId);

            if (!files.Any())
            {
                throw new ArgumentNullException("Multipart formdata files collection expected");
            }

            try
            {
                var uploadInfo = await _forgeService.StartConvertToSvfAsync(siteId, files);

                var request = new CreateUpdateModule3DRequest
                {
                    Modules3D = uploadInfo.Select(i => new Module3DInfo { Url = i.ForgeInfo.Urn, ModuleName = i.File.FileName }).ToList()
                };

                var (floor, validationError) = await _floorsApiService.UpdateFloorModules3DAsync(siteId, floorId, request);
                if (validationError != null)
                {
                    return StatusCode(StatusCodes.Status422UnprocessableEntity, validationError);
                }

                await _floorsService.Broadcast(siteId, floorId, uploadInfo.Select(x => x.ForgeInfo.Urn).ToList());

                _floorsService.ClearCache(siteId);

                return FloorDetailDto.MapFrom(floor);
            }
            catch(Autodesk.Forge.Client.ApiException ex)
            {
                switch (ex.ErrorCode)
                {
                    case StatusCodes.Status409Conflict:
                        return StatusCode(StatusCodes.Status429TooManyRequests);
                    default:
                        throw;
                }
            }
        }

        [HttpDelete("sites/{siteId}/floors/{floorId}/module/{moduleId}")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("Deletes a module from the given floor", Tags = new [] { "Floors" })]
        public async Task<ActionResult<FloorDetailDto>> DeleteModuleAsync(
            [FromRoute] Guid siteId,
            [FromRoute]Guid floorId,
            [FromRoute]Guid moduleId)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageFloors, siteId);

            var floor = await _floorsApiService.DeleteModuleAsync(siteId, floorId, moduleId);
            _floorsService.ClearCache(siteId);

            return FloorDetailDto.MapFrom(floor);
        }

        [HttpPut("sites/{siteId}/floors/sortorder")]
        [Authorize]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [SwaggerOperation("Update sort order for given floors", Tags = new[] { "Floors" })]
        public async Task<IActionResult> UpdateSortOrder([FromRoute] Guid siteId, Guid[] floorIds)
        {
            await _accessControl.EnsureAccessSite(this.GetCurrentUserId(), Permissions.ManageFloors, siteId);

            await _floorsService.UpdateSortOrder(siteId, floorIds);

            return NoContent();
        }
    }
}
