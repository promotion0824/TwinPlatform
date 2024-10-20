namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Willow.LiveData.Core.Common;
    using Willow.LiveData.Core.Domain;

    /// <summary>
    /// Controller for retrieving last trend logs.
    /// </summary>
    [Route("api/livedata")]
    [ApiController]
    public class LastTrendLogsController(ILiveDataService dataService) : Controller
    {
        /// <summary>
        /// Retrieves all rows by analogue points for siteId.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <param name="pointId">The point identifier.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Obsolete]
        [HttpPost("sites/{siteId}/lastTrendlogs")]
        [ProducesResponseType(typeof(PointTimeSeriesRawData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetLastTrendlogs(
            [FromQuery(Name = "clientId")] Guid? clientId,
            Guid siteId,
            [FromQuery(Name = "pointId")] List<Guid> pointId)
        {
            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetLastTimeSeriesRawDataBySiteIdAsync(clientId, siteId, pointId.Any() ? pointId : null);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieves last trend logs between start and end dates.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="start">The start date.</param>
        /// <param name="end">The end date.</param>
        /// <param name="pointIds">The list of point IDs.</param>
        /// <returns>Trend logs for the given points between the start and end date.</returns>
        [HttpGet("points/historicalLastTrendlogs")]
        [ProducesResponseType(typeof(PointTimeSeriesRawData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetHistoricalLastTrendlogs(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "pointId")] List<Guid> pointIds)
        {
            if (pointIds?.Any() != true)
            {
                return BadRequest("PointId filter is required.");
            }

            if (clientId == Guid.Empty)
            {
                return BadRequest("ClientId is required");
            }

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetHistoricalLastTimeSeriesRawDataAsync(
                clientId,
                start,
                end,
                pointIds);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
