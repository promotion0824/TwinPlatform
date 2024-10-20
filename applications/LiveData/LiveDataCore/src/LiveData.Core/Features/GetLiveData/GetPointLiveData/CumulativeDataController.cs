namespace Willow.LiveData.Core.Features.GetLiveData.GetPointLiveData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Willow.LiveData.Core.Common;

/// <summary>
/// Controller for cumulative data (used by mining team).
/// </summary>
/// <param name="dataService">Livedata service.</param>
[Route("api/livedata")]
[ApiController]
public class CumulativeDataController(ILiveDataService dataService) : Controller
{
    /// <summary>
    /// Retrieves cumulative data divided by intervals.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="start">The start date and time of the time series data.</param>
    /// <param name="end">The end date and time of the time series data.</param>
    /// <param name="intervalStr">The interval.</param>
    /// <param name="pointIds">The list of TrendIds for which to retrieve the data. If null, retrieves data for all points.</param>
    /// <param name="valueMultiplier">The value multiplier.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Obsolete("Not supported anymore")]
    [HttpGet("points/cumulativeTrend")]
    [ProducesResponseType(typeof(IReadOnlyList<CumulativeTimeSeriesDataPoint>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<IActionResult> GetCumulativeTrendAsync(
        [FromQuery(Name = "clientId")] Guid? clientId,
        [FromQuery(Name = "start"), BindRequired] DateTime start,
        [FromQuery(Name = "end"), BindRequired] DateTime end,
        [FromQuery(Name = "interval")] string intervalStr,
        [FromQuery(Name = "pointId")] List<Guid> pointIds = null,
        [FromQuery(Name = "valueMultiplier")] double valueMultiplier = 1)
    {
        if (string.IsNullOrWhiteSpace(intervalStr))
        {
            return BadRequest("Interval is required");
        }

        if (!TimeSpan.TryParse(intervalStr, out var interval))
        {
            return BadRequest("Invalid format of interval");
        }

        if (pointIds?.Any() != true)
        {
            return BadRequest("PointId filter is required");
        }

        if (start >= end)
        {
            return BadRequest("Start date must be before end date");
        }

        var liveDataService = dataService.GetLiveDataService(clientId);
        var result = await liveDataService.GetCumulativeTrendAsync(
            clientId,
            start,
            end,
            interval,
            pointIds,
            valueMultiplier: valueMultiplier);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Retrieves accumulation over selected period of time.
    /// </summary>
    /// <param name="clientId">The client ID.</param>
    /// <param name="start">The start date and time of the time series data.</param>
    /// <param name="end">The end date and time of the time series data.</param>
    /// <param name="intervalStr">The interval.</param>
    /// <param name="pointIds">The list of TrendIds for which to retrieve the data. If null, retrieves data for all points.</param>
    /// <param name="valueMultiplier">The value multiplier.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Obsolete("Not supported anymore")]
    [HttpGet("points/cumulativeSum")]
    [ProducesResponseType(typeof(IReadOnlyList<TimeSeriesDataPoint>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UnauthorizedResult), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<IActionResult> GetCumulativeSumAsync(
        [FromQuery(Name = "clientId")] Guid? clientId,
        [FromQuery(Name = "start"), BindRequired] DateTime start,
        [FromQuery(Name = "end"), BindRequired] DateTime end,
        [FromQuery(Name = "interval")] string intervalStr,
        [FromQuery(Name = "pointId")] List<Guid> pointIds = null,
        [FromQuery(Name = "valueMultiplier")] double valueMultiplier = 1)
    {
        if (string.IsNullOrWhiteSpace(intervalStr))
        {
            return BadRequest("Interval is required");
        }

        if (!TimeSpan.TryParse(intervalStr, out var interval))
        {
            return BadRequest("Invalid format of interval");
        }

        if (pointIds?.Any() != true)
        {
            return BadRequest("PointId filter is required");
        }

        if (start >= end)
        {
            return BadRequest("Start date must be before end date");
        }

        var liveDataService = dataService.GetLiveDataService(clientId);
        var result = await liveDataService.GetCumulativeSumAsync(
            clientId,
            start,
            end,
            interval,
            pointIds,
            valueMultiplier: valueMultiplier);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
