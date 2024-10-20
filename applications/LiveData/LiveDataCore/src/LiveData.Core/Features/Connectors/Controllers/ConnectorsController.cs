namespace Willow.LiveData.Core.Features.Connectors.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Willow.LiveData.Core.Features.Connectors.DTOs;
    using Willow.LiveData.Core.Features.Connectors.Interfaces;
    using Willow.LiveData.Core.Infrastructure.Attributes;

    /// <summary>
    /// Connector controller for getting connector status, unique trends, and missing trends.
    /// </summary>
    /// <param name="connectorService">A connector service instance.</param>
    [Route("api/livedata/stats")]
    [ApiController]
    public class ConnectorsController(IConnectorService connectorService) : Controller
    {
        /// <summary>
        /// Gets the connector status.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="connectorList">A list of connector IDs.</param>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        /// <param name="singleBin">Single bin.</param>
        /// <returns>A connector status result.</returns>
        [HttpGet("connectors")]
        [DateRangeValidation]
        [ConnectorListValidation]
        [ProducesResponseType(typeof(ConnectorStatusResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetConnectorStatusAsync(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromBody] ConnectorList connectorList,
            [FromQuery(Name = "start")] DateTime? start,
            [FromQuery(Name = "end")] DateTime? end,
            [FromQuery(Name = "binFullInterval")] string singleBin)
        {
            var result = await connectorService.GetConnectorStatusAsync(clientId, connectorList, start, end, singleBin);

            return Ok(result);
        }

        /// <summary>
        /// Gets unique trends.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="connectorList">A list of connector IDs.</param>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        /// <returns>A unique trends result.</returns>
        [HttpGet("uniqueTrends")]
        [DateRangeValidation]
        [ConnectorListValidation]
        [ProducesResponseType(typeof(UniqueTrendsResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetUniqueTrendsAsync(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromBody] ConnectorList connectorList,
            [FromQuery(Name = "start")] DateTime? start,
            [FromQuery(Name = "end")] DateTime? end)
        {
            var result = await connectorService.GetUniqueTrendsAsync(clientId, connectorList, start, end);

            return Ok(result);
        }

        /// <summary>
        /// Gets missing trends.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="connectorList">A list of connector IDs.</param>
        /// <param name="start">The start time.</param>
        /// <param name="end">The end time.</param>
        /// <returns>A missing trends result.</returns>
        [HttpGet("missingTrends")]
        [DateRangeValidation]
        [ConnectorListValidation]
        [ProducesResponseType(typeof(MissingTrendsResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetMissingTrendsAsync(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromBody] ConnectorList connectorList,
            [FromQuery(Name = "start")] DateTime? start,
            [FromQuery(Name = "end")] DateTime? end)
        {
            var result = await connectorService.GetMissingTrendsAsync(clientId, connectorList, start, end);

            return Ok(result);
        }
    }
}
