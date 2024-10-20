using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class ImpactScoreTests
{
	[TestMethod]
	public async Task OneScoreDependentOnAnother()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.MinTrigger.With(12),
			Fields.MaxTrigger.With(35),
			Fields.PercentageOfTime.With(0.01)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Zone Temperature Setpoint", "result", "OPTION([dtmi:com:willowinc:EffectiveZoneAirTemperatureSetpoint;1],[dtmi:com:willowinc:ZoneAirTemperatureSetpoint;1])"),
		};

		var scores = new List<RuleParameter>()
		{
			new RuleParameter("Cost impact", "cost_impact", "(0.5 * TIME) + 1", "USD"),
			new RuleParameter("Comfort impact", "comfort_impact", "[cost_impact] + 2"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-setpoint-out-of-range",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			ImpactScores = scores,
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, "equipment", "dtmi:com:willowinc:EffectiveZoneAirTemperatureSetpoint;1", "a10df277-c937-4420-9d20-9c39aa15c5cc");

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("ImpactScore", "OnceScoreDependentOnAnother.csv");

		(var insights, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single();

		actor.OutputValues.Count.Should().BeGreaterThan(0);

		var costTS = actor.TimedValues["cost_impact"].Points.ToList();
		var comfortTS = actor.TimedValues["comfort_impact"].Points.ToList();

		for (var i = 0; i < costTS.Count; i++)
		{
			comfortTS[i].NumericValue.Should().Be(costTS[i].NumericValue + 2);
		}

		insights.Count.Should().Be(1);

		var insight = insights[0];

		insight.ImpactScores.Any(v => v.FieldId == "cost_impact").Should().BeTrue();
		insight.ImpactScores.Any(v => v.FieldId == "comfort_impact").Should().BeTrue();
		insight.ImpactScores.First(v => v.FieldId == "cost_impact").Unit.Should().Be("USD");
	}

	[TestMethod]
	public async Task Bug_87977_ShouldOnlySendDataEveryHour()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(50.0),
			Fields.OverHowManyHours.With(1),
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Occupancy", "Occupancy", "[dtmi:com:willowinc:PeopleCountSensor;1]"),
			new RuleParameter("Other", "Other", "[dtmi:com:willowinc:OtherSensor;1]"),
			new RuleParameter("Expression", "result", "[Occupancy] > 0", "")
		};

		var impactScores = new List<RuleParameter>()
		{
			new RuleParameter("is1", "is1", "[Occupancy]"),
			new RuleParameter("is2", "is2", "[Other]"),
		};

		var rule = new Rule()
		{
			Id = "floor-unoccupied-when-overtime-air-requested",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements,
			ImpactScores = impactScores
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:PeopleCountSensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:OtherSensor;1", "sensor2", trendId: "a9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1,
			sensor2
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("ImpactScore", "Bug87977Timeseries.csv");

		await harness.ExecuteRules(filePath);

		var output = harness.eventHubService.Output.Where(v => v.ExternalId == "equipment_floor-unoccupied-when-overtime-air-requested_is1_V1");

		//1 point per hour total
		output.Count().Should().Be(36);

		var firstDayData = output.Where(v => v.SourceTimestamp.Day == 24).ToList();

		var secondDayData = output.Where(v => v.SourceTimestamp.Day == 25).ToList();

		var previous = firstDayData.First();

		previous.SourceTimestamp.Hour.Should().Be(4);

		foreach (var point in firstDayData.Skip(1))
		{
			previous.SourceTimestamp.Hour.Should().BeLessThan(point.SourceTimestamp.Hour);
			previous = point;
		}

		previous = secondDayData.First();
		previous.SourceTimestamp.Hour.Should().Be(0);

		foreach (var point in secondDayData.Skip(1))
		{
			previous.SourceTimestamp.Hour.Should().BeLessThan(point.SourceTimestamp.Hour);
			previous = point;
		}

		output = harness.eventHubService.Output.Where(v => v.ExternalId == "equipment_floor-unoccupied-when-overtime-air-requested_is2_V1");

		//the last day for is2 should have alot less output due to compressed 0's and 1's
		output.Where(v => v.SourceTimestamp.Day == 26).Count().Should().Be(12);

		firstDayData = output.Where(v => v.SourceTimestamp.Day == 26).ToList();

		previous = firstDayData.First();

		previous.SourceTimestamp.Hour.Should().Be(0);

		foreach (var point in firstDayData.Skip(1))
		{
			previous.SourceTimestamp.Hour.Should().BeLessThan(point.SourceTimestamp.Hour);
			previous = point;
		}
	}

	[TestMethod]
	public async Task ShoulIncrementVersionOnRerun()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(50.0),
			Fields.OverHowManyHours.With(1),
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Occupancy", "Occupancy", "[dtmi:com:willowinc:PeopleCountSensor;1]"),
			new RuleParameter("Expression", "result", "[Occupancy] > 0", "")
		};

		var impactScores = new List<RuleParameter>()
		{
			new RuleParameter("is1", "is1", "[Occupancy]")
		};

		var rule = new Rule()
		{
			Id = "floor-unoccupied-when-overtime-air-requested",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements,
			ImpactScores = impactScores,
			CommandEnabled = true
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment");
		var equipment2 = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment2");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:PeopleCountSensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:PeopleCountSensor;1", "sensor2", trendId: "a9463069-6db6-465d-b3e1-96969ac30c0a");

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>()
		{
			sensor1
		});

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("ImpactScore", "Bug87977Timeseries.csv");

		//realtime should not increment
		(var insights, _, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		var insight = insights[0];

		insight.IsFaulty.Should().Be(false);

		var lastSyncDate = insight.LastSyncDateUTC;

		insight.LastSyncDateUTC.Should().NotBe(DateTimeOffset.MinValue);

		var output = harness.eventHubService.Output.Where(v => v.ExternalId == "equipment_floor-unoccupied-when-overtime-air-requested_is1_V1");

		output.Count().Should().BeCloseTo(36, 3);

		harness.eventHubService.Output.Clear();

		harness.OverrideCaches(rule, equipment2, new List<TwinOverride>()
		{
			sensor2
		});

		await harness.GenerateRuleInstances();

		await harness.ExecuteRules(filePath, assertSimulation: false, isRealtime: false);

		output = harness.eventHubService.Output.Where(v => v.ExternalId == "equipment_floor-unoccupied-when-overtime-air-requested_is1_V2");

		output.Count().Should().BeCloseTo(36, 4);

		output = harness.eventHubService.Output.Where(v => v.ExternalId == "equipment2_floor-unoccupied-when-overtime-air-requested_is1_V2");

		output.Count().Should().BeCloseTo(21, 3);

		harness.eventHubService.Output.Clear();

		//second run
		await harness.ExecuteRules(filePath, assertSimulation: false, isRealtime: false);

		output = harness.eventHubService.Output.Where(v => v.ExternalId == "equipment_floor-unoccupied-when-overtime-air-requested_is1_V3");

		output.Count().Should().BeCloseTo(36, 4);

		output = harness.eventHubService.Output.Where(v => v.ExternalId == "equipment2_floor-unoccupied-when-overtime-air-requested_is1_V3");

		output.Count().Should().BeCloseTo(21, 2);

		harness.eventHubService.Output.Clear();

		//realtime should not increment
		(var insignts, _, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		insight = insights[0];

		insight.IsFaulty.Should().Be(false);

		//syncing for valid, non-faulty, insights should be forced during batches
		insight.LastSyncDateUTC.Should().BeAfter(lastSyncDate);

		output = harness.eventHubService.Output.Where(v => v.ExternalId == "equipment_floor-unoccupied-when-overtime-air-requested_is1_V3");

		//1 point per hour total
		output.Count().Should().BeCloseTo(36, 4);

		output = harness.eventHubService.Output.Where(v => v.ExternalId == "equipment2_floor-unoccupied-when-overtime-air-requested_is1_V3");

		//1 point per hour total
		output.Count().Should().BeCloseTo(21, 2);
	}
}
