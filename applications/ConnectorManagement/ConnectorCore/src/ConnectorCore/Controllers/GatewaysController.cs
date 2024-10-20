namespace ConnectorCore.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Data;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using ConnectorCore.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Gateways.
    /// </summary>
    [Route("sites/{siteId}/[controller]")]
    [ApiController]
    public class GatewaysController : Controller
    {
        private readonly IGatewaysService gatewaysService;
        private readonly IConnectorCoreDbContext connectorCoreDb;

        internal GatewaysController(IGatewaysService gatewaysService, IConnectorCoreDbContext connectorCoreDb)
        {
            this.gatewaysService = gatewaysService;
            this.connectorCoreDb = connectorCoreDb;
        }

        /// <summary>
        /// List all gateways on site.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
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
        public async Task<ActionResult<IEnumerable<GatewayEntity>>> GetListAsync([FromRoute] Guid siteId, [FromQuery] bool? isEnabled)
        {
            return (await gatewaysService.GetListBySiteIdAsync(new[] { siteId }, isEnabled))[siteId].ToList();
        }

        /// <summary>
        /// List all gateways/connector(if gateways is null) on sites.
        /// </summary>
        /// <param name="siteIds">Id of the sites.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/siteConnectivityStatistics")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<ConnectivityStatistics>>> GetGatewayConnectorListAsync([FromQuery] List<Guid> siteIds)
        {
            var connectivityStatisticsList = new List<ConnectivityStatistics>();
            var gateways = await gatewaysService.GetListBySiteIdAsync(siteIds, null);

            foreach (var siteId in siteIds)
            {
                var gatewaysTemp = gateways[siteId].ToList();
                var connectorsTemp = new List<ConnectorEntity>();
                if (!gatewaysTemp.Any())
                {
                    connectorsTemp = await connectorCoreDb.Connectors.Where(x => x.SiteId == siteId).Select(x => x.ToConnectorEntity()).ToListAsync();
                }

                connectivityStatisticsList.Add(
                    new ConnectivityStatistics
                    {
                        SiteId = siteId,
                        Gateways = gatewaysTemp,
                        Connectors = connectorsTemp,
                    });
            }

            return connectivityStatisticsList;
        }

        /// <summary>
        /// Find gateway by Id.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="gatewayId">Id of gateway to be retrieved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="404">Device not found.</response>
        /// <response code="400">Device id provided is not a valid GUID.</response>
        [HttpGet("{gatewayId}")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GatewayEntity>> GetAsync([FromRoute] Guid siteId, [FromRoute] Guid gatewayId)
        {
            var gateway = await gatewaysService.GetItemAsync(gatewayId);

            if (gateway == null || gateway.SiteId != siteId)
            {
                return NotFound();
            }

            return gateway;
        }

        /// <summary>
        /// List all gateways for connector.
        /// </summary>
        /// <param name="connectorId">Id of the connector.</param>
        /// <param name="isEnabled">If specified - returns only gateways with matching IsEnabled flag.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        [HttpGet("/connectors/{connectorId}/[controller]")]
        [Authorize]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<GatewayEntity>>> GetGatewaysByConnectorAsync([FromRoute] Guid connectorId, [FromQuery] bool? isEnabled)
        {
            return (await gatewaysService.GetListByConnectorIdAsync(connectorId, isEnabled)).ToList();
        }

        /// <summary>
        /// Update gateway heartbeat.
        /// </summary>
        /// <param name="siteId">Id of the site.</param>
        /// <param name="gatewayId">Id of gateway to be retrieved.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <response code="200">Successful operation.</response>
        /// <response code="500">Internal server error.</response>
        /// <response code="404">Device not found.</response>
        /// <response code="400">Device id provided is not a valid GUID.</response>
        [HttpPut("{gatewayId}/heartbeat")]
        [Authorize]
        [Produces("application/json")]
        [ProducesErrorResponseType(typeof(void))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DateTime>> PutHeartbeatAsync([FromRoute] Guid siteId, [FromRoute] Guid gatewayId)
        {
            var gateway = await gatewaysService.GetItemAsync(gatewayId);

            if (gateway == null || gateway.SiteId != siteId)
            {
                return NotFound();
            }

            gateway.LastHeartbeatTime = DateTime.UtcNow;
            await gatewaysService.UpdateAsync(gateway);

            return gateway.LastHeartbeatTime;
        }
    }
}
