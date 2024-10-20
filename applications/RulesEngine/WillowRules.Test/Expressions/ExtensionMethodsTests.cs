using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions;
using Willow.Rules.Model;

namespace WillowExpressions.Test;

[TestClass]
public class ExtensionMethodsTests
{
	private readonly Random r = new Random();

	const string pointEntityId = "Point";

	private List<TimedValue> GetDoublesTestList(int size)
	{
		List<TimedValue> list = new();

		for (int i = 0; i < size; i++)
		{
			double random = r.NextDouble() * 100;
			list.Add(new TimedValue(DateTime.Now.AddSeconds(i), random));
		}

		return list;
	}
	private List<TimedValue> GetBoolsTestList(int size)
	{
		List<TimedValue> list = new();

		for (int i = 0; i < size; i++)
		{
			bool v = r.NextDouble() > 0.5;
			list.Add(new TimedValue(DateTime.Now.AddSeconds(i), v));
		}

		return list;
	}

	[TestMethod]
	public void CanFindMinOfDoubleList()
	{
		var list = GetDoublesTestList(5);
		var values = list.Select(x => x.ValueDouble);
		list.Min(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1), TimedValue.Invalid).Should().Be(values.Min());
	}

	[TestMethod]
	public void CanFindMaxOfDoubleList()
	{
		var list = GetDoublesTestList(5);
		var values = list.Select(x => x.ValueDouble);
		list.Max(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1), TimedValue.Invalid).Should().Be(values.Max());
	}

	[TestMethod]
	public void CanFindMinOfBoolList()
	{
		var list = GetBoolsTestList(500);
		list.Min(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1), TimedValue.Invalid).Should().Be(0);
	}

	[TestMethod]
	public void CanFindMaxOfBoolList()
	{
		var list = GetBoolsTestList(500);
		list.Max(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1), TimedValue.Invalid).Should().Be(1);
	}

	[TestMethod]
	public void CanFindAverageOfDoubleList()
	{
		List<TimedValue> list = new()
			{
				new TimedValue(DateTime.Today, 5.0),
				new TimedValue(DateTime.Today.AddSeconds(1), 15.0),
				new TimedValue(DateTime.Today.AddSeconds(2), 10.0)
			};

		list.Average(DateTime.Today, DateTime.Today.AddSeconds(2)).Should().Be(11.25);
	}

	[TestMethod]
	public void CanFindAverageFromSampleActor()
	{
		var actor = new ActorState("discharge-air-out-of-range-2", "MS -PS-B122-VSVAV.L01.39-DAT_Discharge air out of range min max 2", DateTimeOffset.Now, 1)
		{
		};

		ActorStateExtensions.PruneAndCheckValid(actor.TimedValues, new TimedValue(DateTimeOffset.Now.AddMinutes(-2), 71.6d), "result", "");
		ActorStateExtensions.PruneAndCheckValid(actor.TimedValues, new TimedValue(DateTimeOffset.Now.AddMinutes(-1), 71.5d), "result", "");
		ActorStateExtensions.PruneAndCheckValid(actor.TimedValues, new TimedValue(DateTimeOffset.Now.AddMinutes(-2), 69.4d), "8444fb74-3a37-4d9d-beb0-3eb6a774302b", "");
		ActorStateExtensions.PruneAndCheckValid(actor.TimedValues, new TimedValue(DateTimeOffset.Now.AddMinutes(-2), 0.003680722317621387d), "AREA_OUTSIDE_OCCUPIED", "");

		actor.ValidOutput(DateTimeOffset.Now, false, Env.Empty.Push());

		var results = actor.Filter("result", "bool").Points.ToList();
		results.Should().NotBeEmpty();
		var earliest = results.Select(x => x.Timestamp).Min();
		var latest = results.Select(x => x.Timestamp).Max();

		double average = results.Average(earliest, latest);
		double min = results.Min(earliest, latest, TimedValue.Invalid);
		double max = results.Max(earliest, latest, TimedValue.Invalid);

		Console.WriteLine($"{min} - {average} - {max}");
		min.Should().BeLessOrEqualTo(average);
		average.Should().BeLessOrEqualTo(max);
	}

	[TestMethod]
	public void CanFindInterpolatedAverageOfDoubleList()
	{
		List<TimedValue> list = new()
			{
				new TimedValue(DateTime.Today, 5.0),
				new TimedValue(DateTime.Today.AddSeconds(1), 15.0),
				new TimedValue(DateTime.Today.AddSeconds(2), 10.0)
			};

		list.Average(DateTime.Today.AddSeconds(0.5), DateTime.Today.AddSeconds(1.5)).Should().Be(13.125);
	}

	[TestMethod]
	public void CanFindInterpolatedMinOfDoubleList()
	{
		List<TimedValue> list = new()
			{
				new TimedValue(DateTime.Today, 1),
				new TimedValue(DateTime.Today.AddSeconds(10), 20),
				new TimedValue(DateTime.Today.AddSeconds(20), 19)
			};

		list.Min(DateTime.Today.AddSeconds(5), DateTime.Today.AddSeconds(30), TimedValue.Invalid).Should().Be(10.5);
	}

	[TestMethod]
	public void CanFindInterpolatedMaxOfDoubleList()
	{
		List<TimedValue> list = new()
			{
				new TimedValue(DateTime.Today, 100),
				new TimedValue(DateTime.Today.AddSeconds(10), 20),
				new TimedValue(DateTime.Today.AddSeconds(20), 19)
			};

		list.Max(DateTime.Today.AddSeconds(5), DateTime.Today.AddSeconds(30), TimedValue.Invalid).Should().Be(60.0);
	}

	[TestMethod]
	public void CanFindInterpolatedAverageOfBoolList()
	{
		List<TimedValue> list = new()
			{
				new TimedValue(DateTime.Today, false),
				new TimedValue(DateTime.Today.AddSeconds(1), true),
				new TimedValue(DateTime.Today.AddSeconds(2), false)
			};

		list.Average(DateTime.Today.AddSeconds(0.5), DateTime.Today.AddSeconds(1.5)).Should().Be(0.75);
	}
}
