using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item77166Tests
{
	[TestMethod]
	public void Should_Calculate_Faulty_Time_AnyFault()
	{
		double percentage = 0.666;

		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(percentage)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Water Flow", "water_flow", "OPTION([dtmi:com:willowinc:WaterFlowSensor;1])"),
			new RuleParameter("Leak Detection", "result", "([water_flow] > 10)"),
		};

		var impactScores = new List<RuleParameter>()
		{
			new RuleParameter("Impact", "impact", "[TIME]")
		};

		var rule = new Rule()
		{
			Id = "meter-water-flow-leak-detection-metric",
			PrimaryModelId = "dtmi:com:willowinc:WaterMeter;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			ImpactScores = impactScores,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item77166", "TimeseriesAnyFault.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:WaterFlowSensor;1", "6f483d75-7de9-479d-9779-1bb620f7b5d4", testcaseName: "AnyFault");

		var actor = bugHelper.Actor!;

		var pointsFirstRun = actor.TimedValues["TIME"].Points.ToList();

		var first = pointsFirstRun.First();
		
		foreach (var p in pointsFirstRun.Skip(1))
		{
			p.ValueDouble!.Value.Should().BeGreaterThan(first.ValueDouble!.Value);
			first = p;
		}
		
		var faultTime = actor.TimedValues["TIME"].GetLastValueDouble()!;
		
		Math.Round(faultTime.Value, 2).Should().BeGreaterThan(0);
		
		var impact = insight.ImpactScores.First(v => v.FieldId == "impact");
		
		Math.Round(impact.Score, 2).Should().BeGreaterThan(0);

		bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:WaterFlowSensor;1", "6f483d75-7de9-479d-9779-1bb620f7b5d4", testcaseName: "AnyFault");

		actor = bugHelper.Actor!;

		var pointsSecondRun = actor.TimedValues["TIME"].Points.ToList();

		pointsSecondRun.Should().BeEquivalentTo(pointsFirstRun);
	}

	[TestMethod]
	public void Should_Calculate_Faulty_Time_Unchanging()
	{
		var elements = new List<RuleUIElement>()
		{
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("CO2 Sensor", "result", "OPTION([dtmi:com:willowinc:CO2AirQualitySensor;1])"),
		};

		var impactScores = new List<RuleParameter>()
		{
			new RuleParameter("Impact", "impact", "[TIME]")
		};

		var rule = new Rule()
		{
			Id = "unchanging",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateUnchanging.ID,
			Parameters = parameters,
			ImpactScores = impactScores,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item77166", "TimeseriesUnchanging.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:CO2AirQualitySensor;1", "f250d1db-b913-42f6-9b2e-f3c6ff87e1e8", testcaseName: "Unchanging");

		var actor = bugHelper.Actor!;

		var faultTime = actor.TimedValues["TIME"].GetLastValueDouble()!;

		Math.Round(faultTime.Value, 2).Should().BeGreaterThan(0);

		var impact = insight.ImpactScores.First(v => v.FieldId == "impact");

		Math.Round(impact.Score, 2).Should().BeGreaterThan(0);
	}

	[TestMethod]
	public void Should_Calculate_Faulty_Time_AnyHysteresis()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.MinTrigger.With(310),
			Fields.MaxTrigger.With(1850),
			Fields.PercentageOfTime.With(0.166666666666666)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("CO2 Sensor", "result", "OPTION([dtmi:com:willowinc:CO2AirQualitySensor;1])"),
		};

		var impactScores = new List<RuleParameter>()
		{
			new RuleParameter("Impact", "impact", "[TIME]")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-co2-out-of-range",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			ImpactScores = impactScores,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item77166", "TimeseriesAnyHysteresis.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:CO2AirQualitySensor;1", "200348dc-844c-4725-a602-5c3457129dbe", testcaseName: "AnyHysteresis");

		var actor = bugHelper.Actor!;

		var faultTime = actor.TimedValues["TIME"].GetLastValueDouble()!;

		Math.Round(faultTime.Value, 2).Should().BeGreaterThan(0);

		var impact = insight.ImpactScores.First(v => v.FieldId == "impact");

		Math.Round(impact.Score, 2).Should().BeGreaterThan(0);
	}
}
