namespace ConnectorCore.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using ConnectorCore.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Willow.Infrastructure.Exceptions;

    /// <summary>
    /// Get points.
    /// </summary>
    [ApiController]
    [Route("sites/{siteId}/[controller]")]
    public class PointsController : Controller
    {
        private readonly IPointsService pointsService;

        internal PointsController(IPointsService pointsService)
        {
            this.pointsService = pointsService;
        }

        /// <summary>
        /// List all points on site.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="equipmentId">If provided filters points by equipmentId.</param>
        /// <param name="continuationToken">If provided fetches the next page of data.</param>
        /// <param name="pageSize">If provided overrides default page size.</param>
        /// <param name="includeEquipment">If true includes related equipment in the response.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<GetPointsResult>> Get(Guid siteId, [FromQuery] Guid equipmentId, [FromQuery] string continuationToken, [FromQuery] int? pageSize, [FromQuery] bool? includeEquipment)
        {
            if (equipmentId != Guid.Empty)
            {
                return await pointsService.GetListByEquipmentIdAsync(siteId, equipmentId, continuationToken, pageSize, includeEquipment);
            }

            return await pointsService.GetListBySiteIdAsync(siteId, continuationToken, pageSize, includeEquipment);
        }

        /// <summary>
        /// Find point by Entity Id.
        /// </summary>
        /// <param name="pointEntityId">Id of the point to be retrieved.</param>
        /// <param name="includeDevice">Pass true to include referenced device object.</param>
        /// <param name="includeEquipment">Pass true to include referenced equipment object.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="404">Point not found.</response>
        /// <response code="400">Invalid input.</response>
        [HttpGet("/[controller]/{pointEntityId}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PointEntity>> GetPointByEntityId(Guid pointEntityId, [FromQuery] bool? includeDevice, [FromQuery] bool? includeEquipment)
        {
            var point = await pointsService.GetAsync(pointEntityId, includeDevice, includeEquipment);
            if (point == null)
            {
                return NotFound();
            }

            return point;
        }

        /// <summary>
        /// Find point by Entity Id.
        /// </summary>
        /// <param name="pointId">Id of the point to be retrieved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        [Obsolete]
        [HttpGet("/temp/points/{pointId}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PointEntity>> GetPointById(Guid pointId)
        {
            var point = await pointsService.GetByIdAsync(pointId);
            if (point == null)
            {
                throw new ResourceNotFoundException("point", pointId);
            }

            return point;
        }

        /// <summary>
        /// Find point by Ids.
        /// </summary>
        /// <param name="pointEntityIds">Ids of the point to be retrieved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="404">Point not found.</response>
        /// <response code="400">Invalid input.</response>
        [HttpGet("/[controller]")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PointEntity>>> Get([FromQuery(Name = "pointEntityId")] Guid[] pointEntityIds)
        {
            var points = await pointsService.GetListAsync(pointEntityIds);
            var result = points.ToList();
            return result;
        }

        /// <summary>
        /// Find points by tag name.
        /// </summary>
        /// <param name="siteId">Site Id.</param>
        /// <param name="tagName">Tag to search points.</param>
        /// <param name="includeEquipment">Include equipment data into response.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="400">Invalid input.</response>
        [HttpGet("/sites/{siteId}/[controller]/byTag/{tagName}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PointEntity>>> GetByTagName([FromRoute] Guid siteId, [FromRoute] string tagName, [FromQuery] bool? includeEquipment)
        {
            var points = await pointsService.GetByTagNameAsync(siteId, tagName, includeEquipment);
            return points;
        }

        /// <summary>
        /// List all points for the given connector.
        /// </summary>
        /// <param name="connectorId">Id of the connector.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/connectors/{connectorId}/[controller]")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<PointEntity>>> GetByConnector(Guid connectorId)
        {
            return (await pointsService.GetListByConnectorIdAsync(connectorId)).ToList();
        }

        /// <summary>
        /// List all points on site for the given device.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="deviceId">Id of the device.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/sites/{siteId}/devices/{deviceId}/[controller]")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<PointEntity>>> Get(Guid siteId, Guid deviceId)
        {
            return (await pointsService.GetListAsync(siteId, deviceId)).ToList();
        }

        /// <summary>
        /// List all points on site for the given device.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="externalPointId">Id of the device.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/sites/{siteId}/[controller]/{externalPointId}/identifier")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<PointIdentifier>> Get(Guid siteId, string externalPointId)
        {
            return await pointsService.GetPointIdentifierByExternalPointIdAsync(siteId, externalPointId);
        }
    }
}
