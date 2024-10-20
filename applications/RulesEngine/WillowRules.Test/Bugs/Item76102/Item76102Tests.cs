using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item76102Tests
{
	[TestMethod]
	public void Item76102_TestInsight_FaultOccurrnce_Should_Not_Clear()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(24)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("CO2 Sensor", "result", "[dtmi:com:willowinc:CO2AirQualitySensor;1] + 1 - 1"),
			new RuleParameter("Cost impact", "cost_impact", "0.5 * TIME"),
			new RuleParameter("Comfort impact", "comfort_impact", "0.0"),
			new RuleParameter("Reliability impact", "reliability_impact", "1.0 * TIME"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-co2-stuck",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateUnchanging.ID,
			Parameters = parameters,
			Elements = elements
		};

		//sensor went green on 23rd
		DateTime date = new DateTime(2022, 11, 22);

		var bugHelper = new BugHelper("Item76102", "Timeseries.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:CO2AirQualitySensor;1", "f2888d32-13cd-4207-b4cf-37ef4b8bf728", endDate: date);

		insight.IsFaulty.Should().BeTrue();

		insight.FaultedCount.Should().Be(1);

		//now clear out actor
		bugHelper.Actors.Clear();

		//the second run might not apply insight occurrence yet if outputs were only removed at flush time but the third should
		insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:CO2AirQualitySensor;1", "f2888d32-13cd-4207-b4cf-37ef4b8bf728", startDate: date, endDate: date.AddDays(30));

		//at this point old output values should be removed
		insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:CO2AirQualitySensor;1", "f2888d32-13cd-4207-b4cf-37ef4b8bf728", startDate: date.AddDays(30));

		//time has passed too far back, should be removed by now
		insight.FaultedCount.Should().Be(1);

		insight.Occurrences.Any(v => v.IsFaulted).Should().BeTrue();

		var lastOccurrence = insight.Occurrences.Last();

		lastOccurrence.Should().NotBeNull();

		lastOccurrence.IsFaulted.Should().BeFalse();

		//confirms that it has data up to the end of the timeseries data
		lastOccurrence!.Ended.Should().BeAfter(new DateTime(2023, 2, 2).ToUniversalTime());
	}
}
