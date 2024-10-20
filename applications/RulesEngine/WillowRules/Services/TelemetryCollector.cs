using Microsoft.Extensions.Caching.Memory;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Willow.Rules.Services;

/// <summary>
/// Telemetry collector - publishes to application insights
/// </summary>
public interface ITelemetryCollector
{
	/// <summary>
	/// Tracks command totals
	/// </summary>
	void TrackFaultyInsights(int count);

	/// <summary>
	/// Tracks command totals
	/// </summary>
	void TrackCommands(int totalUpserted, int totalSynced);

	/// <summary>
	/// Tracks command insight totals
	/// </summary>
	void TrackCommandInsights(int totalUpserted, int totalDeleted);

	/// <summary>
	/// Tracks insights queued
	/// </summary>
	void TrackInsightsQueued(int totalQueued);

	/// <summary>
	/// Tracks insights avg sync speed
	/// </summary>
	void TrackInsightsSyncSpeed(double value);

	/// <summary>
	/// Tracks execution speed using metric key RulesEngine-ExecutionSpeed
	/// </summary>
	void TrackExecutionSpeed(double value);

	/// <summary>
	/// Tracks execution speed using metric key RulesEngine-ProportionSpentExecuting
	/// </summary>
	void TrackProportionSpentExecuting(double value);

	/// <summary>
	/// Tracks execution speed using metric key RulesEngine-ProportionSpentInActor
	/// </summary>
	void TrackProportionSpentInActor(double value);

	/// <summary>
	/// Tracks ADX lines per millisecond using metric key RulesEngine-ADXLineSpeed
	/// </summary>
	void TrackLineSpeed(double value);

	/// <summary>
	/// Tracks Time Series stats
	/// </summary>
	void TrackTimeSeries(int totalBuffers, int totalValues);

	/// <summary>
	/// Tracks Actor stats
	/// </summary>
	void TrackActors(int totalActors, int totalPoints, int totalOutputs, int totalOutputVariables);

	/// <summary>
	/// Tracks Total items in memory cache
	/// </summary>
	void TrackMemoryCacheCount();

	/// <summary>
	/// Tracks Insights generated
	/// </summary>
	void TrackInsightsGeneratedCount(double value);

	/// <summary>
	/// Tracks the amount of time (in minutes) Rules Engine spends executing prior to the wait interval
	/// </summary>
	/// <param name="value"></param>
	void TrackExecutionTotalWaitTime(double value);

	/// <summary>
	/// Tracks Calculated Points generated
	/// </summary>
	void TrackCalculatedPoints(int upserted, int deleted);
}

/// <summary>
/// Telemetry collector
/// </summary>
public class TelemetryCollector : ITelemetryCollector
{
	private readonly Histogram<double> executionSpeed;
	private readonly Histogram<double> proportionSpentExecuting;
	private readonly Histogram<double> proportionSpentInActor;
	private readonly Histogram<double> lineSpeed;
	private readonly Histogram<int> timeSeriesTotalPoints;
	private readonly Histogram<int> timeSeriesTotalValues;
	private readonly Histogram<int> actorTotal;
	private readonly Histogram<int> actorTotalCalculatedValues;
	private readonly Histogram<int> actorTotalOutputs;
	private readonly Histogram<int> actorTotalOutputVariables;
	private readonly Histogram<int> memoryCacheCount;
	private readonly Histogram<double> insightsGenerated;
	private readonly Histogram<double> insightsQueued;
	private readonly Histogram<double> insightsSyncSpeed;
	private readonly Histogram<double> executionTotalWaitTime;
	private readonly Histogram<double> commandInsightTotalUpserted;
	private readonly Histogram<double> commandInsightTotalDeleted;
	private readonly Histogram<double> calculatedPointsUpserted;
	private readonly Histogram<double> calculatedPointsDeleted;
	private readonly Histogram<double> commandsTotalUpserted;
	private readonly Histogram<double> commandsTotalSynced;
	private readonly Histogram<double> faultyInsights;
	private readonly IMemoryCache memoryCache;
	private readonly TagList defaultTags;
	private readonly Meter meter;

	/// <summary>
	/// Creates a new <see cref="TelemetryCollector"/> tracker
	/// </summary>
	public TelemetryCollector(string applicationName, string applicationVersion, TagList defaultTags, IMemoryCache memoryCache)
	{
		this.meter = new Meter(applicationName, applicationVersion);
		this.defaultTags = defaultTags;
		this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		this.executionSpeed = meter.CreateHistogram<double>("RulesEngine-ExecutionSpeed");
		this.proportionSpentExecuting = meter.CreateHistogram<double>("RulesEngine-ProportionSpentExecuting");
		this.proportionSpentInActor = meter.CreateHistogram<double>("RulesEngine-ProportionSpentInActor");
		this.lineSpeed = meter.CreateHistogram<double>("RulesEngine-ADXLineSpeed");
		this.timeSeriesTotalPoints = meter.CreateHistogram<int>("RulesEngine-TimeSeriesTotalPoints");
		this.timeSeriesTotalValues = meter.CreateHistogram<int>("RulesEngine-TimeSeriesTotalValues");
		this.actorTotal = meter.CreateHistogram<int>("RulesEngine-ActorTotal");
		this.actorTotalCalculatedValues = meter.CreateHistogram<int>("RulesEngine-ActorTotalCalculatedValues");
		this.actorTotalOutputs = meter.CreateHistogram<int>("RulesEngine-ActorTotalOutputs");
		this.actorTotalOutputVariables = meter.CreateHistogram<int>("RulesEngine-ActorTotalOutputVariables");
		this.memoryCacheCount = meter.CreateHistogram<int>("RulesEngine-MemoryCacheCount");
		this.insightsGenerated = meter.CreateHistogram<double>("RulesEngine-InsightsGenerated");
		this.insightsQueued = meter.CreateHistogram<double>("RulesEngine-InsightsQueued");
		this.insightsSyncSpeed = meter.CreateHistogram<double>("RulesEngine-InsightsSyncSpeed");
		this.executionTotalWaitTime = meter.CreateHistogram<double>("RulesEngine-ExecutionTotalWaitTime");
		this.commandInsightTotalUpserted = meter.CreateHistogram<double>("RulesEngine-CommandInsightTotalUpserted");
		this.commandInsightTotalDeleted = meter.CreateHistogram<double>("RulesEngine-CommandInsightTotalDeleted");
		this.calculatedPointsUpserted = meter.CreateHistogram<double>("RulesEngine-CalculatedPointsUpserted");
		this.calculatedPointsDeleted = meter.CreateHistogram<double>("RulesEngine-CalculatedPointsDeleted");
		this.commandsTotalUpserted = meter.CreateHistogram<double>("RulesEngine-CommandsTotalUpserted");
		this.commandsTotalSynced = meter.CreateHistogram<double>("RulesEngine-CommandsTotalSynced");
		this.faultyInsights = meter.CreateHistogram<double>("RulesEngine-FaultyInsights");
	}

	public void TrackExecutionSpeed(double value)
	{
		executionSpeed.Record(value, defaultTags);
	}

	public void TrackProportionSpentExecuting(double value)
	{
		proportionSpentExecuting.Record(value, defaultTags);
	}

	public void TrackProportionSpentInActor(double value)
	{
		proportionSpentInActor.Record(value, defaultTags);
	}

	public void TrackLineSpeed(double value)
	{
		lineSpeed.Record(value, defaultTags);
	}

	public void TrackTimeSeries(int totalBuffers, int totalValues)
	{
		timeSeriesTotalPoints.Record(totalBuffers, defaultTags);
		timeSeriesTotalValues.Record(totalValues, defaultTags);
	}

	public void TrackActors(int totalActors, int totalPoints, int totalOutputs, int totalOutputVariables)
	{
		actorTotal.Record(totalActors, defaultTags);
		actorTotalCalculatedValues.Record(totalPoints, defaultTags);
		actorTotalOutputs.Record(totalOutputs, defaultTags);
		actorTotalOutputVariables.Record(totalOutputVariables, defaultTags);
	}

	public void TrackMemoryCacheCount()
	{
		if (memoryCache is MemoryCache cache)
		{
			memoryCacheCount.Record(cache.Count, defaultTags);
		}
	}

	public void TrackInsightsGeneratedCount(double value)
	{
		insightsGenerated.Record(value, defaultTags);
	}

	public void TrackExecutionTotalWaitTime(double value)
	{
		executionTotalWaitTime.Record(value, defaultTags);
	}

	public void TrackCalculatedPoints(int totalUpserted, int totalDeleted)
	{
		calculatedPointsUpserted.Record(totalUpserted, defaultTags);
		calculatedPointsDeleted.Record(totalDeleted, defaultTags);
	}

	public void TrackCommandInsights(int totalUpserted, int totalDeleted)
	{
		commandInsightTotalUpserted.Record(totalUpserted, defaultTags);
		commandInsightTotalDeleted.Record(totalDeleted, defaultTags);
	}

	public void TrackCommands(int totalUpserted, int totalSynced)
	{
		commandsTotalUpserted.Record(totalUpserted);
		commandsTotalSynced.Record(totalSynced);
	}

	public void TrackInsightsQueued(int totalQueued)
	{
		insightsQueued.Record(totalQueued, defaultTags);
	}

	public void TrackInsightsSyncSpeed(double value)
	{
		insightsSyncSpeed.Record(value, defaultTags);
	}

	public void TrackFaultyInsights(int count)
	{
		faultyInsights.Record(count, defaultTags);
	}
}
