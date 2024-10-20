using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item115923Tests
{
	[TestMethod]
	public void Item115923_TestWithNoData()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.5)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Fan Speed", "fan_speed", "[dtmi:com:willowinc:FanCurrentSensor;1]"),
			new RuleParameter("Expression", "result", "fan_speed > 1"),
		};

		var rule = new Rule()
		{
			Id = "food-display-case-evaporator-fan-over-design",
			PrimaryModelId = "dtmi:com:willowinc:RefrigeratedFoodDisplayCase;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item115923", "Timeseries.csv");

		var sensors = new List<TwinOverride>()
		{
			new TwinOverride(
				"dtmi:com:willowinc:FanCurrentSensor;1",
				"944-CKTS-BC-C03b-MD-MEAT-EVAP-FAN-AMPS",
				trendId: "50158491-a62d-4b45-98be-f505b7181f0d",
				externalId: "PNTQCtHWKrdfFUkQNcyoXsEKs"
				)
		};

		var equipment = new TwinOverride(rule.PrimaryModelId, "equipment");

		var insight = bugHelper.GenerateInsightForPoint(rule, equipment, sensors, assertSimulation: false);

		var faultedOccurrence = insight.Occurrences.First(v => v.Ended == DateTime.Parse("2023-09-30T23:50:44Z"));

		faultedOccurrence.IsFaulted.Should().BeTrue();

		var nextOccurrence = insight.Occurrences[insight.Occurrences.IndexOf(faultedOccurrence) + 1];
		nextOccurrence.IsValid.Should().BeFalse();
		//invalid data start date should NOT start where the previous fault ended
		nextOccurrence.Started.ToUniversalTime().Should().Be(DateTime.Parse("2024/01/29 23:50:44 +00:00"));
		nextOccurrence.Ended.ToUniversalTime().Should().Be(DateTime.Parse("2024/01/29 23:50:44 +00:00"));
	}

	[TestMethod]
	public void Item115923_TestWithLongGap()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.5)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Fan Speed", "fan_speed", "[dtmi:com:willowinc:FanCurrentSensor;1]"),
			new RuleParameter("Expression", "result", "fan_speed > 1"),
		};

		var rule = new Rule()
		{
			Id = "food-display-case-evaporator-fan-over-design",
			PrimaryModelId = "dtmi:com:willowinc:RefrigeratedFoodDisplayCase;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item115923", "TimeseriesBigGap.csv");

		//the buffer data swaps from trnedid to externalid
		var sensors = new List<TwinOverride>()
		{
			new TwinOverride(
				"dtmi:com:willowinc:FanCurrentSensor;1",
				"944-CKTS-BC-C03b-MD-MEAT-EVAP-FAN-AMPS",
				trendId: "50158491-a62d-4b45-98be-f505b7181f0d",
				externalId: "PNTQCtHWKrdfFUkQNcyoXsEKs",
				connectorId: "00000000-35c5-4415-a4b3-7b798d0568e8"
				)
		};

		var equipment = new TwinOverride(rule.PrimaryModelId, "equipment");

		var insight = bugHelper.GenerateInsightForPoint(rule, equipment, sensors, maxDaysToKeep: 90, assertSimulation: false);

		var faultedOccurrence = insight.Occurrences.First(v => v.Ended == DateTime.Parse("2023-09-30T23:50:44Z").ToUniversalTime());

		faultedOccurrence.IsFaulted.Should().BeTrue();

		var nextOccurrence = insight.Occurrences[insight.Occurrences.IndexOf(faultedOccurrence) + 1];
		nextOccurrence.IsValid.Should().BeFalse();
		nextOccurrence.Started.ToUniversalTime().Should().Be(DateTime.Parse("2024/01/30 08:49:17 +00:00").ToUniversalTime());
		nextOccurrence.Ended.ToUniversalTime().Should().Be(DateTime.Parse("2024/01/30 08:49:17 +00:00").ToUniversalTime());

		nextOccurrence = insight.Occurrences[insight.Occurrences.IndexOf(nextOccurrence) + 1];
		nextOccurrence.IsFaulted.Should().BeTrue();
		nextOccurrence.Started.ToUniversalTime().Should().Be(DateTime.Parse("2024/01/30 08:59:17 +00:00").ToUniversalTime());
		nextOccurrence.Ended.ToUniversalTime().Should().Be(DateTime.Parse("2024/01/30 12:59:18 +00:00").ToUniversalTime());
	}
}
