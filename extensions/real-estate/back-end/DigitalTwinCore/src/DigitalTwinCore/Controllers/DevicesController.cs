using DigitalTwinCore.Dto;
using DigitalTwinCore.Models;
using DigitalTwinCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;

namespace DigitalTwinCore.Controllers
{
    [Route("sites/{siteId}/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public DevicesController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        /// <summary>
        /// Retrieves list of devices for the specified site, optionally including points
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="includePoints"></param>
        /// <returns></returns>
        [HttpGet("paged")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getDevices", Tags = new[] { "Devices" })]
        public async Task<ActionResult<Page<DeviceDto>>> GetListAsync(
            [FromRoute] Guid siteId, 
            [FromQuery] bool? includePoints,
            [FromHeader] string continuationToken)
        {
            var output = await _assetService.GetDevicesAsync(siteId, includePoints, HeaderUtilities.UnescapeAsQuotedString(continuationToken).Value);

            return Ok(new Page<DeviceDto> { Content = DeviceDto.MapFrom(output.Content.ToList()), ContinuationToken = output.ContinuationToken });
        }

        /// <summary>
        /// Retrieves list of devices for the specified site, optionally including points
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="includePoints"></param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getDevices", Tags = new[] { "Devices" })]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetListAsync(
            [FromRoute] Guid siteId,
            [FromQuery] bool? includePoints)
        {
            var output = await _assetService.GetDevicesAsync(siteId, includePoints);
            return Ok(DeviceDto.MapFrom(output));
        }


        /// <summary>
        /// Retrieves list of devices for the specified site and connector id, optionally including points
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="includePoints"></param>
        /// <returns></returns>
        [HttpGet("/sites/{siteId}/connectors/{connectorId}/[Controller]/paged")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getDevices", Tags = new[] { "Devices" })]
        public async Task<ActionResult<Page<DeviceDto>>> GetListByConnectorAsync(
            [FromRoute] Guid siteId, 
            [FromRoute] Guid connectorId, 
            [FromQuery] bool? includePoints,
            [FromHeader] string continuationToken)
        {
            var output = await _assetService.GetDevicesByConnectorAsync(siteId, connectorId, includePoints, HeaderUtilities.UnescapeAsQuotedString(continuationToken).Value);

            return Ok(new Page<DeviceDto> { Content = DeviceDto.MapFrom(output.Content.ToList()), ContinuationToken = output.ContinuationToken });
        }

        /// <summary>
        /// Retrieves list of devices for the specified site and connector id, optionally including points
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="includePoints"></param>
        /// <returns></returns>
        [HttpGet("/sites/{siteId}/connectors/{connectorId}/[Controller]")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getDevices", Tags = new[] { "Devices" })]
        public async Task<ActionResult<IEnumerable<DeviceDto>>> GetListByConnectorAsync(
            [FromRoute] Guid siteId,
            [FromRoute] Guid connectorId,
            [FromQuery] bool? includePoints)
        {
            var output = await _assetService.GetDevicesByConnectorAsync(siteId, connectorId, includePoints);
            return Ok(DeviceDto.MapFrom(output));
        }

        /// <summary>
        /// Retrieves an device and its parameters by its unique id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getDeviceByUniqueId", Tags = new[] { "Devices" })]
        public async Task<ActionResult<DeviceDto>> GetByUniqueIdAsync([FromRoute] Guid siteId, [FromRoute] Guid id, [FromQuery] bool? includePoints)
        {
            var output = await _assetService.GetDeviceByUniqueIdAsync(siteId, id, includePoints);

            if (output == null)
            {
                throw new ResourceNotFoundException("Device", id);
            }

            return Ok(DeviceDto.MapFrom(output));
        }

        /// <summary>
        /// Retrieves an device and its parameters by the external point id of a point that belongs to the device
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="externalPointId"></param>
        /// <returns></returns>
        [HttpGet("externalPointId/{externalPointId}")]
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        [SwaggerOperation(OperationId = "getDeviceByExternalPointId", Tags = new[] { "Devices" })]
        public async Task<ActionResult<DeviceDto>> GetByExternalPointIdAsync(
            [FromRoute] Guid siteId, 
            [FromRoute] string externalPointId)
        {
            var output = await _assetService.GetDeviceByExternalPointIdAsync(siteId, externalPointId);

            if (output == null)
            {
                throw new ResourceNotFoundException("Device", externalPointId);
            }

            return Ok(DeviceDto.MapFrom(output));
        }

        /// <summary>
        /// Updates an existing device with the specified id
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="id"></param>
        /// <param name="deviceMetadata"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<DeviceDto>> PutAsync(
            [FromRoute] Guid siteId, 
            [FromRoute] Guid id, 
            [FromBody] DeviceMetadataDto deviceMetadata)
        {
            if (deviceMetadata == null)
            {
                throw new BadRequestException("Request body not provided, or invalid.");
            }
            var output = await _assetService.UpdateDeviceMetadataAsync(siteId, id, deviceMetadata);
            return Ok(DeviceDto.MapFrom(output));
        }
    }
}
