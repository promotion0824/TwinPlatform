using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Expressions.Visitor;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

#nullable disable

namespace WillowRules.Test.Bugs;

[TestClass]
public class TimeAggregationTests
{
	private TwinOverride equipment;
	private TwinOverride sensor1;
	private TwinOverride sensor2;

	[TestInitialize]
	public void Setup()
	{
		equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment", "");
		sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");
		sensor2 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor2", "a9463069-6db6-465d-b3e1-96969ac30c0a");
	}

	[TestMethod]
	public void TemporalMustNotSubstitute()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("result", "result", "[dtmi:com:willowinc:ZoneAirTemperatureSensor;1] + 1 - 1"),
			new RuleParameter("result1", "result1", "result*10"),
			new RuleParameter("result_average", "result_average", "AVERAGE(result, 5d)"),
			new RuleParameter("result_average1", "result_average1", "AVERAGE(result1, 5d)"),
			new RuleParameter("result_delta", "result_delta", "DELTA(result, 5d)"),
			new RuleParameter("result_delta1", "result_delta1", "DELTA(result1, 5d)"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-overcooling-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Expressions", "SingleSensorTimeAggregationTimeseries.csv");

		var sensors = new List<TwinOverride>()
		{
			sensor1,
		};

		var insight = bugHelper.GenerateInsightForPoint(
			rule,
			equipment,
			sensors,
			testcaseName: "Average_Over_Time");

		var average1 = bugHelper.Actor.TimedValues["result_average"].Points.ToList();
		var average2 = bugHelper.Actor.TimedValues["result_average1"].Points.ToList();

		for (int i = 1; i < average1.Count; i++)
		{
			average2[i].NumericValue.Should().NotBe(average1[i].NumericValue);
		}

		var delta1 = bugHelper.Actor.TimedValues["result_delta"].Points.ToList();
		var delta2 = bugHelper.Actor.TimedValues["result_delta1"].Points.ToList();

		for (int i = 1; i < average1.Count; i++)
		{
			delta2[i].NumericValue.Should().NotBe(delta1[i].NumericValue);
		}
	}

	[TestMethod]
	public void Single_Sensor_Over_Time_Test()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("result", "result", "[dtmi:com:willowinc:ZoneAirTemperatureSensor;1] + 1 - 1"),
			new RuleParameter("result_average", "result_average", "AVERAGE(result, 5d)"),
			new RuleParameter("result_count", "result_count", "COUNT(result, 5d)"),
			new RuleParameter("result_any", "result_any", "ANY(result, 5d)"),
			new RuleParameter("result_all", "result_all", "ALL(result, 5d)"),
			new RuleParameter("result_min", "result_min", "MIN(result, 5d)"),
			new RuleParameter("result_max", "result_max", "MAX(result, 5d)"),
			new RuleParameter("result_delta", "result_delta", "DELTA(result, 1h)"),
			new RuleParameter("result_stnd", "result_stnd", "STND(result, 5d)"),
			new RuleParameter("result_average_1", "result_average_1", "AVERAGE(result, 1d, -4d)"),
			new RuleParameter("result_count_1", "result_count_1", "COUNT(result, 1d, -4d)"),
			new RuleParameter("result_any_1", "result_any_1", "ANY(result, 1d, -4d)"),
			new RuleParameter("result_all_1", "result_all_1", "ALL(result, 1d, -4d)"),
			new RuleParameter("result_min_1", "result_min_1", "MIN(result, 1d, -4d)"),
			new RuleParameter("result_max_1", "result_max_1", "MAX(result, 1d, -4d)"),
			new RuleParameter("result_delta_1", "result_delta_1", "DELTA(result, 1d, -1h)"),
			new RuleParameter("result_stnd_1", "result_stnd_1", "STND(result, 1d, -4d)"),
			new RuleParameter("fallback", "fallback", "MAX(1, 1d)"),
		};

		var impactScores = new List<RuleParameter>()
		{
			new RuleParameter("score_delta1", "score_delta1", "DELTA(result, 1h)"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-overcooling-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			ImpactScores = impactScores,
			Elements = elements
		};

		var bugHelper = new BugHelper("Expressions", "SingleSensorTimeAggregationTimeseries.csv");

		var sensors = new List<TwinOverride>()
		{
			sensor1,
		};

		var insight = bugHelper.GenerateInsightForPoint(
			rule,
			equipment,
			sensors,
			testcaseName: "Average_Over_Time");

		bugHelper.Actor.TimedValues["result_count"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_any"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_all"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_average"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_min"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_max"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_delta"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_stnd"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["score_delta1"].Points.Count().Should().BeGreaterThan(0);

		bugHelper.Actor.TimedValues["result_count_1"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_any_1"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_all_1"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_average_1"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_min_1"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_max_1"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_delta_1"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_stnd_1"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["fallback"].Points.Count().Should().BeGreaterThan(0);

		//bugHelper.Actor.TimedValues["result_count"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		//bugHelper.Actor.TimedValues["result_any"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		//bugHelper.Actor.TimedValues["result_all"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		bugHelper.Actor.TimedValues["result_average"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		bugHelper.Actor.TimedValues["result_min"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		bugHelper.Actor.TimedValues["result_max"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		bugHelper.Actor.TimedValues["result_delta"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		bugHelper.Actor.TimedValues["result_stnd"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		bugHelper.Actor.TimedValues["score_delta1"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		bugHelper.Actor.TimedValues["fallback"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();

		//bugHelper.Actor.TimedValues["result_count_1"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		//bugHelper.Actor.TimedValues["result_any_1"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		//bugHelper.Actor.TimedValues["result_all_1"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		bugHelper.Actor.TimedValues["result_average_1"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		bugHelper.Actor.TimedValues["result_min_1"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		bugHelper.Actor.TimedValues["result_max_1"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		bugHelper.Actor.TimedValues["result_delta_1"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
		//bugHelper.Actor.TimedValues["result_stnd_1"].Points.Any(v => v.NumericValue != 0).Should().BeTrue();
	}

	[TestMethod]
	public void Multi_Sensor_Over_Time_Test()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("buffer", "buffer", "[dtmi:com:willowinc:TerminalUnit;1].[dtmi:com:willowinc:ZoneAirTemperatureSensor;1]", "array"),
			new RuleParameter("result_average", "result_average", "AVERAGE(buffer, 1d)", "array"),
			new RuleParameter("result_count", "result_count", "COUNT(buffer, 1d)", "array"),
			new RuleParameter("result_any", "result_any", "ANY(buffer, 1d)", "array"),
			new RuleParameter("result_all", "result_all", "ALL(buffer, 1d)", "array"),
			new RuleParameter("result_min", "result_min", "MIN(buffer, 1d)", "array"),
			new RuleParameter("result_max", "result_max", "MAX(buffer, 1d)", "array"),
			new RuleParameter("result_sum_of_average", "result_sum_of_average", "SUM(AVERAGE(buffer, 5d))", "array"),
			new RuleParameter("result_stnd", "result_stnd", "STND(buffer)", "array"),
			new RuleParameter("result_average_2", "result_average_2", "AVERAGE(buffer, 1d, -4d)", "array"),
			new RuleParameter("result_count_2", "result_count_2", "COUNTLEADING(buffer, 1d, -4d)", "array"),
			new RuleParameter("result_any_2", "result_any_2", "ANY(buffer, 1d, -4d)", "array"),
			new RuleParameter("result_all_2", "result_all_2", "ALL(buffer, 1d, -4d)", "array"),
			new RuleParameter("result_min_2", "result_min_2", "MIN(buffer, 1d, -4d)", "array"),
			new RuleParameter("result_max_2", "result_max_2", "MAX(buffer, 1d, -4d)", "array"),
			new RuleParameter("result_sum_of_average_2", "result_sum_of_average_2", "SUM(AVERAGE(buffer, 1d, -4d))", "array"),
			new RuleParameter("result_delta_2", "result_delta_2", "DELTA(buffer, 1d, -4d)", "array"),
			new RuleParameter("result", "result", "0", "array"),
			new RuleParameter("result_serialize_test", "result_serialize_test", "AVERAGE(buffer, (buffer + 5)d)", "array"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-overcooling-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Expressions", "MultiSensorTimeAggregationTimeseries.csv");

		var terminalUnit = new BasicDigitalTwinPoco()
		{
			Id = "tmu",
			Contents = new Dictionary<string, object>(),
			Metadata = new DigitalTwinMetadataPoco()
			{
				ModelId = "dtmi:com:willowinc:TerminalUnit;1"
			}
		};

		bugHelper.harness.AddTwinCache(terminalUnit).Wait();

		bugHelper.harness.AddForwardEdge(equipment.twinId, new Edge()
		{
			RelationshipType = "isPartOf",
			Destination = terminalUnit
		}).Wait();

		bugHelper.harness.AddBackwardEdge(terminalUnit.Id, new Edge()
		{
			RelationshipType = "isCapbilityOf",
			Destination = new BasicDigitalTwinPoco()
			{
				Id = sensor1.twinId,
				name = sensor1.twinId,
				Metadata = new DigitalTwinMetadataPoco()
				{
					ModelId = sensor1.modelId
				}
			}
		}).Wait();

		bugHelper.harness.AddBackwardEdge(terminalUnit.Id, new Edge()
		{
			RelationshipType = "isCapbilityOf",
			Destination = new BasicDigitalTwinPoco()
			{
				Id = sensor2.twinId,
				name = sensor2.twinId,
				Metadata = new DigitalTwinMetadataPoco()
				{
					ModelId = sensor2.modelId
				}
			}
		}).Wait();

		var sensors = new List<TwinOverride>()
		{
			sensor1,
			sensor2
		};

		var insight = bugHelper.GenerateInsightForPoint(
			rule,
			equipment,
			sensors,
			testcaseName: "Average_Over_Time_Multi",
			assertSimulation: false);

		var ruleInstance = bugHelper.RuleInstance!;

		var param = ruleInstance.RuleParametersBound.First(v => v.FieldId == "result_average");

		var serializer = new TokenExpressionSerializer();

		var serialized = serializer.Visit(param.PointExpression);

		serialized.Should().ContainAny("{AVERAGE(sensor1, 1[d]),AVERAGE(sensor2, 1[d])}", "{AVERAGE(sensor2, 1[d]),AVERAGE(sensor1, 1[d])}");

		param = ruleInstance.RuleParametersBound.First(v => v.FieldId == "result_average_2");

		serialized = serializer.Visit(param.PointExpression);

		serialized.Should().ContainAny("{AVERAGE(sensor1, 1[d], -4[d]),AVERAGE(sensor2, 1[d], -4[d])}", "{AVERAGE(sensor2, 1[d], -4[d]),AVERAGE(sensor1, 1[d], -4[d])}");

		param = ruleInstance.RuleParametersBound.First(v => v.FieldId == "result_serialize_test");

		serialized = serializer.Visit(param.PointExpression);

		serialized.Should().ContainAny(
			"{AVERAGE(sensor2, ({sensor2 + 5,sensor1 + 5})[d]),AVERAGE(sensor1, ({sensor2 + 5,sensor1 + 5})[d])}",
			"{AVERAGE(sensor1, ({sensor1 + 5,sensor2 + 5})[d]),AVERAGE(sensor2, ({sensor1 + 5,sensor2 + 5})[d])}");

		bugHelper.Actor.TimedValues.Count.Should().BeGreaterThan(0);

		bugHelper.Actor.TimedValues["result_count"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_any"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_all"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_average"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_min"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_max"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_sum_of_average"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_stnd"].Points.Count().Should().BeGreaterThan(0);

		bugHelper.Actor.TimedValues["result_count_2"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_any_2"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_all_2"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_average_2"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_min_2"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_max_2"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_sum_of_average_2"].Points.Count().Should().BeGreaterThan(0);
		bugHelper.Actor.TimedValues["result_delta_2"].Points.Count().Should().BeGreaterThan(0);
	}

	[TestMethod]
	public void ToTrackOrNotToTrack()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTime.With(0.01)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("value", "value", "[dtmi:com:willowinc:ZoneAirTemperatureSensor;1]"),//alias
			new RuleParameter("result1", "result1", "value"),//not alias
			new RuleParameter("result2", "result2", "value"),//alias and tracked because of average
			new RuleParameter("result3", "result3", "value + 1"),//tracked because min
			new RuleParameter("result4", "result4", "value + 1"),//no tracking
			new RuleParameter("result_average", "result_average", "AVERAGE(value, 5d)"),//no tracking
			new RuleParameter("result_max", "result_max", "MAX(result2, 5d)"),//no tracking
			new RuleParameter("result_min", "result_min", "MIN(result3, 5d)"),//no tracking
			new RuleParameter("result_cumulative", "result_cumulative", "value", CumulativeType.Accumulate),//tracked
			new RuleParameter("result_delta", "result_delta", "DELTA(value)"),//not tracked
			new RuleParameter("result", "result", "true"),//tracked by default
		};

		var scores = new List<RuleParameter>()
		{
			new RuleParameter("score1", "score1", "result_max"),//tracked because of score2
			new RuleParameter("score2", "score2", "MAX(score1, 30d)")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-overcooling-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			ImpactScores = scores,
			Elements = elements
		};

		var bugHelper = new BugHelper("Expressions", "ToTrackOrNotToTrack.csv");

		var sensors = new List<TwinOverride>()
		{
			sensor1,
		};

		bugHelper.GenerateInsightForPoint(
			rule,
			equipment,
			sensors,
			testcaseName: "Average_Over_Time",
			assertSimulation: false,//first point's gap is removed in execution but not from simulation
			maxDaysToKeep: 90);

		//alias values are removed during flush
		bugHelper.Actor.TimedValues.ContainsKey("value").Should().BeFalse();

		//only capabilite buffers are aliased (currently at least)
		bugHelper.Actor.TimedValues.ContainsKey("result1").Should().BeTrue();
		bugHelper.Actor.TimedValues.ContainsKey("result2").Should().BeTrue();

		bugHelper.Actor.TimedValues["result2"].Points.Count().Should().BeGreaterThan(3);
		bugHelper.Actor.TimedValues["result2"].MaxTimeToKeep.Value.Days.Should().Be(5);
		bugHelper.Actor.TimedValues["result3"].Points.Count().Should().BeGreaterThan(3);
		bugHelper.Actor.TimedValues["result3"].Points.Count().Should().BeGreaterThan(3);
		bugHelper.Actor.TimedValues["result4"].Points.Count().Should().BeLessThanOrEqualTo(3);
		bugHelper.Actor.TimedValues["result_average"].Points.Count().Should().BeLessThanOrEqualTo(3);
		bugHelper.Actor.TimedValues["result_max"].Points.Count().Should().BeLessThanOrEqualTo(3);
		bugHelper.Actor.TimedValues["result_min"].Points.Count().Should().BeLessThanOrEqualTo(3);
		bugHelper.Actor.TimedValues["result_cumulative"].Points.Count().Should().BeGreaterThan(3);
		bugHelper.Actor.TimedValues["result_delta"].Points.Count().Should().BeLessThanOrEqualTo(3);
		bugHelper.Actor.TimedValues["result"].Points.Count().Should().BeGreaterThanOrEqualTo(2);
		bugHelper.Actor.TimedValues["percentage_faulted"].Points.Count().Should().BeGreaterThan(3);
		bugHelper.Actor.TimedValues["TIME"].Points.Count().Should().BeGreaterThan(3);
		bugHelper.Actor.TimedValues["score1"].Points.Count().Should().BeGreaterThan(3);
		bugHelper.Actor.TimedValues["score2"].Points.Count().Should().BeLessThanOrEqualTo(3);

		bugHelper.Actor.TimedValues["result_max"].Points.Last().ValueDouble.Should().Be(9);

		//scores should not do data range checks for temporals
		var diff = bugHelper.Actor.TimedValues["score1"].Points.Last().Timestamp - bugHelper.Actor.TimedValues["score1"].Points.First().Timestamp;

		diff.Should().BeGreaterThan(TimeSpan.FromDays(30));

		bugHelper.Actor.OutputValues.Points.Any(v => v.Text == "Variable 'result2' does not have sufficient data for period 5.00:00:00").Should().Be(true);
		//now the TS's TTK should align with the temporal request of 90days
		bugHelper.harness.repositoryTimeSeriesBuffer.Data[0].MaxTimeToKeep.Value.Days.Should().Be(5);

		//should not overwrite time if no chance to register becuase no points to process (due to start time)
		bugHelper.RuleInstance = null;
		parameters[8] = new RuleParameter("result", "result", "false");

		bugHelper.GenerateInsightForPoint(
			rule,
			equipment,
			sensors,
			testcaseName: "Average_Over_Time",
			//only process one or two points
			startDate: bugHelper.Actor.Timestamp.AddDays(-1).DateTime,
			assertSimulation: false,//first point's gap is removed in execution but not from simulation
			maxDaysToKeep: 90);

		bugHelper.Actor.TimedValues["TIME"].Points.Count().Should().BeGreaterThan(3);
	}

	[TestMethod]
	public void BufferMaxSettingsCheck()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTime.With(0.01)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("result", "result", "MAX([dtmi:com:willowinc:ZoneAirTemperatureSensor;1], 1d)"),//alias
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-overcooling-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Expressions", "MultiSensorTimeAggregationTimeseries.csv");

		var sensors = new List<TwinOverride>()
		{
			sensor1,
		};

		bugHelper.GenerateInsightForPoint(
			rule,
			equipment,
			sensors,
			testcaseName: "Average_Over_Time",
			assertSimulation: false,//first point's gap is removed in execution but not from simulation
			maxDaysToKeep: 15);

		var unlinkedBuffer = bugHelper.harness.repositoryTimeSeriesBuffer.Data.First(v => v.Id == "a9463069-6db6-465d-b3e1-96969ac30c0a");
		var linkedBuffer = bugHelper.harness.repositoryTimeSeriesBuffer.Data.First(v => v.Id == "f9463069-6db6-465d-b3e1-96969ac30c0a");

		unlinkedBuffer.MaxCountToKeep.Value.Should().Be(3);
		unlinkedBuffer.MaxTimeToKeep.Should().BeNull();

		linkedBuffer.MaxCountToKeep.Should().BeNull();
		linkedBuffer.MaxTimeToKeep.Value.TotalDays.Should().Be(1);
	}
}
