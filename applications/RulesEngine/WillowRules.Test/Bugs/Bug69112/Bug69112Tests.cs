using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug69112Tests
{
	[TestMethod]
	public void Bug69112_Test()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.MinTrigger.With(12),
			Fields.MaxTrigger.With(35),
			Fields.PercentageOfTime.With(0.08333333333333333)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Zone Temperature Setpoint", "result", "OPTION([dtmi:com:willowinc:EffectiveZoneAirTemperatureSetpoint;1],[dtmi:com:willowinc:ZoneAirTemperatureSetpoint;1])"),
			new RuleParameter("Cost impact", "cost_impact", "0.5 * TIME"),
			new RuleParameter("Comfort impact", "comfort_impact", "1.0 * TOTAL * TOTAL"),
			new RuleParameter("Reliability impact", "reliability_impact", "1.0 * TIME"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-setpoint-out-of-range",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Bug69112", "Timeseries.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:EffectiveZoneAirTemperatureSetpoint;1", "a10df277-c937-4420-9d20-9c39aa15c5cc");

		insight.Should().NotBeNull();

		insight!.Occurrences.Count(v => v.IsFaulted).Should().BeGreaterThan(0);
	}
}
