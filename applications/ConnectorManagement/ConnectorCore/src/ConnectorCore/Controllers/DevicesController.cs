namespace ConnectorCore.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Devices.
    /// </summary>
    [Route("sites/{siteId}/[controller]")]
    [ApiController]
    public class DevicesController : Controller
    {
        private readonly IDevicesService devicesService;

        internal DevicesController(IDevicesService devicesService)
        {
            this.devicesService = devicesService;
        }

        /// <summary>
        /// List all devices on site.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="includePoints">Pass true to include points for the device.</param>
        /// <param name="isEnabled">If specified - returns only devices with matching IsEnabled flag.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<DeviceEntity>>> Get(Guid siteId, [FromQuery] bool? includePoints, [FromQuery] bool? isEnabled)
        {
            return (await devicesService.GetListBySiteIdAsync(siteId, includePoints, isEnabled)).ToList();
        }

        /// <summary>
        /// Find device by Id.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="deviceId">Id of device to be retrieved.</param>
        /// <param name="includePoints">Pass true to include points for the device.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="404">Device not found.</response>
        /// <response code="400">Device id provided is not a valid GUID.</response>
        [HttpGet("{deviceId}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeviceEntity>> Get(Guid siteId, Guid deviceId, [FromQuery] bool? includePoints)
        {
            var device = await devicesService.GetItemAsync(deviceId, includePoints);

            if (device == null || device.SiteId != siteId)
            {
                return NotFound();
            }

            return device;
        }

        /// <summary>
        /// Find device by External Point Id.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="externalPointId">External id of a point related to the device.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="404">Device not found.</response>
        /// <response code="400">Site id provided is not a valid GUID.</response>
        [HttpGet("externalPointId/{externalPointId}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeviceEntity>> GetByExternalPointId(Guid siteId, string externalPointId)
        {
            var device = await devicesService.GetByExternalPointId(siteId, externalPointId);

            if (device == null || device.SiteId != siteId)
            {
                return NotFound();
            }

            return device;
        }

        /// <summary>
        /// Find device by Id.
        /// </summary>
        /// <param name="connectorId">Id of the connector.</param>
        /// <param name="deviceId">Id of device to be retrieved.</param>
        /// <param name="includePoints">Pass true to include points for the device.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="404">Device not found.</response>
        /// <response code="400">Device id provided is not a valid GUID.</response>
        [HttpGet("/connectors/{connectorId}/Devices/{deviceId}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeviceEntity>> GetDeviceByConnector(Guid connectorId, Guid deviceId, [FromQuery] bool? includePoints)
        {
            var device = await devicesService.GetItemAsync(deviceId, includePoints);

            if (device == null || device.ConnectorId != connectorId)
            {
                return NotFound();
            }

            return device;
        }

        /// <summary>
        /// List all devices on connector.
        /// </summary>
        /// <param name="connectorId">Id of the connector.</param>
        /// <param name="includePoints">Pass true to include points for the device.</param>
        /// <param name="isEnabled">If specified - returns only devices with matching IsEnabled flag.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/connectors/{connectorId}/Devices")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<DeviceEntity>>> GetDevicesByConnector(Guid connectorId, [FromQuery] bool? includePoints, [FromQuery] bool? isEnabled)
        {
            return (await devicesService.GetListByConnectorIdAsync(connectorId, includePoints, isEnabled)).ToList();
        }

        /// <summary>
        /// Update exists device.
        /// </summary>
        /// <param name="device">Device object to be updated.</param>
        /// <returns>Updated device.</returns>
        /// <response code="200">Successfully updated.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="400">Invalid input.</response>
        /// <response code="404">Device not found.</response>
        [HttpPut]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<ActionResult<DeviceEntity>> Put([FromForm] DeviceEntity device)
        {
            try
            {
                device = await devicesService.UpdateAsync(device);
                return Ok(device);
            }
            catch (ArgumentException e)
            {
                return BadRequest(new ValidationProblemDetails
                { Title = "Data validation error", Detail = e.Message });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}
