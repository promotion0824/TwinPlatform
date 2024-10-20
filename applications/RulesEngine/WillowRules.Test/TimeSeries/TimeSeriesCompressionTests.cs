using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;

namespace Willow.Rules.Test;

[TestClass]
public class TimeSeriesCompressionTests
{
	[TestMethod]
	public void CanCompressTimeSeries()
	{
		var values = new Dictionary<string, TimeSeriesBuffer>();

		var start = DateTimeOffset.Now;

		var tpv1 = new TimedValue(start, 1.0);
		var tpv2 = new TimedValue(start.AddMinutes(1), 1.0);
		var tpv3 = new TimedValue(start.AddMinutes(2), 1.0);

		ActorStateExtensions.PruneAndCheckValid(values, tpv1, "test", "");
		ActorStateExtensions.PruneAndCheckValid(values, tpv2, "test", "", compression: 0.1);
		ActorStateExtensions.PruneAndCheckValid(values, tpv3, "test", "", compression: 0.1);
		ActorStateExtensions.PruneAndCheckValid(values, tpv3, "test", "", compression: 0.1);

		values["test"].Points.Should().HaveCount(2);
	}

	[TestMethod]
	public void CanCompressMixedTimeSeries()
	{
		var values = new Dictionary<string, TimeSeriesBuffer>();

		var start = DateTimeOffset.Now;

		var tpv1 = new TimedValue(start, 1.0);
		var tpv2 = new TimedValue(start.AddMinutes(1), 1.0);
		var tpv3 = new TimedValue(start.AddMinutes(2), 1.0);
		var tpv4 = new TimedValue(start.AddMinutes(3), 1.0);
		var tpv5 = new TimedValue(start.AddMinutes(4), 2.0);
		var tpv5Again = new TimedValue(start.AddMinutes(4), 2.0);
		var tpv6 = new TimedValue(start.AddMinutes(5), 1.0);

		ActorStateExtensions.PruneAndCheckValid(values, tpv1, "test1", "");
		values.Values.SelectMany(v => v.Points).Should().HaveCount(1);
		values["test1"].Points.Should().HaveCount(1);
		ActorStateExtensions.PruneAndCheckValid(values, tpv2, "test2", "");
		values.Values.SelectMany(v => v.Points).Should().HaveCount(2);
		values["test1"].Points.Should().HaveCount(1);
		values["test2"].Points.Should().HaveCount(1);
		ActorStateExtensions.PruneAndCheckValid(values, tpv3, "test1", "", compression: 0.1);
		values.Values.SelectMany(v => v.Points).Should().HaveCount(3);
		values["test1"].Points.Should().HaveCount(2);
		values["test2"].Points.Should().HaveCount(1);
		ActorStateExtensions.PruneAndCheckValid(values, tpv4, "test2", "", compression: 0.1);
		values.Values.SelectMany(v => v.Points).Should().HaveCount(4);
		values["test1"].Points.Should().HaveCount(2);
		values["test2"].Points.Should().HaveCount(2);
		ActorStateExtensions.PruneAndCheckValid(values, tpv5, "test1", "", compression: 0.1);
		values.Values.SelectMany(v => v.Points).Should().HaveCount(5);
		values["test1"].Points.Should().HaveCount(3);
		values["test2"].Points.Should().HaveCount(2);
		ActorStateExtensions.PruneAndCheckValid(values, tpv5Again, "test1", "", compression: 0.1);
		values.Values.SelectMany(v => v.Points).Should().HaveCount(5);
		values["test1"].Points.Should().HaveCount(3);
		values["test2"].Points.Should().HaveCount(2);
		ActorStateExtensions.PruneAndCheckValid(values, tpv6, "test2", "", compression: 0.1);
		values.Values.SelectMany(v => v.Points).Should().HaveCount(5);
		values["test1"].Points.Should().HaveCount(3);
		values["test2"].Points.Should().HaveCount(2);
	}

	[TestMethod]
	public void DuplicatesAreIgnored()
	{
		var values = new Dictionary<string, TimeSeriesBuffer>();

		var start = DateTimeOffset.Now;

		var tpv1 = new TimedValue(start, 1.0);

		ActorStateExtensions.PruneAndCheckValid(values, tpv1, "test1", "");
		ActorStateExtensions.PruneAndCheckValid(values, tpv1, "test1", "", compression: 0.1);
		ActorStateExtensions.PruneAndCheckValid(values, tpv1, "test1", "", compression: 0.1);
		values["test1"].Points.Should().HaveCount(1);
	}
}
