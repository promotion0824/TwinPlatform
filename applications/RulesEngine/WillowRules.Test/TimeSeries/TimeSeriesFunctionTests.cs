using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;

namespace Willow.Rules.Test;

[TestClass]
public class TimeSeriesFunctionTests
{
	[TestMethod]
	public void StandardDeviation()
	{
		var timestamp1 = new DateTimeOffset(DateTime.Today);
		var timestamp2 = timestamp1.AddMinutes(10);
		var timestamp3 = timestamp1.AddMinutes(20);
		var timestamp4 = timestamp1.AddMinutes(30);

		var timeseries = new TimeSeries("test", "");

		timeseries.AddPoint(new TimedValue(timestamp1, 6), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp2, 2), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp3, 3), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp4, 1), applyCompression: true);

		double stnd = timeseries.Points.StandardDeviation();
		Math.Round(stnd, 2).Should().Be(1.87);
	}

	[TestMethod]
	public void Average()
	{
		var timestamp1 = new DateTimeOffset(DateTime.Today); // 1
		var timestamp2 = timestamp1.AddMinutes(10);          // 2
		var timestamp25 = timestamp1.AddMinutes(15);         // interpolate to here
		var timestamp3 = timestamp1.AddMinutes(20);          // 3
		var timestamp4 = timestamp1.AddMinutes(30);          // 4
		var now = timestamp1.AddMinutes(90);

		var timeseries = new TimeSeries("test", "");

		timeseries.AddPoint(new TimedValue(timestamp1, 1), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp2, 2), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp3, 3), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp4, 4), applyCompression: true);

		double average = timeseries.Points.Average(timestamp2, now);

		average.Should().Be(3.0);

		average = timeseries.Points.Average(timestamp25, now);

		average.Should().Be(3.25);
	}

	[TestMethod]
	public void Delta()
	{
		var timestamp1 = new DateTimeOffset(DateTime.Today); // 1
		var timestamp2 = timestamp1.AddMinutes(10);          // 2
		var timestamp25 = timestamp1.AddMinutes(15);         // interpolate to here
		var timestamp3 = timestamp1.AddMinutes(20);          // 3
		var timestamp4 = timestamp1.AddMinutes(30);          // 4
		var now = timestamp1.AddMinutes(90);

		var timeseries = new TimeSeries("test", "");

		timeseries.AddPoint(new TimedValue(timestamp1, 1), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp2, 2), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp3, 3), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp4, 4), applyCompression: true);

		double delta = timeseries.Points.Delta(timestamp2, now);

		delta.Should().Be(2.0);

		delta = timeseries.Points.Delta(timestamp25, now);

		delta.Should().Be(1.5);
	}

	[TestMethod]
	public void Slope()
	{
		var timestamp1 = new DateTimeOffset(DateTime.Today.AddDays(-7)); // 1
		var timestamp2 = timestamp1.AddDays(1);          // 2
		var timestamp3 = timestamp1.AddDays(2);          // 3
		var timestamp4 = timestamp1.AddDays(3);          // 4

		var timeseries = new TimeSeries("test", "");

		timeseries.AddPoint(new TimedValue(timestamp1, 1), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp2, 2), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp3, 3), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp4, 4), applyCompression: true);

		double slope = timeseries.Points.Slope(timestamp1, timestamp4);

		slope.Should().Be(1.0);
	}

	[TestMethod]
	public void Forecast()
	{
		var timestamp1 = new DateTimeOffset(DateTime.Today.AddDays(-7)); // 1
		var timestamp2 = timestamp1.AddDays(1);          // 2
		var timestamp3 = timestamp1.AddDays(2);          // 3
		var timestamp4 = timestamp1.AddDays(3);          // 4

		var timeseries = new TimeSeries("test", "");

		timeseries.AddPoint(new TimedValue(timestamp1, 1), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp2, 2), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp3, 3), applyCompression: true);
		timeseries.AddPoint(new TimedValue(timestamp4, 4), applyCompression: true);

		double prediction = timeseries.Points.Forecast(TimeSpan.FromDays(5));

		prediction.Should().BeApproximately(9.0, 0.000001);
	}
}
