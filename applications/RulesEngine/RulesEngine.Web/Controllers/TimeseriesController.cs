using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RulesEngine.Web;
using RulesEngine.Web.DTO;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;
using Willow.Rules.Sources;
using WillowRules.DTO;

namespace Willow.Rules.Web.Controllers;

/// <summary>
/// Controller for time series data
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = nameof(CanViewInsights))]
[ApiExplorerSettings(GroupName = "v1")]
public class TimeseriesController : ControllerBase
{
    private readonly ILogger<TimeseriesController> logger;
    private readonly WillowEnvironment willowEnvironment;
    private readonly IRepositoryInsight repositoryInsight;
    private readonly IRepositoryInsightChange repositoryInsightChange;
    private readonly IRepositoryActorState repositoryActorState;
    private readonly IRepositoryTimeSeriesBuffer repositoryTimeSeries;
    private readonly ITwinService twinService;
    private readonly IModelService modelService;
    private readonly IADXService adxService;
    private readonly IRepositoryRuleInstances ruleInstances;

    /// <summary>
    /// Creates a new <see cref="TimeseriesController"/>
    /// </summary>
    public TimeseriesController(ILogger<TimeseriesController> logger,
        WillowEnvironment willowEnvironment,
        IRepositoryInsight repositoryInsight,
        IRepositoryActorState repositoryActorState,
        IRepositoryTimeSeriesBuffer repositoryTimeSeries,
        IRepositoryInsightChange repositoryInsightChange,
        ITwinService twinService,
        IModelService modelService,
        IADXService adxService,
        IRepositoryRuleInstances ruleInstances)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.willowEnvironment = willowEnvironment ?? throw new ArgumentNullException(nameof(willowEnvironment));
        this.repositoryInsight = repositoryInsight ?? throw new ArgumentNullException(nameof(repositoryInsight));
        this.repositoryActorState = repositoryActorState ?? throw new ArgumentNullException(nameof(repositoryActorState));
        this.repositoryTimeSeries = repositoryTimeSeries ?? throw new ArgumentNullException(nameof(repositoryTimeSeries));
        this.twinService = twinService ?? throw new ArgumentNullException(nameof(twinService));
        this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
        this.adxService = adxService ?? throw new ArgumentNullException(nameof(adxService));
        this.ruleInstances = ruleInstances ?? throw new ArgumentNullException(nameof(ruleInstances));
        this.repositoryInsightChange = repositoryInsightChange ?? throw new ArgumentNullException(nameof(repositoryInsightChange));
    }

    /// <summary>
    /// Gets timeseries data for charts for a specific rule instance
    /// </summary>
    /// <remarks>
    /// Each rule instance has an actor state with one or more time series values and calculated
    /// values from there, culminating in a final result value
    /// </remarks>
    /// <param name="ruleInstanceId">Id of the rule instance</param>
    /// <returns>An array of time series data</returns>
    [HttpGet("GetTimeSeriesData", Name = "GetTimeSeriesData")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TimeSeriesDataDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetTimeseriesData(string ruleInstanceId)
    {
        logger.LogInformation("Get time series data for rule instance {ruleInstanceId}", ruleInstanceId);

        var ruleInstance = await this.ruleInstances.GetOne(ruleInstanceId);
        if (ruleInstance is null) return NotFound($"Did not find rule instance {ruleInstanceId}");

        // Actorstate Id is 1:1 with rule instance id
        var actorState = await this.repositoryActorState.GetOne(ruleInstanceId);

        if (actorState is null)
        {
            return NotFound($"Did not find actor state for {ruleInstanceId}");
        }

        // Insight Id is 1:1 with rule instance id
        var insight = await this.repositoryInsight.GetOne(ruleInstanceId);
        var insightChanges = await repositoryInsightChange.Get(v => v.InsightId == ruleInstanceId);
        var result = ruleInstance.GetTimeseriesDataForRuleInstance(actorState, actorState.EarliestSeen, actorState.Timestamp, insight: insight, changes: insightChanges.OrderBy(v => v.Timestamp));

        logger.LogInformation("Returning time series data point count = {pdc}, time value count = {tvc}", ruleInstance.PointEntityIds.Count, actorState.TimedValues.Count());

        return Ok(result);
    }

    /// <summary>
    /// Gets timeseries data for charts for a specific capability twin
    /// </summary>
    /// <remarks>
    /// Time series data buffers are now stored independently from actor state
    /// which holds just calculated values and results
    /// </remarks>
    /// <param name="twinId">TwinId</param>
    /// <param name="startTime">An optional start time for the data else it defaults to 10 days back</param>
    /// <param name="endTime">An optional end time for the data else it defaults current time</param>
    /// <param name="timeZone">An optional timeZone for the data else it defaults to UTC</param>
    /// <param name="enableCompression">enableCompression</param>
    /// <param name="optmizeCompression">optmizeCompression</param>
    /// <returns>An array of time series data</returns>
    [HttpGet("get-timeseriesdata-for-capability", Name = "GetTimeSeriesDataForCapability")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TimeSeriesBufferDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetTimeseriesDataForCapability(string twinId, DateTime? startTime = null, DateTime? endTime = null, string timeZone = null, bool enableCompression = true, bool optmizeCompression = true)
    {
        logger.LogInformation("Get time series buffer for twin {twinId}", twinId);

        var twin = await twinService.GetCachedTwin(twinId);

        if (twin is null) return NotFound($"Could not find twin '{twinId}'");

        string uid = twin.Id;

        startTime = startTime ?? DateTime.UtcNow.AddDays(-10);
        endTime = endTime ?? DateTime.UtcNow;

        if (endTime > DateTime.UtcNow)
        {
            //dont go over DateTime.UtcNow otherwise final status might go "offline"
            endTime = DateTime.UtcNow;
        }
        else
        {
            //simulations give the last point time, add one minute to be sure to add point on the exact time stamps
            startTime = startTime.Value.AddMinutes(-1);
            endTime = endTime.Value.AddMinutes(1);
        }

        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var idFilter = new IdFilter(twin.trendID, twin.externalID, twin.connectorID);

        if(!idFilter.IsValid())
        {
            logger.LogWarning($"Twin '{twinId}' has no trend id or external id");

            return BadRequest($"Twin '{twinId}' has no trend id or external id");
        }

        var tz = !string.IsNullOrEmpty(timeZone) ? TimeZoneInfoHelper.From(timeZone) : null;
        var ontology = await modelService.GetModelGraphCachedAsync();

        var buffer = new TimeSeries(uid, twin.unit)
        {
            DtId = twinId,
            ModelId = twin.Metadata?.ModelId,
            TrendInterval = twin.trendInterval,
            //override last seen so that we dont have "Offline" at the start
            LastSeen = new DateTimeOffset(startTime.Value)
        };

        var rawBuffer = new TimeSeries(uid, twin.unit);

        var statusList = new List<(TimedValue value, TimeSeriesStatus status)>();

        //ValueOutOfRange is part of TimeSeries data quality validation,
        //which means we can't track them as those points aren't added to the timeseries
        //TimeSeriesStatus.PeriodOutOfRange is too noisy. Really online offline and stuck seems fine
        var statusPriority = new TimeSeriesStatus[]
        {
            TimeSeriesStatus.Offline
        };

        try
        {
            await foreach (var line in adxService.RunRawQuery(startTime.Value, endTime.Value, idFilter, cancellationToken.Token))
            {
                var timestamp = (tz is not null) ? line.SourceTimestamp.ConvertToDateTimeOffset(tz) : line.SourceTimestamp;

                var value = new TimedValue(timestamp, line.Value, line.TextValue);

                //dont do offline as the starting status
                if (statusList.Count > 0)
                {
                    //can never be offline if we set the status after adding the point
                    buffer.SetStatus(timestamp);
                }

                var status = buffer.GetStatus();

                buffer.SetLatencyEstimate(line.EnqueuedTimestamp.Subtract(line.SourceTimestamp));
                buffer.AddPoint(value, applyCompression: enableCompression, includeDataQualityCheck: true, reApplyCompression: optmizeCompression);
                rawBuffer.AddPoint(value, applyCompression: enableCompression, includeDataQualityCheck: false, reApplyCompression: false);

                //set status again otherwise status like "stuck" is not accurate
                if (!status.HasFlag(TimeSeriesStatus.Offline))
                {
                    buffer.SetStatus(timestamp);
                    status = buffer.GetStatus();
                }

                if (status != TimeSeriesStatus.Valid)
                {
                    //only allow one status at a time
                    status = statusPriority.FirstOrDefault(v => v.HasFlag(status));

                    if (!statusPriority.Any(v => v == status))
                    {
                        status = TimeSeriesStatus.Valid;
                    }
                }

                statusList.Add((value, status));
            }
        }
        catch (OperationCanceledException)
        {

        }

        //check raw buffer as the actual buffer may be empty due to quality checks
        if (rawBuffer.Count == 0)
        {
            return NoContent();
        }

        buffer.SetStatus(endTime.Value);

        List<AxisDto> axes = new();

        TrendlineDto GetTrendLineDto(string name, Unit unit, IEnumerable<TimedValue> values)
        {
            string key = unit.Name;

            int axisIndex = 0;

            var axis = new AxisDto
            {
                Key = key,
                ShortName = axisIndex == 0 ? "y" : "y" + (axisIndex + 1),
                LongName = axisIndex == 0 ? "yaxis" : "yaxis" + (axisIndex + 1),
                Title = name + " " + unit.Name
            };

            axes.Add(axis);

            // If all the values are bool values this is a stepped line
            bool isStepped = unit == Unit.boolean;

            return new TrendlineDto
            {
                Id = twinId,
                Name = (name + " " + unit.Name).TrimEnd(' '),
                Unit = unit.Name,
                IsOutput = false,
                IsRanking = false,
                Axis = axis.ShortName,
                Shape = isStepped ? "hv" : "linear",
                Data = values.Select(v => new TimedValueDto(v)).ToList()
            };
        }

        var points = buffer.Points;

        var trendLine = GetTrendLineDto(twin.name, Unit.Get(twin.unit), points);
        var rawTrendLine = GetTrendLineDto(twin.name, Unit.Get(twin.unit), rawBuffer.Points);

        TrendlineStatusDto statusDto = null;

        trendLine.Statuses = new List<TrendlineStatusDto>();

        //remove unneccessary valid points
        statusList.RemoveAll(v => v.status == TimeSeriesStatus.Valid && !points.Any(p => p.Timestamp == v.value.Timestamp));

        //now reduce to statuses and its time period of values
        statusList.Aggregate(statusDto, (s, v) =>
        {
            if (statusDto is null || statusDto.Status != v.status)
            {
                var statusLine = GetTrendLineDto(twin.name, Unit.Get(twin.unit), new List<TimedValue>());

                if (statusDto is not null)
                {
                    //add last point from previous status to draw the starting line
                    statusLine.Data.Add(statusDto.Values.Data.Last());
                }

                statusDto = new TrendlineStatusDto()
                {
                    Status = v.status,
                    Values = statusLine
                };

                trendLine.Statuses.Add(statusDto);
            }

            //always snap to an existing point after compression
            statusDto.Values.Data.Add(new TimedValueDto(points.First(p => p.Timestamp >= v.value.Timestamp)));

            return statusDto;
        });

        logger.LogInformation("Returning time series buffer count = {tvc}", points.Count());

        return Ok(new TimeSeriesBufferDto(twinId, new TimeSeriesDto(buffer), trendLine, rawTrendLine, timeZone));
    }

    /// <summary>
    /// Gets timeseries summaries
    /// </summary>
    /// <remarks>
    /// Time series summaries are just the statistical data about a time series
    /// allowing for a display where we filter, sort, ... by id, unit of measure, max, min, ...
    /// </remarks>
    /// <returns>An array of time series summaries</returns>
    [HttpPost("get-timeseries-summaries", Name = "GetTimeSeriesSummaries")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchDto<TimeSeriesDto>))]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> GetTimeSeriesSummaries(BatchRequestDto request)
    {
        var batch = await GetTimeSeriesSummariesBatch(request);

        return Ok(batch);
    }

    /// <summary>
    /// Exports timeseries summaries
    /// </summary>
    [HttpPost("export-timeseries-summaries", Name = "ExportTimeSeriesSummaries")]
    [FileResultContentType("text/csv")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> ExportTimeSeriesSummaries(BatchRequestDto request)
    {
        var batch = await GetTimeSeriesSummariesBatch(request);

        return WebExtensions.CsvResult(batch.Items.Select(v =>
        {
            dynamic expando = new ExpandoObject();

            expando.Id = v.Id;
            expando.DtId = v.DtId;
            expando.ModelId = v.ModelId;
            expando.Count = v.TotalValuesProcessed;
            expando.Period = v.EstimatedPeriod;
            expando.Latency = v.Latency;
            expando.TrendInterval = v.TrendInterval;
            expando.Min = v.MinValue;
            expando.Average = v.AverageValue;
            expando.Max = v.MaxValue;
            expando.Unit = v.UnitOfMeasure;
            expando.Status = v.Status;
            expando.LastSeen = v.EndTime;

            var expandoLookup = (IDictionary<string, object>)expando;

            foreach (var location in v.TwinLocations.GroupLocationsByModel())
            {
                expandoLookup[location.Key] = location.Value;
            }

            return expando;

            }), "Capabilities.csv");
    }

    private async Task<BatchDto<TimeSeriesDto>> GetTimeSeriesSummariesBatch(BatchRequestDto request)
    {
        logger.LogInformation("Get time series summaries {page} {take}", request.Page, request.PageSize);

        var batch = await this.repositoryTimeSeries.GetAll(
            request.SortSpecifications,
            request.FilterSpecifications,
            page: request.Page,
            take: request.PageSize);

        var result = batch.Transform((x) => new TimeSeriesDto(x));

        return new BatchDto<TimeSeriesDto>(result);
    }
}
