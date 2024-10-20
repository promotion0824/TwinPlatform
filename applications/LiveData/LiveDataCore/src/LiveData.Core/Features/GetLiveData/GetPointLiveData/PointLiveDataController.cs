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
    using Willow.LiveData.Core.Infrastructure.Attributes;

    /// <summary>
    /// Gets point live data.
    /// </summary>
    /// <param name="dataService">An instance of the live data service.</param>
    [Route("api/livedata")]
    [ApiController]
    public class PointLiveDataController(ILiveDataService dataService) : Controller
    {
        /// <summary>
        /// Retrieves all rows by analogue point.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="pointEntityId">The point entity ID.</param>
        /// <param name="selectedInterval">The selected interval.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("point/analog/{pointEntityId}")]
        [ProducesResponseType(typeof(TimeSeriesAnalogData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetAnalog(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromRoute] Guid pointEntityId,
            [FromQuery(Name = "interval")] string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesDataByTrendIdAsync(
                clientId,
                start,
                end,
                Constants.Analog,
                pointEntityId,
                interval);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves all rows by analog point.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="pointEntityIds">A list of point entity IDs.</param>
        /// <param name="selectedInterval">The selected interval.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("point/analog")]
        [ProducesResponseType(typeof(TimeSeriesAnalogData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetAnalogBulk(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "pointEntityId"), BindRequired] Guid[] pointEntityIds,
            [FromQuery(Name = "interval")] string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesDataByTrendIdAsync(
                clientId,
                start,
                end,
                Constants.Analog,
                pointEntityIds,
                interval);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves all rows by binary point.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="pointEntityId">The point entity ID.</param>
        /// <param name="selectedInterval">The selected interval.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("point/binary/{pointEntityId}")]
        [ProducesResponseType(typeof(TimeSeriesBinaryData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> Get(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromRoute] Guid pointEntityId,
            [FromQuery(Name = "interval")] string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesDataByTrendIdAsync(
                clientId,
                start,
                end,
                Constants.Binary,
                pointEntityId,
                interval);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves all rows by binary point.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="pointEntityIds">A list of point entity IDs.</param>
        /// <param name="selectedInterval">The selected interval.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("point/binary")]
        [ProducesResponseType(typeof(TimeSeriesBinaryData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetBinaryBulk(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "pointEntityId"), BindRequired] Guid[] pointEntityIds,
            [FromQuery(Name = "interval")] string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesDataByTrendIdAsync(
                clientId,
                start,
                end,
                Constants.Binary,
                pointEntityIds,
                interval);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves all rows by mulit-state point.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="pointEntityId">The point entity ID.</param>
        /// <param name="selectedInterval">The selected interval.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("point/multistate/{pointEntityId}")]
        [ProducesResponseType(typeof(TimeSeriesMultiStateData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetMultiState(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromRoute] Guid pointEntityId,
            [FromQuery(Name = "interval")] string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesDataByTrendIdAsync(
                clientId,
                start,
                end,
                Constants.MultiState,
                pointEntityId,
                interval);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves all rows by multistate point.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="pointEntityIds">A list of point entity IDs.</param>
        /// <param name="selectedInterval">The selected interval.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("point/multistate")]
        [ProducesResponseType(typeof(TimeSeriesMultiStateData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetMultiStateBulk(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "pointEntityId"), BindRequired] Guid[] pointEntityIds,
            [FromQuery(Name = "interval")] string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesDataByTrendIdAsync(
                clientId,
                start,
                end,
                Constants.MultiState,
                pointEntityIds,
                interval);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves all rows by binary point.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="pointEntityId">The point entity ID.</param>
        /// <param name="selectedInterval">The selected interval.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("point/sum/{pointEntityId}")]
        [ProducesResponseType(typeof(TimeSeriesSumData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetSum(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromRoute] Guid pointEntityId,
            [FromQuery(Name = "interval")] string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesDataByTrendIdAsync(
                clientId,
                start,
                end,
                Constants.Sum,
                pointEntityId,
                interval);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves all rows by binary point.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="pointEntityIds">A list of point entity IDs.</param>
        /// <param name="selectedInterval">The selected interval.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("point/sum")]
        [ProducesResponseType(typeof(TimeSeriesSumData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetSumBulk(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "pointEntityId"), BindRequired] Guid[] pointEntityIds,
            [FromQuery(Name = "interval")] string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesDataByTrendIdAsync(
                clientId,
                start,
                end,
                Constants.Sum,
                pointEntityIds,
                interval);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves all rows by point type.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="pointEntityIds">A list of point entity IDs.</param>
        /// <param name="pointTypes">A list of point types.</param>
        /// <param name="selectedInterval">The selected interval.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("point/aggregate")]
        [ProducesResponseType(typeof(Dictionary<Guid, List<TimeSeriesData>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetPointsAggregate(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "pointEntityIds"), BindRequired] Guid[] pointEntityIds,
            [FromQuery(Name = "pointTypes"), BindRequired] string[] pointTypes,
            [FromQuery(Name = "interval")] string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;
            if (pointTypes.Length != pointEntityIds.Length)
            {
                return BadRequest($"Number of {nameof(pointEntityIds)} should be the same as {nameof(pointTypes)}");
            }

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesDataByTrendIdAsync(
                clientId,
                start,
                end,
                pointEntityIds,
                pointTypes,
                interval);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves count of points from list that have data since specified timestamp.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="pointEntityIds">A list of point entity IDs.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Obsolete("This method is deprecated and is not supported for ADX.")]
        [HttpPost("point/stats")]
        [ProducesResponseType(typeof(TimeSeriesBinaryData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<ActionResult<int>> GetPointStatsDataByIds(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromBody] List<Guid> pointEntityIds)
        {
            var liveDataService = dataService.GetLiveDataService(clientId);
            return this.Json(await liveDataService.GetPointsStatsCountByIdsListAsync(clientId, start, pointEntityIds));
        }

        /// <summary>
        /// Retrieves count of points per site that have data since specified timestamp.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Obsolete("This method is deprecated and is not supported for ADX.")]
        [HttpGet("point/stats")]
        [ProducesResponseType(typeof(TimeSeriesBinaryData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<PointStatsData>>> GetPointStatsData(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start)
        {
            var liveDataService = dataService.GetLiveDataService(clientId);
            return await liveDataService.GetPointStatsAsync(clientId, start);
        }

        /// <summary>
        /// Retrieves all rows by analog points for siteId.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="pointEntityId">The point entity ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="continuationToken">The paging continuation token.</param>
        /// <param name="pageSize">The page size.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("points/{pointEntityId}/trendlog")]
        [ProducesResponseType(typeof(GetTrendlogResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetTrendlog(
            [FromQuery(Name = "clientId")] Guid? clientId,
            Guid pointEntityId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "continuationToken")] string continuationToken,
            [FromQuery] int? pageSize)
        {
            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesRawDataByTrendIdAsync(
                clientId,
                pointEntityId,
                start,
                end,
                continuationToken,
                pageSize);

            if (result.Data == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves all rows by analogue points for siteId.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="siteId">The site ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="pointId">A list of point entity IDs.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Obsolete]
        [HttpGet("sites/{siteId}/trendlogs")]
        [ProducesResponseType(typeof(GetTrendlogsResultItem[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetTrendlogs(
            [FromQuery(Name = "clientId")] Guid? clientId,
            Guid siteId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "pointId")] List<Guid> pointId = null)
        {
            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesRawDataBySiteIdAsync(
                clientId,
                siteId,
                start,
                end,
                pointId?.Any() is true ? pointId : null);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves site time series data inside time intervals with calculated based on aggregationType value.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="intervalStr">The selected interval.</param>
        /// <param name="pointIds">A list of point entity IDs.</param>
        /// <param name="aggregationTypeStr">The type of aggregation.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("points/aggregatedTimeseriesData")]
        [ProducesResponseType(typeof(GetTrendlogsResultItem[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetAggregatedPointsInsideIntervalAsync(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromQuery(Name = "interval")] string intervalStr,
            [FromQuery(Name = "pointId")] List<Guid> pointIds = null,
            [FromQuery(Name = "aggregationType")] string aggregationTypeStr = "avg")
        {
            if (string.IsNullOrWhiteSpace(intervalStr))
            {
                return this.BadRequest("Interval is required");
            }

            if (!TimeSpan.TryParse(intervalStr, out var interval))
            {
                return this.BadRequest("Invalid format of interval");
            }

            if (!Enum.TryParse(typeof(AggregationType), aggregationTypeStr, true, out var aggregationTypeObj))
            {
                return this.BadRequest("Invalid aggregation type");
            }

            if (pointIds?.Any() != true)
            {
                return this.BadRequest("PointId filter is required");
            }

            if (clientId == Guid.Empty)
            {
                return this.BadRequest("ClientId is required");
            }

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetAggregatedValuesInsideTimeIntervalsAsync(
                clientId,
                start,
                end,
                interval,
                pointIds,
                (AggregationType)aggregationTypeObj!);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves connector's raw telemetry data.
        /// </summary>
        /// <param name="clientId">The client ID.</param>
        /// <param name="start">The start date and time.</param>
        /// <param name="end">The end date and time.</param>
        /// <param name="payload">The telemetry request body.</param>
        /// <param name="pagesize">The page size.</param>
        /// <param name="continuationToken">The paging continuation token.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("telemetry")]
        [DateRangeValidation]
        [TelemetryInputValidation]
        [ServiceFilter(typeof(PageSizeValidationAttribute))]
        [ProducesResponseType(typeof(GetTelemetryResult[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public async Task<IActionResult> GetTelemetryAsync(
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "start"), BindRequired] DateTime start,
            [FromQuery(Name = "end"), BindRequired] DateTime end,
            [FromBody, BindRequired] TelemetryRequestBody payload,
            [FromQuery(Name = "pageSize")] int pagesize,
            [FromQuery(Name = "continuationToken")] string continuationToken = "")
        {
            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTelemetryAsync(
                clientId,
                payload,
                start,
                end,
                pagesize,
                continuationToken);

            return this.Ok(result);
        }

        /// <summary>
        /// Retrieves all rows by external id.
        /// </summary>
        /// <param name="connectorId">The connector ID.</param>
        /// <param name="externalId">The external ID.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="startUtc">start UTC date.</param>
        /// <param name="endUtc">end UTC date.</param>
        /// <param name="selectedInterval"> this represents a timespan e.g. `01:00:00`. </param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [HttpGet("point/analog/{connectorId}/{externalId}")]
        [ProducesResponseType(typeof(TimeSeriesAnalogData[]), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status403Forbidden)]
        [Produces("application/json")]
        public async Task<IActionResult> GetLiveDataByExternalId(
            [FromRoute] Guid connectorId,
            [FromRoute] string externalId,
            [FromQuery(Name = "clientId")] Guid? clientId,
            [FromQuery(Name = "startUtc"), BindRequired] DateTime startUtc,
            [FromQuery(Name = "endUtc"), BindRequired] DateTime endUtc,
            [FromQuery(Name = "interval")] string selectedInterval)
        {
            var interval = string.IsNullOrWhiteSpace(selectedInterval) ||
                           !TimeSpan.TryParse(selectedInterval, out var parsedInterval)
                ? (TimeSpan?)null
                : parsedInterval;

            var liveDataService = dataService.GetLiveDataService(clientId);
            var result = await liveDataService.GetTimeSeriesDataByExternalIdAsync(
                connectorId,
                externalId,
                clientId,
                startUtc,
                endUtc,
                interval);

            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }
    }
}
