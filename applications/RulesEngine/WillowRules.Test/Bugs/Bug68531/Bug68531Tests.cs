using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug68531Tests
{
	[TestMethod]
	public void Bug_68531_24HourPeriod_Test()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(24),
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("CO2 Sensor", "result", "OPTION([dtmi:com:willowinc:ZoneAirTemp;1])"),
		};

		var rule = new Rule()
		{
			Id = "unchanging",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateUnchanging.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Bug68531", "Timeseries.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:ZoneAirTemp;1", "dcd4e8e3-baaf-46bf-b673-8adf7ff07342", testcaseName: "24hr");

		insight!.FaultedCount.Should().Be(0);
	}
}
