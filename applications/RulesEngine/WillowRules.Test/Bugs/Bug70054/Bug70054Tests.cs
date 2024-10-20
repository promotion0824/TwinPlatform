using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug70054Tests
{
	[TestMethod]
	public void Bug70054_Test()
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
			new RuleParameter("Zone Air Temperature Sensor", "result", "OPTION([dtmi:com:willowinc:ZoneAirTemperatureSensor;1])"),
			new RuleParameter("Cost impact", "cost_impact", "0.5 * TIME"),
			new RuleParameter("Comfort impact", "comfort_impact", "1.0 * TOTAL * TOTAL"),
			new RuleParameter("Reliability impact", "reliability_impact", "1.0 * TIME"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-out-of-range",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Bug70054", "Timeseries.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "200348dc-844c-4725-a602-5c3457129dbe");

		insight.IsFaulty.Should().BeFalse();

		var faultedEntry = insight.Occurrences.Single(v => v.IsFaulted);

		faultedEntry.Started.Should().BeAfter(new System.DateTimeOffset(new System.DateTime(2022, 07, 08)));
		faultedEntry.Ended.Should().BeBefore(new System.DateTimeOffset(new System.DateTime(2022, 07, 12)));
	}
}
