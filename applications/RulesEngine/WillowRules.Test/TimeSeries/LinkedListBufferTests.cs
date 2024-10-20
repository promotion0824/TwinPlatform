using System;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;

namespace Willow.Rules.Test;

[TestClass]
public class LinkedListBufferTests
{
	private TimeSeries buffer = new();

	[TestInitialize]
	public void Setup()
	{
		buffer = new TimeSeries("test", "");
	}

	[TestMethod]
	public void ShouldKeepPointsWith12HourRange()
	{
		buffer.SetCompression(0.05);

		for (var i = 48; i > 0; i--)
		{
			var timestamp = DateTimeOffset.Now.AddHours(-i);
			buffer.AddPoint(new TimedValue(timestamp, 1), true);
		}
		var values = buffer.Points.ToList();

		values.Should().HaveCount(6);
	}

	[TestMethod]
	public void ShouldLeavePointsWithLargeGap()
	{
		var timestamp1 = DateTimeOffset.Now.AddHours(-15);
		var timestamp2 = DateTimeOffset.Now.AddHours(-1);
		var timestamp3 = DateTimeOffset.Now.AddHours(-1.9);

		buffer.SetCompression(0.05);

		buffer.AddPoint(new TimedValue(timestamp1, 1), true);
		buffer.AddPoint(new TimedValue(timestamp2, 1), true);
		buffer.AddPoint(new TimedValue(timestamp3, 1), true);

		var timestampBetweenFirstTwoPoints = timestamp1.AddHours(1);

		buffer.ApplyLimits(null, timestampBetweenFirstTwoPoints);

		var values = buffer.Points.ToList();

		values.Should().HaveCount(2);

		values[0].Timestamp.Should().Be(timestamp1);
		values[1].Timestamp.Should().Be(timestamp3);
	}

	[TestMethod]
	public void ShouldLeavePointsWithTwo12HourGaps()
	{
		var timestamp1 = DateTimeOffset.Now.AddHours(-24);
		var timestamp2 = DateTimeOffset.Now.AddHours(-20);
		var timestamp3 = DateTimeOffset.Now.AddHours(-2);
		var timestamp4 = DateTimeOffset.Now.AddHours(-1);

		buffer.SetCompression(0.05);

		buffer.AddPoint(new TimedValue(timestamp1, 1), true);
		buffer.AddPoint(new TimedValue(timestamp2, 1), true);
		buffer.AddPoint(new TimedValue(timestamp3, 1), true);
		buffer.AddPoint(new TimedValue(timestamp4, 1), true);

		var timestampBetweenFirstTwoPoints = timestamp2.AddHours(1);

		buffer.ApplyLimits(null, timestampBetweenFirstTwoPoints);

		var values = buffer.Points.ToList();

		values.Should().HaveCount(3);

		values[0].Timestamp.Should().Be(timestamp2);
		values[1].Timestamp.Should().Be(timestamp3);
		values[2].Timestamp.Should().Be(timestamp4);
	}

	[TestMethod]
	public void ShouldDropPointsWithLargeGap()
	{
		var timestamp1 = DateTimeOffset.Now.AddHours(-15);
		var timestamp2 = DateTimeOffset.Now.AddHours(-1);
		var timestamp3 = DateTimeOffset.Now.AddHours(-1.9);
		var timestamp4 = DateTimeOffset.Now.AddHours(-1.8);
		var timestamp5 = DateTimeOffset.Now.AddHours(-1.7);
		var timestamp6 = DateTimeOffset.Now.AddHours(-1.6);
		var timestamp7 = DateTimeOffset.Now.AddHours(-1.5);

		buffer.SetCompression(0.05);

		buffer.AddPoint(new TimedValue(timestamp1, 1), true);
		buffer.AddPoint(new TimedValue(timestamp2, 1), true);
		buffer.AddPoint(new TimedValue(timestamp3, 1), true);
		buffer.AddPoint(new TimedValue(timestamp4, 1), true);
		buffer.AddPoint(new TimedValue(timestamp5, 1), true);
		buffer.AddPoint(new TimedValue(timestamp6, 1), true);
		buffer.AddPoint(new TimedValue(timestamp7, 1), true);

		buffer.ApplyLimits(null, timestamp2);

		var values = buffer.Points.ToList();

		values.Should().HaveCount(2);

		values[0].Timestamp.Should().Be(timestamp3);
		values[1].Timestamp.Should().Be(timestamp7);
	}

	[TestMethod]
	public void MustCompress()
	{
		var timestamp1 = DateTimeOffset.Now.AddMinutes(-4);
		var timestamp2 = DateTimeOffset.Now.AddMinutes(-3);
		var timestamp3 = DateTimeOffset.Now.AddMinutes(-2);
		var timestamp4 = DateTimeOffset.Now.AddMinutes(-1);

		buffer.SetCompression(0.1);

		buffer.AddPoint(new TimedValue(timestamp1, 1), true);
		buffer.AddPoint(new TimedValue(timestamp2, 1), true);
		buffer.AddPoint(new TimedValue(timestamp3, 1), true);
		buffer.AddPoint(new TimedValue(timestamp4, 1), true);

		var values = buffer.Points.ToList();

		values.Should().HaveCount(2);
	}

	[TestMethod]
	public void MustCompress1()
	{
		var ts1 = new TimeSeries();
		ts1.SetCompression(0.05);
		ts1.AddPoint(new TimedValue(DateTimeOffset.Parse("2023/03/23 12:00:00"), 20), applyCompression: true);
		ts1.AddPoint(new TimedValue(DateTimeOffset.Parse("2023/03/23 12:15:22"), 30), applyCompression: true);
		ts1.AddPoint(new TimedValue(DateTimeOffset.Parse("2023/03/23 12:30:00"), 40), applyCompression: true);
		ts1.AddPoint(new TimedValue(DateTimeOffset.Parse("2023/03/23 12:45:00"), 50), applyCompression: true);
		ts1.AddPoint(new TimedValue(DateTimeOffset.Parse("2023/03/23 13:00:00"), 60), applyCompression: true);

		ts1.Count.Should().Be(2);

		var ts2 = new TimeSeries();
		ts2.SetCompression(0.05);

		ts2.AddPoint(new TimedValue(DateTimeOffset.Parse("2023/03/23 12:00:00"), 20), applyCompression: true);
		ts2.AddPoint(new TimedValue(DateTimeOffset.Parse("2023/03/23 12:15:23"), 30), applyCompression: true);//1 SECOND DIFFERENCE
		ts2.AddPoint(new TimedValue(DateTimeOffset.Parse("2023/03/23 12:30:00"), 40), applyCompression: true);
		ts2.AddPoint(new TimedValue(DateTimeOffset.Parse("2023/03/23 12:45:00"), 50), applyCompression: true);
		ts2.AddPoint(new TimedValue(DateTimeOffset.Parse("2023/03/23 13:00:00"), 60), applyCompression: true);

		ts2.Count.Should().Be(3);  // The 1s off point is just through it out of alignment but everything else compresses
	}

	[TestMethod]
	public void TimeSeriesBufferWithMaxTime()
	{
		var timestamp1 = DateTimeOffset.Now.AddMinutes(-50);
		var timestamp2 = DateTimeOffset.Now.AddMinutes(-49);
		var timestamp3 = DateTimeOffset.Now.AddMinutes(-12);
		var timestamp4 = DateTimeOffset.Now.AddMinutes(-10);
		var timestamp5 = DateTimeOffset.Now.AddMinutes(-5);

		buffer.SetMaxBufferTime(TimeSpan.FromMinutes(13));

		buffer.AddPoint(new TimedValue(timestamp1, 1), false);
		buffer.AddPoint(new TimedValue(timestamp2, 2), false);
		buffer.AddPoint(new TimedValue(timestamp3, 3), false);
		buffer.AddPoint(new TimedValue(timestamp4, 4), false);
		buffer.AddPoint(new TimedValue(timestamp5, 5), false);

		buffer.ApplyLimits(null, DateTimeOffset.Now.AddMinutes(-13));

		var values = buffer.Points.ToList();

		values.Should().HaveCount(4);

		values[0].Timestamp.Should().Be(timestamp2);
		values[1].Timestamp.Should().Be(timestamp3);
		values[2].Timestamp.Should().Be(timestamp4);
		values[3].Timestamp.Should().Be(timestamp5);
	}

	[TestMethod]
	public void TimeSeriesBufferWithMaxCount()
	{
		var timestamp1 = DateTimeOffset.Now.AddMinutes(-14);
		var timestamp2 = DateTimeOffset.Now.AddMinutes(-12);
		var timestamp3 = DateTimeOffset.Now.AddMinutes(-10);
		var timestamp4 = DateTimeOffset.Now.AddMinutes(-5);

		var maxCount = 2;

		buffer.SetMaxBufferCount(maxCount);

		buffer.AddPoint(new TimedValue(timestamp1, 1), false);
		buffer.AddPoint(new TimedValue(timestamp2, 2), false);
		buffer.AddPoint(new TimedValue(timestamp3, 3), false);
		buffer.AddPoint(new TimedValue(timestamp4, 4), false);

		buffer.ApplyLimits(buffer.MaxCountToKeep, null);

		var values = buffer.Points.ToList();

		values.Should().HaveCount(2);

		values[0].Timestamp.Should().Be(timestamp3);
		values[1].Timestamp.Should().Be(timestamp4);
	}

	[TestMethod]
	public void ShouldNotAddDuplicate()
	{
		var timestamp = DateTimeOffset.Now.AddMinutes(-5);

		buffer.AddPoint(new TimedValue(timestamp, 1), false);
		buffer.AddPoint(new TimedValue(timestamp, 1), false);

		var values = buffer.Points.ToList();

		values.Should().HaveCount(1);
	}

	[TestMethod]
	public void TimeSeriesBufferMaxCount()
	{
		var timestamp1 = DateTimeOffset.Now.AddMinutes(-14);
		var timestamp2 = DateTimeOffset.Now.AddMinutes(-12);
		var timestamp3 = DateTimeOffset.Now.AddMinutes(-10);
		var timestamp4 = DateTimeOffset.Now.AddMinutes(-5);

		var maxTime = TimeSpan.FromMinutes(1);
		var maxCount = 3;

		buffer.SetMaxBufferCount(maxCount);
		buffer.SetMaxBufferTime(maxTime);

		buffer.AddPoint(new TimedValue(timestamp1, 1), false);
		buffer.AddPoint(new TimedValue(timestamp2, 2), false);
		buffer.AddPoint(new TimedValue(timestamp3, 3), false);
		buffer.AddPoint(new TimedValue(timestamp4, 4), false);

		buffer.ApplyLimits(buffer.MaxCountToKeep, DateTimeOffset.Now.AddMinutes(-1));

		//count is larger than time
		var values = buffer.Points.ToList();

		values.Should().HaveCount(2);

		values[0].Timestamp.Should().Be(timestamp3);
		values[1].Timestamp.Should().Be(timestamp4);
	}

	[TestMethod]
	public void TimeSeriesBufferMaxTime()
	{
		var timestamp1 = DateTimeOffset.Now.AddMinutes(-20);
		var timestamp2 = DateTimeOffset.Now.AddMinutes(-15);
		var timestamp3 = DateTimeOffset.Now.AddMinutes(-10);
		var timestamp4 = DateTimeOffset.Now.AddMinutes(-5);

		var buffer = new TimeSeries("test", "");

		var maxTime = TimeSpan.FromMinutes(11);
		var maxCount = 1;

		buffer.SetMaxBufferCount(maxCount);
		buffer.SetMaxBufferTime(maxTime);

		buffer.AddPoint(new TimedValue(timestamp1, 1), false);
		buffer.AddPoint(new TimedValue(timestamp2, 2), false);
		buffer.AddPoint(new TimedValue(timestamp3, 3), false);
		buffer.AddPoint(new TimedValue(timestamp4, 4), false);

		buffer.ApplyLimits(buffer.MaxCountToKeep, DateTimeOffset.Now.AddMinutes(-11));

		//time is larger than count
		var values = buffer.Points.ToList();

		values.Should().HaveCount(2);

		values[0].Timestamp.Should().Be(timestamp3);
		values[1].Timestamp.Should().Be(timestamp4);
	}
}
