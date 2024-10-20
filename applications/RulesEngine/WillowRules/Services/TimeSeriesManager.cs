using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.DTO;
using Willow.Rules.Logging;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace Willow.Rules.Services;

/// <summary>
/// Manages time series instances
/// </summary>
public interface ITimeSeriesManager
{
	/// <summary>
	/// Gets the telemetry key for an external Id and connector Id
	/// </summary>
	string TelemetryKey(string externalId, string connectorId) => $"{externalId}_{connectorId}";

	/// <summary>
	/// Gets a time series by twin id
	/// </summary>
	bool TryGetByTwinId(string twinId, out TimeSeries? timeSeries);

	/// <summary>
	/// Loads all time series from the database into the provided buffers
	/// </summary>
	Task LoadTimeSeriesBuffers(Dictionary<string, List<RuleInstance>> instanceLookup, RuleTemplateFactory ruleTemplateFactory, DateTimeOffset startDate);

	/// <summary>
	/// Flush time series buffers after pruning
	/// </summary>
	Task FlushToDatabase(DateTime start, DateTime end, SystemSummary summary, ProgressTrackerForRuleExecution progressTracker);

	/// <summary>
	/// Apply limits to timeseries
	/// </summary>
	int ApplyLimits(TimeSeries timeseries, DateTime now);

	/// <summary>
	/// Get an existing time series buffer or add a new one if we have never seen this point before
	/// </summary>
	Task<TimeSeries?> GetOrAdd(string pointId, string connectorId, string externalId);

	/// <summary>
	/// Adds a timeseries directly to internal buffers
	/// </summary>
	/// <param name="timeSeries"></param>
	void AddToBuffers(TimeSeries timeSeries);

	/// <summary>
	/// Get a count of time series that are mapped to a twin
	/// </summary>
	int Count { get; }

	/// <summary>
	/// Get all of them
	/// </summary>
	IEnumerable<TimeSeries> BufferList { get; }

	/// <summary>
	/// Maximum days to keep for buffers
	/// </summary>
	TimeSpan MaxDaysToKeep { get; }
}

/// <summary>
/// Manages time series instances
/// </summary>
public class TimeSeriesManager : ITimeSeriesManager
{
	/// <summary>
	/// Mapped connector Id is the same for all customers and connectors using Mapped
	/// </summary>
	public const string MappedConnectorId = "00000000-35C5-4415-A4B3-7B798D0568E8";
	private const int maxNoTwinCount = 100_000;

	/// <summary>
	/// Time series buffers by trendId or externalId+connectorId
	/// as supplied by the live data
	/// </summary>
	/// <remarks>
	/// Warning, this could be very large
	/// </remarks>
	readonly Dictionary<string, TimeSeries> buffersByTrendId = new();

	readonly Dictionary<(string externalId, string connectorId), TimeSeries> buffersByTelemetryKey = new();

	readonly Dictionary<string, TimeSeries> buffersByTwinId = new();

	public string TelemetryKey(string externalId, string connectorId) => $"{externalId}_{connectorId}";

	public bool TryGetByTwinId(string twinId, out TimeSeries? timeSeries)
	{
		return buffersByTwinId.TryGetValue(twinId, out timeSeries);
	}

	private readonly IRepositoryTimeSeriesBuffer repositoryTimeSeries;
	private readonly IRepositoryTimeSeriesMapping repositoryTimeSeriesMapping;
	private readonly ILogger<TimeSeriesManager> logger;
	private readonly ILogger throttledLogger;
	private readonly ITelemetryCollector telemetryCollector;
	private readonly IModelService modelService;
	private readonly TimeSpan maxTimeToKeep;
	private HashSet<string> mappingsByTrendId = new HashSet<string>();
	private HashSet<(string, string)> mappingsByExternalId = new HashSet<(string, string)>();
	private int noTwinCount = 0;

	/// <summary>
	/// Creates a new <see cref="TimeSeriesManager" />
	/// </summary>
	public TimeSeriesManager(
		IRepositoryTimeSeriesBuffer repositoryTimeSeries,
		IRepositoryTimeSeriesMapping repositoryTimeSeriesMapping,
		ITelemetryCollector telemetryCollector,
		IModelService modelService,
		ILogger<TimeSeriesManager> logger,
		int maxDaysToKeep = 90)
	{
		this.repositoryTimeSeries = repositoryTimeSeries ?? throw new System.ArgumentNullException(nameof(repositoryTimeSeries));
		this.repositoryTimeSeriesMapping = repositoryTimeSeriesMapping ?? throw new ArgumentNullException(nameof(repositoryTimeSeriesMapping));
		this.telemetryCollector = telemetryCollector ?? throw new ArgumentNullException(nameof(telemetryCollector));
		this.modelService = modelService ?? throw new ArgumentNullException(nameof(modelService));
		this.maxTimeToKeep = TimeSpan.FromDays(maxDaysToKeep);
		this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
		this.throttledLogger = logger.Throttle(TimeSpan.FromSeconds(10));
	}

	/// <summary>
	/// Adds a loaded or new time series to all of the indexes
	/// </summary>
	public void AddToBuffers(TimeSeries timeSeries)
	{
		if (!string.IsNullOrEmpty(timeSeries.Id) &&
			Guid.TryParse(timeSeries.Id, out var guid) && !guid.Equals(Guid.Empty))
		{
			buffersByTrendId[timeSeries.Id] = timeSeries;
		}
		if (!string.IsNullOrEmpty(timeSeries.ExternalId) && !string.IsNullOrEmpty(timeSeries.ConnectorId))
		{
			buffersByTelemetryKey[(timeSeries.ExternalId, timeSeries.ConnectorId)] = timeSeries;
		}
		if (!string.IsNullOrEmpty(timeSeries.DtId))
		{
			buffersByTwinId[timeSeries.DtId] = timeSeries;
		}
	}

	/// <summary>
	/// Load the time series buffers
	/// </summary>
	public async Task LoadTimeSeriesBuffers(
		Dictionary<string, List<RuleInstance>> instanceLookup,
		RuleTemplateFactory ruleTemplateFactory, DateTimeOffset startDate)
	{
		using var timedLogger = logger.TimeOperation("Loading all time series buffers");

		int countLoaded = 0;

		noTwinCount = 0;

		var ontology = await modelService.GetModelGraphCachedAsync();

		try
		{
			var allMappings = await repositoryTimeSeriesMapping.Get(v => true);
			mappingsByTrendId = allMappings.Where(v => !string.IsNullOrEmpty(v.TrendId)).Select(v => v.TrendId).ToHashSet();
			mappingsByExternalId = allMappings.Where(v => !string.IsNullOrEmpty(v.ExternalId)).Select(v => (v.ExternalId, v.ConnectorId)).ToHashSet();
			var deletedTimeseries = new List<TimeSeries>();

			await foreach (var timeSeries in repositoryTimeSeries.GetAll())
			{
				//Delete timeseries that has a twinid but has the wrong trendid/externalid values for that twin.
				//This can happen when the ADT twin has been updated with a new trendid/externalid but we still have the old one in the buffer.
				//We do this to avoid the wrong timeseries being used during execution. If there is still telemetry for this deleted timeseries,
				//it'll be re-added but with no twinid
				if (!string.IsNullOrEmpty(timeSeries.DtId))
				{
					if (!mappingsByTrendId.Contains(timeSeries.Id) &&
						!mappingsByExternalId.Contains((timeSeries.ExternalId, timeSeries.ConnectorId)))
					{
						deletedTimeseries.Add(timeSeries);
						continue;
					}
				}
				else
				{
					noTwinCount++;

					if (noTwinCount > maxNoTwinCount)
					{
						deletedTimeseries.Add(timeSeries);
						continue;
					}
				}

				AddToBuffers(timeSeries);

				countLoaded++;
			}

			if (deletedTimeseries.Count > 0)
			{
				logger.LogWarning("Found {count} time seires to delete during load", deletedTimeseries.Count);

				if (noTwinCount > maxNoTwinCount)
				{
					logger.LogWarning("Found {count} time seires with no twin. {total} will be deleted", noTwinCount, noTwinCount - maxNoTwinCount);
				}

				var removedLogger = logger.Throttle(TimeSpan.FromSeconds(15));
				int c = 0;
				foreach (var timeseriesGroup in deletedTimeseries.Chunk(100))
				{
					c += timeseriesGroup.Length;

					removedLogger.LogWarning("Removing deprecated timeseries #{c}/{t}", c++, deletedTimeseries.Count);

					await repositoryTimeSeries.BulkDelete(timeseriesGroup.ToList());
				}
			}

			//refresh all twin mappings
			foreach (var mapping in allMappings)
			{
				if ((!string.IsNullOrEmpty(mapping.TrendId) && buffersByTrendId.TryGetValue(mapping.TrendId, out var timeSeries))
				|| (!string.IsNullOrEmpty(mapping.ExternalId) && buffersByTelemetryKey.TryGetValue((mapping.ExternalId, mapping.ConnectorId), out timeSeries)))
				{
					timeSeries.DtId = mapping.DtId;  // always update the DTDID, value is null or empty
					timeSeries.ModelId = string.IsNullOrEmpty(mapping.ModelId) ? timeSeries.ModelId : mapping.ModelId;
					timeSeries.ConnectorId = string.IsNullOrEmpty(timeSeries.ConnectorId) ? mapping.ConnectorId : timeSeries.ConnectorId;
					timeSeries.ExternalId = string.IsNullOrEmpty(timeSeries.ExternalId) ? mapping.ExternalId : timeSeries.ExternalId;
					timeSeries.UnitOfMeasure = string.IsNullOrEmpty(mapping.Unit) ? timeSeries.UnitOfMeasure : mapping.Unit;
					timeSeries.TrendInterval = mapping.TrendInterval.HasValue ? mapping.TrendInterval : timeSeries.TrendInterval;
					timeSeries.TwinLocations = mapping.TwinLocations ?? new List<TwinLocation>(0);

					if (!string.IsNullOrEmpty(timeSeries.DtId))
					{
						//for new dtid's make sure they are the twinlookup buffer
						buffersByTwinId[timeSeries.DtId] = timeSeries;
					}
				}
			}

			foreach ((string twinId, var ruleInstances) in instanceLookup)
			{
				if (buffersByTwinId.TryGetValue(twinId, out var timeSeries))
				{
					foreach (var ruleInstance in ruleInstances)
					{
						var template = ruleTemplateFactory.GetRuleTemplateForRuleInstance(ruleInstance, logger);
						if (template is null) logger.LogWarning("Null template for {instance}", ruleInstance.Id);
						template?.ConfigureTimeSeries(timeSeries, ontology);
					}

					timeSeries.RemovePointsAfter(startDate);

#if DEBUG
					if (!timeSeries.CheckTimeSeriesIsInOrder())
					{
						logger.LogWarning("TimeSeries Id {id} was not in order", timeSeries.Id);
						timeSeries.Sort();
					}
#endif
				}
			}

			if (noTwinCount >= maxNoTwinCount)
			{
				logger.LogWarning("More than {max} buffers without a twin has been found and will be limited", maxNoTwinCount);
			}
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "LoadTimeSeriesBuffers failed");
		}

		logger.LogInformation("Loaded {countLoaded:N0} time series buffers", countLoaded);
	}

	/// <summary>
	/// Flush time series buffers
	/// </summary>
	public async Task FlushToDatabase(DateTime start, DateTime end, SystemSummary summary, ProgressTrackerForRuleExecution progressTracker)
	{
		using var loggerContext = logger.BeginScope("FlushToDatabase");

		var removedLogger = logger.Throttle(TimeSpan.FromSeconds(15));

		List<string> noLongerHere = new();

		int removed = 0;

		using (logger.TimeOperation("Flush time series"))
		{
			int totalCount = 0;
			int totalPoints = 0;
			int bufferCount = BufferList.Count();

			foreach (TimeSeries timeSeries in BufferList)
			{
				removed += ApplyLimits(timeSeries, end);  // now trimmed as we go along so this should not find any to remove

				// Update all the status values for this TimeSeries buffer (Is...)
				timeSeries.SetStatus(end);

				//only keep timeseries that have points
				if (!timeSeries.Points.Any())
				{
					noLongerHere.Add(timeSeries.Id);
					//don't queue for flushing, it must get deleted.
					continue;
				}

				totalCount++;
				totalPoints += timeSeries.Points.Count();
				summary.AddToSummary(timeSeries);

				if (!string.IsNullOrEmpty(timeSeries.DtId))
				{
					// Modifying collection we are enumerating but it's safe
					buffersByTwinId[timeSeries.DtId] = timeSeries;
				}
				else
				{
					throttledLogger.LogWarning("TimeSeries save, still no twin Id for {id} {externalId} {connectorId}", timeSeries.Id, timeSeries.ExternalId, timeSeries.ConnectorId);
				}

				await repositoryTimeSeries.QueueWrite(timeSeries, updateCache: false);

				throttledLogger.LogInformation("TimeSeries updated: {count}", totalCount);

				await progressTracker.ReportFlushingTimeseries(totalCount, bufferCount);
			}

			int c = 0;

			if (noLongerHere.Count > 0)
			{
				logger.LogInformation("Found {count} time seires with no points", noLongerHere.Count);

				foreach (var id in noLongerHere)
				{
					var item = await repositoryTimeSeries.GetOne(id);
					if (item is not null)
					{
						removedLogger.LogWarning("Removing deprecated timeseries #{c}/{t} {id}", c++, noLongerHere.Count, id);
						await repositoryTimeSeries.DeleteOne(item);
					}
				}
			}

			//also delete empty buffers from in memory buffers
			RemoveEmptyBuffers();

			if (c > 0)
			{
				logger.LogWarning("Removed {c} deprecated timeseries", c);
			}

			logger.LogInformation("Flushed time series {count}. Timeseries with Twins {twinCount}. Removed {removedCount:N0} values", totalCount, Count, removed);

			telemetryCollector.TrackTimeSeries(totalCount, totalPoints);

			await repositoryTimeSeries.FlushQueue(updateCache: false);

			await progressTracker.ReportFlushingTimeseries(bufferCount, bufferCount);
		}
	}

	/// <summary>
	/// Counts buffers mapped to a twin
	/// </summary>
	public int Count => this.buffersByTwinId.Count;

	public IEnumerable<TimeSeries> BufferList
	{
		get
		{
			return BufferListInternal().Distinct();
		}
	}

	public TimeSpan MaxDaysToKeep => maxTimeToKeep;

	public int ApplyLimits(TimeSeries timeseries, DateTime now)
	{
		//clear out points older than "default 30" max days. The db flush will delete buffers without points.
		//this can be due to orphaned telemetry, telemetry created by mistake, etc.
		//use the setmaxbuffertime method here. The timeseries might
		//be used as an alias to an actor that requires even larger maxtime to keep
		return timeseries.ApplyLimits(now, TimeSpan.FromDays(7), maxTimeToKeep, canRemoveAllPoints: true);
	}

	/// <summary>
	/// Need to lock around adding one because we have multiple indexes to update
	/// </summary>
	private readonly System.Threading.SemaphoreSlim onlyOne = new(1);

	public async Task<TimeSeries?> GetOrAdd(string trendId, string connectorId, string externalId)
	{
		if (!string.IsNullOrEmpty(trendId) && buffersByTrendId.TryGetValue(trendId, out var timeSeries))
		{
			// Ensure external Id and connectorId are set
			timeSeries.ExternalId = externalId;
			timeSeries.ConnectorId = connectorId;
			return timeSeries;
		}

		if (!string.IsNullOrEmpty(externalId) && buffersByTelemetryKey.TryGetValue((externalId, connectorId), out timeSeries))
		{
			return timeSeries;
		}

		//we limit the amount of "no twin" buffers to save on memory
		//if the ids don't exist in our mappings then return null and don't add
		if (noTwinCount >= maxNoTwinCount)
		{
			if (!string.IsNullOrEmpty(trendId) && !mappingsByTrendId.Contains(trendId))
			{
				return null;
			}

			if (!string.IsNullOrEmpty(externalId) && !mappingsByExternalId.Contains((externalId, connectorId)))
			{
				return null;
			}
		}

		string timeSeriesId = !string.IsNullOrEmpty(trendId) ? trendId : TelemetryKey(externalId, connectorId);

		timeSeries = new TimeSeries(timeSeriesId, null)
		{
			ExternalId = externalId,
			ConnectorId = connectorId
		};  // unknown unit at this point, set below

		// Keep just three values for buffers created to track non-rule points
		// This will get updated from any rules that use it
		timeSeries.SetMaxBufferCount(3);

		try
		{
			await onlyOne.WaitAsync();

			// Try to find the value in the mapping table
			var mappings = await repositoryTimeSeriesMapping
				.GetAll(Array.Empty<SortSpecificationDto>(), Array.Empty<FilterSpecificationDto>(),
					x =>
						(!string.IsNullOrEmpty(trendId) && x.TrendId == trendId) ||
						(x.ConnectorId == connectorId && x.ExternalId == externalId) ||
						// Mapped doesn't have connector IDs in the twin, all their externalIds are globally unique
						(string.IsNullOrEmpty(x.ConnectorId) && x.ExternalId == externalId));

			if (mappings.Items.Any())
			{
				// debug check only one Id
				//System.Diagnostics.Debug.Assert(mappings.Items.Select(x => x.DtId).Distinct().Count() == 1, "TimeSeries: There must be only one twin matching");
				var mapping = mappings.Items.OrderByDescending(x => x.LastUpdate).First();

				if (!string.IsNullOrEmpty(mapping.ConnectorId) && !string.Equals(timeSeries.ConnectorId, mapping.ConnectorId, StringComparison.OrdinalIgnoreCase)) throttledLogger.LogWarning("TimeSeries: Mismatch {timeSeriesId} ConnectorId mismatch: `{live}` != `{twin}`", timeSeriesId, timeSeries.ConnectorId, mapping.ConnectorId);
				if (!string.IsNullOrEmpty(timeSeries.ExternalId) && !string.Equals(timeSeries.ExternalId, (mapping.ExternalId ?? ""), StringComparison.OrdinalIgnoreCase)) throttledLogger.LogWarning("TimeSeries: Mismatch {timeSeriesId} ExternalId mismatch: `{live}` != `{twin}`", timeSeriesId, timeSeries.ExternalId, mapping.ExternalId);

				timeSeries.DtId = mapping.DtId;
				timeSeries.ModelId = mapping.ModelId;
				timeSeries.ConnectorId = mapping.ConnectorId;
				timeSeries.ExternalId = mapping.ExternalId;
				timeSeries.UnitOfMeasure = string.IsNullOrEmpty(mapping.Unit) ? timeSeries.UnitOfMeasure : mapping.Unit;
				timeSeries.TrendInterval = mapping.TrendInterval;
				timeSeries.TwinLocations = mapping.TwinLocations ?? new List<TwinLocation>(0);
				throttledLogger.LogInformation("TimeSeries: Added buffer with twin id from last scan {dtid} {model} {unit}", mapping.DtId, mapping.ModelId, mapping.Unit);
			}
			else
			{
				noTwinCount++;
				throttledLogger.LogInformation("TimeSeries: Added buffer {timeSeriesId} NO TWIN", timeSeriesId);
			}

			AddToBuffers(timeSeries);
			return timeSeries;
		}
		finally
		{
			onlyOne.Release();
		}
	}

	private void RemoveEmptyBuffers()
	{
		foreach (var key in buffersByTwinId.Keys)
		{
			if (!buffersByTwinId[key].Points.Any())
			{
				buffersByTwinId.Remove(key);
			}
		}

		foreach (var key in buffersByTelemetryKey.Keys)
		{
			if (!buffersByTelemetryKey[key].Points.Any())
			{
				buffersByTelemetryKey.Remove(key);
			}
		}

		foreach (var key in buffersByTrendId.Keys)
		{
			if (!buffersByTrendId[key].Points.Any())
			{
				buffersByTrendId.Remove(key);
			}
		}
	}

	private IEnumerable<TimeSeries> BufferListInternal()
	{
		foreach ((_, var value) in this.buffersByTwinId)
		{
			yield return value;
		}

		foreach ((_, var value) in this.buffersByTelemetryKey)
		{
			yield return value;
		}

		foreach ((_, var value) in this.buffersByTrendId)
		{
			yield return value;
		}
	}
}
