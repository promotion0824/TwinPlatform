using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item91377Tests
{
	[TestMethod]
	public void Item91377_ShouldWaitBeforeReopeningResolvedInsight()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(0.1)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("CO2 Sensor", "result", "[dtmi:com:willowinc:CO2AirQualitySensor;1] > 0"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-co2-stuck",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements,
			CommandEnabled = true
		};

		var bugHelper = new BugHelper("Item91377", "Timeseries.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:CO2AirQualitySensor;1", "f2888d32-13cd-4207-b4cf-37ef4b8bf728");

		insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:CO2AirQualitySensor;1", "f2888d32-13cd-4207-b4cf-37ef4b8bf728");

		insight.IsFaulty.Should().BeTrue();
		insight.NextAllowedSyncDateUTC.Should().BeExactly(DateTimeOffset.MinValue);

		//simulate status change to resolved
		insight.Status = InsightStatus.Resolved;

		insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:CO2AirQualitySensor;1", "f2888d32-13cd-4207-b4cf-37ef4b8bf728");

		insight.NextAllowedSyncDateUTC.Should().BeCloseTo(insight.LastSyncDateUTC + TimeSpan.FromHours(12), TimeSpan.FromSeconds(1));

		//fake an open status
		insight.Status = InsightStatus.Open;

		insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:CO2AirQualitySensor;1", "f2888d32-13cd-4207-b4cf-37ef4b8bf728");

		insight.NextAllowedSyncDateUTC.Should().BeExactly(DateTimeOffset.MinValue);
	}
}
