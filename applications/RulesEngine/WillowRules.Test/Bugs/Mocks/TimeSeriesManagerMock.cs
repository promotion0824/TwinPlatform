using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Services;

namespace WillowRules.Test.Bugs.Mocks;

/// <summary>
/// Wraps time series manager and intercepts the load method
/// </summary>
public class TimeSeriesManagerMock : ITimeSeriesManager
{
	private readonly TimeSeriesManager timeSeriesManager;

	public int Count => timeSeriesManager.Count;

	IEnumerable<TimeSeries> ITimeSeriesManager.BufferList => timeSeriesManager.BufferList;

	public TimeSpan MaxDaysToKeep => timeSeriesManager.MaxDaysToKeep;

	public TimeSeriesManagerMock(TimeSeriesManager timeSeriesManager)
	{
		this.timeSeriesManager = timeSeriesManager ?? throw new ArgumentNullException(nameof(timeSeriesManager));
	}

	public virtual Task FlushToDatabase(DateTime start, DateTime end, SystemSummary summary, ProgressTrackerForRuleExecution progressTracker)
	{
		var endDate = timeSeriesManager.BufferList.Max(v => v.LastSeen);
		return timeSeriesManager.FlushToDatabase(start, endDate.DateTime, summary, progressTracker);
	}

	public virtual async Task LoadTimeSeriesBuffers(Dictionary<string, List<RuleInstance>> instanceLookup, RuleTemplateFactory ruleTemplateFactory, DateTimeOffset startDate)
	{
		await timeSeriesManager.LoadTimeSeriesBuffers(instanceLookup, ruleTemplateFactory, startDate);
	}

	public virtual Task<bool> UpdateDtId(BasicDigitalTwinPoco twin)
	{
		throw new NotImplementedException();
	}

	public virtual int ApplyLimits(TimeSeries timeseries, DateTime now)
	{
		return timeSeriesManager.ApplyLimits(timeseries, now);
	}

	public virtual Task SendCapabilityStatusUpdate(CancellationToken cancellationToken = default)
	{
		return Task.CompletedTask;
	}

	public virtual async Task<TimeSeries?> GetOrAdd(string pointId, string connectorId, string externalId)
	{
		var buffer = await timeSeriesManager.GetOrAdd(pointId, connectorId, externalId);

		return buffer;
	}

	public virtual bool TryGetByTwinId(string twinId, out TimeSeries? timeSeries)
	{
		return timeSeriesManager.TryGetByTwinId(twinId, out timeSeries);
	}

	public void AddToBuffers(TimeSeries timeSeries)
	{
		timeSeriesManager.AddToBuffers(timeSeries);
	}
}
