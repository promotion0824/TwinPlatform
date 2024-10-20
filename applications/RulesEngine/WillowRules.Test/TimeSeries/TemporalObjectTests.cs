using FluentAssertions;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace Willow.Rules.Test
{
	[TestClass]
	public class TemporalObjectTests
	{
		[TestMethod]
		public void MustUseEndPeriod()
		{
			var timestamp = new DateTimeOffset(DateTime.Today).AddHours(-24);

			var buffer = new TimeSeries("test", "degC")
			{
				ModelId = "TemperatureSensor;1",
			};

			for (var i = 0; i <= 24; i++)
			{
				buffer.AddPoint(new TimedValue(timestamp.AddHours(i), 1), applyCompression: false);
			}

			var temporal = new TemporalObject(buffer, TimeSpan.FromDays(100), buffer.LastSeen);

			var result = (double)temporal.Sum(new UnitValue(Unit.Get("h"), 2), new UnitValue(Unit.Get("h"), -5));

			result.Should().Be(3);
		}

		[TestMethod]
		public void MustKeepDelta()
		{
			var timestamp = new DateTimeOffset(DateTime.Today.AddYears(-1)).AddHours(-24);

			//only keep last 3 points
			var buffer = new TimeSeries("test", "degC")
			{
				MaxCountToKeep = 3
			};

			//applies more compression for older values
			var compressed = new TimeSeries("test", "degC")
			{
				MaxTimeToKeep = TimeSpan.FromDays(30)
			};

			for (var i = 0; i <= 24 * 100; i++)
			{
				double val = Random.Shared.NextDouble();

				buffer.AddPoint(new TimedValue(timestamp.AddHours(i), val), applyCompression: true, includeDataQualityCheck: false);
				compressed.AddPoint(new TimedValue(timestamp.AddHours(i), val), applyCompression: true, includeDataQualityCheck: false);

				buffer.ApplyLimits(timestamp.AddHours(i).DateTime, TimeSpan.FromDays(15), TimeSpan.FromDays(15));
				compressed.ApplyLimits(timestamp.AddHours(i).DateTime, TimeSpan.FromDays(15), TimeSpan.FromDays(15));

				if (i > 1)
				{
					buffer.CompressionState.LastDelta.Should().Be(compressed.CompressionState.LastDelta);
				}				
			}
		}
	}
}
