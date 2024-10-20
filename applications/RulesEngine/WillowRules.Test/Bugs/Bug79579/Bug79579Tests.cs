using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using System;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug79579Tests
{
	[TestMethod]
	public void Bug_79579_Stuck_TextShouldShouldCorrectHoursOver24()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("CO2 Sensor", "result", "[dtmi:com:willowinc:ZoneAirTemperatureSensor;1]"),
		};

		var rule = new Rule()
		{
			Id = "unchanging",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateUnchanging.ID,
			Parameters = parameters,
			Elements = elements,
			Description = "Faulted Stuck on {result} for {TIME}",
		};

		var bugHelper = new BugHelper("Bug79579", "Timeseries.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "c301fc32-1a6d-448d-a976-519d160f38d6", outputImagesOnly: false);

		insight.Should().NotBeNull();

		insight!.Occurrences.Single(v => v.IsFaulted).Text.Should().StartWith("Faulted Stuck on 72.00 for 54009.01 s");
	}
}
