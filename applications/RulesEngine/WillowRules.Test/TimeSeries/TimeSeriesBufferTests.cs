using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using WillowRules.Filters;

namespace Willow.Rules.Test;

[TestClass]
public class TimeSeriesBufferTests
{
	[TestMethod]
	public void TimedValueCalcProperties()
	{
		var value = new TimedValue(DateTimeOffset.Now, 1);
		value.BoolValue.Should().Be(1);
		value.NumericValue.Should().Be(1);

		value = new TimedValue(DateTimeOffset.Now, 0);
		value.BoolValue.Should().Be(0);
		value.NumericValue.Should().Be(0);

		value = new TimedValue(DateTimeOffset.Now, true);
		value.BoolValue.Should().Be(1);
		value.NumericValue.Should().Be(1);

		value = new TimedValue(DateTimeOffset.Now, false);
		value.BoolValue.Should().Be(0);
		value.NumericValue.Should().Be(0);
	}

	[TestMethod]
	public void MustGetRange()
	{
		var timestamp1 = DateTimeOffset.Now.AddMinutes(-14);
		var timestamp2 = DateTimeOffset.Now.AddMinutes(-12);
		var timestamp3 = DateTimeOffset.Now.AddMinutes(-10);
		var timestamp4 = DateTimeOffset.Now.AddMinutes(-5);

		var timeseries = new TimeSeries("test", "");

		timeseries.AddPoint(new TimedValue(timestamp1, 1), applyCompression: false);
		timeseries.AddPoint(new TimedValue(timestamp2, 2), applyCompression: false);
		timeseries.AddPoint(new TimedValue(timestamp3, 3), applyCompression: false);
		timeseries.AddPoint(new TimedValue(timestamp4, 4), applyCompression: false);

		var data = timeseries.GetRange(timestamp1, timestamp4).ToList();

		data.Count().Should().Be(4);
		data[0].NumericValue.Should().Be(1);
		data[1].NumericValue.Should().Be(2);
		data[2].NumericValue.Should().Be(3);
		data[3].NumericValue.Should().Be(4);

		data = timeseries.GetRange(timestamp1, timestamp2).ToList();

		data.Count().Should().Be(2);
		data[0].NumericValue.Should().Be(1);
		data[1].NumericValue.Should().Be(2);

		data = timeseries.GetRange(timestamp1, timestamp3).ToList();

		data.Count().Should().Be(3);
		data[0].NumericValue.Should().Be(1);
		data[1].NumericValue.Should().Be(2);
		data[2].NumericValue.Should().Be(3);

		data = timeseries.GetRange(timestamp2, timestamp3).ToList();

		data.Count().Should().Be(2);
		data[0].NumericValue.Should().Be(2);
		data[1].NumericValue.Should().Be(3);

		data = timeseries.GetRange(timestamp1.AddDays(-1), timestamp4).ToList();

		data.Count().Should().Be(4);
		data[0].NumericValue.Should().Be(1);
		data[1].NumericValue.Should().Be(2);
		data[2].NumericValue.Should().Be(3);
		data[3].NumericValue.Should().Be(4);

		data = timeseries.GetRange(timestamp2, timestamp4.AddDays(1)).ToList();

		data.Count().Should().Be(3);
		data[0].NumericValue.Should().Be(2);
		data[1].NumericValue.Should().Be(3);
		data[2].NumericValue.Should().Be(4);

		data = timeseries.GetRange(timestamp3, timestamp3).ToList();

		data.Count().Should().Be(1);
		data[0].NumericValue.Should().Be(3);

		data = timeseries.GetRange(timestamp1.AddDays(-2), timestamp1.AddDays(-1)).ToList();

		data.Count().Should().Be(0);

		data = timeseries.GetRange(timestamp1, timestamp1).ToList();

		data.Count().Should().Be(1);
		data[0].NumericValue.Should().Be(1);

		data = timeseries.GetRange(timestamp1.AddDays(2), timestamp1.AddDays(3)).ToList();

		data.Count().Should().Be(0);

	}

	[TestMethod]
	public void TimeSeriesBufferAggregations()
	{
		var timestamp1 = DateTimeOffset.Now.AddMinutes(-14);
		var timestamp2 = DateTimeOffset.Now.AddMinutes(-12);
		var timestamp3 = DateTimeOffset.Now.AddMinutes(-10);
		var timestamp4 = DateTimeOffset.Now.AddMinutes(-5);

		var aggregation = new TimeSeries("test", "");

		aggregation.EnableEstimatedPeriod();

		aggregation.AddPoint(new TimedValue(timestamp1, 1), applyCompression: false);
		aggregation.AddPoint(new TimedValue(timestamp2, 2), applyCompression: false);
		aggregation.AddPoint(new TimedValue(timestamp3, 3), applyCompression: false);
		aggregation.AddPoint(new TimedValue(timestamp4, 4), applyCompression: false);

		aggregation.AverageValue.Should().Be(2.5);
		aggregation.EarliestSeen.Should().Be(timestamp1);
		aggregation.Points.Should().HaveCount(4);
		aggregation.LastSeen.Should().Be(timestamp4);
		aggregation.LastValueDouble.Should().Be(4);
		aggregation.TotalValuesProcessed.Should().Be(4);

		var timestamp5 = DateTimeOffset.Now.AddMinutes(-2);
		var timestamp6 = DateTimeOffset.Now.AddMinutes(-1);

		aggregation.AddPoint(new TimedValue(timestamp5, 5), applyCompression: false);
		aggregation.AddPoint(new TimedValue(timestamp6, 6), applyCompression: false);

		aggregation.AverageValue.Should().Be(3.5);
		aggregation.EarliestSeen.Should().Be(timestamp1);
		aggregation.Points.Should().HaveCount(6);
		aggregation.LastSeen.Should().Be(timestamp6);
		aggregation.LastValueDouble.Should().Be(6);
		aggregation.TotalValuesProcessed.Should().Be(6);

		aggregation.ApplyLimits(4, null);

		aggregation.AverageInBuffer.Should().Be(4.5);
		aggregation.AverageValue.Should().Be(3.5);
		aggregation.EarliestSeen.Should().Be(timestamp1);
		aggregation.Points.Should().HaveCount(4);
		aggregation.LastSeen.Should().Be(timestamp6);
		aggregation.LastValueDouble.Should().Be(6);
		aggregation.TotalValuesProcessed.Should().Be(6);

		aggregation.EstimatedPeriod.TotalSeconds.Should().BeInRange(130, 135);
	}

	[TestMethod]
	public void QualityCheckShouldNotMakeBufferGoOffline()
	{
		var timestamp = DateTimeOffset.Now.AddHours(-24);

		var aggregation = new TimeSeries("test", "degC")
		{
			ModelId = "TemperatureSensor;1",
			TrendInterval = 900
		};

		aggregation.EnableValidation();

		for (var i = 0; i <= 1000; i++)
		{
			aggregation.AddPoint(new TimedValue(timestamp.AddMinutes(i), 10000), applyCompression: false);
			aggregation.SetStatus(timestamp.AddMinutes(i));
		}

		aggregation.IsValueOutOfRange.Should().BeTrue();
		aggregation.IsOffline.Should().BeFalse();
	}

	[TestMethod]
	public void TimeSeriesOutOfRangeFilterBackToInRangeWithConstant()
	{
		var timestamp = DateTimeOffset.Now.AddHours(-24);

		var aggregation = new TimeSeries("test", "degC")
		{
			ModelId = "TemperatureSensor;1"
		};

		aggregation.EnableValidation();

		var temp = 25;
		for (var i = 0; i <= 1000; i++)
		{
			aggregation.AddPoint(new TimedValue(timestamp.AddMinutes(i), i), applyCompression: false);
			temp++;
		}

		aggregation.SetStatus(timestamp.AddMinutes(1000));

		aggregation.IsValueOutOfRange.Should().BeTrue();

		var inRangeSameTemp = 80;
		for (var i = 0; i <= 230; i++)
		{
			aggregation.AddPoint(new TimedValue(timestamp.AddMinutes(i), inRangeSameTemp), applyCompression: false);
		}

		aggregation.SetStatus(timestamp.AddMinutes(1230));

		aggregation.IsValueOutOfRange.Should().BeFalse();
	}

	[TestMethod]
	public void TimeSeriesOutOfRangeFilterBackToInRangeWithVariance()
	{
		var timestamp = DateTimeOffset.Now.AddHours(-24);

		var aggregation = new TimeSeries("test", "degC")
		{
			ModelId = "TemperatureSensor;1"
		};

		aggregation.EnableValidation();

		var temp = 25;
		for (var i = 0; i <= 1000; i++)
		{
			aggregation.AddPoint(new TimedValue(timestamp.AddMinutes(i), temp), applyCompression: false);
			temp++;
		}

		aggregation.SetStatus(timestamp.AddMinutes(1000));

		aggregation.IsValueOutOfRange.Should().BeTrue();

		var inRangeRandomTemp = new Random();
		for (var i = 0; i <= 230; i++)
		{
			aggregation.AddPoint(new TimedValue(timestamp.AddMinutes(i), inRangeRandomTemp.Next(-73, 93)), applyCompression: false);
		}

		aggregation.SetStatus(timestamp.AddMinutes(1230));

		aggregation.IsValueOutOfRange.Should().BeFalse();
	}

	[TestMethod]
	public void BinaryKalmanFilterTest()
	{
		var measurements = new bool[] { false, true, true, true, false, false, false, true, false, false };

		double initialProbability = 0.2;
		double initialCovariance = Convert.ToDouble(measurements[0]);

		BinaryKalmanFilter binaryFilter = new(initialProbability, initialCovariance);

		foreach (bool measurement in measurements)
		{
			BinaryKalmanFilterFunctions.Update(binaryFilter, measurement);
		}

		binaryFilter.State.Should().BeGreaterThan(binaryFilter.BenchMark);
	}
}
