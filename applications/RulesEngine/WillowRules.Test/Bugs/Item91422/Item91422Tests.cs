using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using Willow.Rules.Model;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item91422Tests
{
	[TestMethod]
	public void TimeseriesLatencyTest()
	{
		var rawDataPoints = new List<RawData>
		{
			new RawData() { SourceTimestamp = DateTime.Parse("2023-11-13T11:16:42.288Z"), EnqueuedTimestamp = DateTime.Parse("2023-11-13T11:25:40.3645151Z") },
			new RawData() { SourceTimestamp = DateTime.Parse("2023-11-13T11:31:42.437Z"), EnqueuedTimestamp = DateTime.Parse("2023-11-13T11:40:40.3299367Z") },
			new RawData() { SourceTimestamp = DateTime.Parse("2023-11-13T11:46:42.579Z"), EnqueuedTimestamp = DateTime.Parse("2023-11-13T11:55:41.4396106Z") },
			new RawData() { SourceTimestamp = DateTime.Parse("2023-11-13T12:01:42.747Z"), EnqueuedTimestamp = DateTime.Parse("2023-11-13T12:10:43.4667983Z") },
			new RawData() { SourceTimestamp = DateTime.Parse("2023-11-13T12:16:42.873Z"), EnqueuedTimestamp = DateTime.Parse("2023-11-13T12:25:42.7363737Z") }
		};

		var timeseries = new TimeSeries();

		foreach (var rawData in rawDataPoints)
		{
			var latency = rawData.EnqueuedTimestamp.Subtract(rawData.SourceTimestamp);
			timeseries.SetLatencyEstimate(latency);
		}

		timeseries.Latency.TotalMinutes.Should().BeApproximately(8, 1);
	}

	[TestMethod]
	public void LargeLatencyTest()
	{
		var data = BugHelper.CreateData("Item91422", "Timeseries_Max.csv");

		var timeseries = new TimeSeries();

		foreach (var item in data.data)
		{
			timeseries.AddPoint(new TimedValue(item.SourceTimestamp, item.Value), applyCompression: true);
			timeseries.SetLatencyEstimate(item.EnqueuedTimestamp.Subtract(item.SourceTimestamp));
		}

		timeseries.Latency.TotalHours.Should().BeApproximately(4, 2);
	}
}
