using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;

namespace Willow.Rules.Test;

[TestClass]
public class TrajectoryCompressorTests
{
	[TestMethod]
	public void CanCompressCollinearPoints()
	{
		var now = DateTimeOffset.Now;

		// collinear values should compress even with a very small percentage

		var values = new List<TimedValue>
		{
			new TimedValue(now.AddMinutes(-6), 0),
			new TimedValue(now.AddMinutes(-5), 1),
			new TimedValue(now.AddMinutes(-4), 2),
			new TimedValue(now.AddMinutes(-3), 3),
			new TimedValue(now.AddMinutes(-2), 4),
			new TimedValue(now.AddMinutes(-1), 5)
		};

		var trajectoryCompressor = new TrajectoryCompressor(0.005);

		var result = new LinkedList<TimedValue>();
		result.Populate(trajectoryCompressor, values);

		result.Count.Should().Be(2);
		result.First!.Value.NumericValue.Should().Be(0);
		result.Last!.Value.NumericValue.Should().Be(5);
	}

	[TestMethod]
	public void CanCompressNearCollinearPoints()
	{
		var now = DateTimeOffset.Now;

		var values = new List<TimedValue>
		{
			new TimedValue(now.AddMinutes(-3), 0.0),
			new TimedValue(now.AddMinutes(-2), 1.01),
			new TimedValue(now.AddMinutes(-1), 1.98),
			new TimedValue(now.AddMinutes(-0), 3.0)
		};

		var trajectoryCompressor = new TrajectoryCompressor(0.05);
		var result = new LinkedList<TimedValue>();
		result.Populate(trajectoryCompressor, values);

		result.Count.Should().Be(2);
		result.First!.Value.NumericValue.Should().Be(0);
		result.Last!.Value.NumericValue.Should().Be(3.0);
	}

	[TestMethod]
	public void CannotCompressNonCollinearPoints()
	{
		var now = DateTimeOffset.Now;

		var values = new List<TimedValue>
		{
			new TimedValue(now.AddMinutes(-3), 0),
			new TimedValue(now.AddMinutes(-2), 1.5),
			new TimedValue(now.AddMinutes(-1), 2)
		};

		var trajectoryCompressor = new TrajectoryCompressor(0.005);
		var result = new LinkedList<TimedValue>();
		result.Populate(trajectoryCompressor, values);

		result.Count.Should().Be(3);
	}

	[TestMethod]
	public void CannotCompressSquareWave()
	{
		var now = DateTimeOffset.Now;

		// Square wave 0/1 like a boolean flipping
		var values = Enumerable.Range(0, 400).Select(x => new TimedValue(now.AddMinutes(-500 + x), x % 2));

		var trajectoryCompressor = new TrajectoryCompressor(0.005);
		var result = new LinkedList<TimedValue>();
		result.Populate(trajectoryCompressor, values);

		result.Count.Should().Be(400);
	}

	private const double epsilon = 5E-13;
	private const int N = 400;
	private const int INTERVAL = 15;

	private const double AMPLITUDE = 200;

	[DataTestMethod]
	[DataRow(0.1, 12)]
	[DataRow(0.05, 15)]
	[DataRow(0.01, 32)]
	[DataRow(0.001, 101)]
	public void CanCompressSineWaveToApproximation(double percentage, int expected)
	{
		var trajectoryCompressor = new TrajectoryCompressor(percentage);
		var now = DateTimeOffset.Now;

		var state = new TrajectoryCompressorState();
		List<(DateTimeOffset timestamp, double value)> result = new();

		for (int i = 0; i <= N; i++)
		{
			DateTimeOffset timestamp = now.AddMinutes(INTERVAL * i);
			double value = AMPLITUDE + AMPLITUDE * Math.Sin(Math.PI * 2 * i / N);

			void update(DateTimeOffset previous, DateTimeOffset now, double value)
			{
				result.Last().timestamp.Should().Be(previous);
				result[result.Count - 1] = (now, value);
			}

			trajectoryCompressor.Add(state, timestamp, value,
				(t, v) => result.Add((t, v)), update);
		}

		result.Should().HaveCount(expected);

		result.First().timestamp.Should().Be(now);
		result.First().value.Should().BeApproximately(AMPLITUDE, epsilon);

		result.Last().timestamp.Should().Be(now.AddMinutes(INTERVAL * N));
		result.Last().value.Should().BeApproximately(AMPLITUDE, epsilon);

		// Plot these in Excel to check
		// foreach (var line in result)
		// {
		// 	Console.WriteLine($"{(line.timestamp - now).TotalMinutes}, {line.value:0.00}");
		// }
	}

	[DataTestMethod]
	[DataRow(0.1, 22)]
	// [DataRow(0.05, 15)]
	// [DataRow(0.01, 32)]
	[DataRow(0.001, 262)]
	public void CanCompressComplexWaveToApproximation(double percentage, int expected)
	{
		var trajectoryCompressor = new TrajectoryCompressor(percentage);
		var now = DateTimeOffset.Now;

		var state = new TrajectoryCompressorState();
		List<(DateTimeOffset timestamp, double value)> result = new();

		for (int i = 0; i <= N; i++)
		{
			DateTimeOffset timestamp = now.AddMinutes(INTERVAL * i);
			double value = AMPLITUDE + AMPLITUDE * Math.Sin(Math.PI * 2 * i / N)
				+ AMPLITUDE / 2 * Math.Sin(Math.PI * 6 * i / N + 0.5);

			void update(DateTimeOffset previous, DateTimeOffset now, double value)
			{
				result.Last().timestamp.Should().Be(previous);
				result[result.Count - 1] = (now, value);
			}

			trajectoryCompressor.Add(state, timestamp, value,
				(t, v) => result.Add((t, v)), update);
		}

		result.Should().HaveCount(expected);

		// Plot these in Excel to check
		// foreach (var line in result)
		// {
		// 	Console.WriteLine($"{(line.timestamp - now).TotalMinutes}, {line.value:0.00}");
		// }
	}

}
