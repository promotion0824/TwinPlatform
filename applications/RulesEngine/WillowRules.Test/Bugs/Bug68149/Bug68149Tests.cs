using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using System;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug68149Tests
{
	[TestMethod]
	public void Bug_68149_Test()
	{
		var elements = new List<RuleUIElement>()
		{
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("CO2 Sensor", "result", "OPTION([dtmi:com:willowinc:CO2AirQualitySensor;1])"),
		};

		var rule = new Rule()
		{
			Id = "unchanging",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateUnchanging.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Bug68149", "Timeseries.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:CO2AirQualitySensor;1", "f250d1db-b913-42f6-9b2e-f3c6ff87e1e8");

		insight.Should().NotBeNull();

		insight!.Occurrences.Count(v => v.IsFaulted).Should().Be(1);
	}

	[TestMethod]
	public void Bug_68149_AXA_Test()
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
			new RuleParameter("Cost impact", "cost_impact", "0.5 * TIME"),
			new RuleParameter("Comfort impact", "comfort_impact", "1.0 * TOTAL * TOTAL"),
			new RuleParameter("Reliability impact", "reliability_impact", "1.0 * TIME"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-co2-out-of-range",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Bug68149", "Timeseries_AXA.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:CO2AirQualitySensor;1", "200348dc-844c-4725-a602-5c3457129dbe", testcaseName: "AXA");

		insight.Should().NotBeNull();

		var faulted = insight!.Occurrences.Where(v => v.IsFaulted);

		faulted.Count().Should().BeGreaterThan(0);

		faulted.All(v => v.Started > new DateTime(2022, 7, 8, 0, 0, 0, DateTimeKind.Utc)
			&& v.Ended < new DateTime(2022, 7, 12, 0, 0, 0, DateTimeKind.Utc)).Should().BeTrue();
	}
}
