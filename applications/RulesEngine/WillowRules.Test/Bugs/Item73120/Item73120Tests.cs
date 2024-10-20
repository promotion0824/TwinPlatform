using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item73120Tests
{
	[TestMethod]
	public void Item73120_TestRerun()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.Hours.With(24)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Discharge Air Flow", "discharge_air_flow", "OPTION([dtmi:com:willowinc:DischargeAirFlowSensor;1])"),
			new RuleParameter("Discharge Air Flow is stuck", "result", "[discharge_air_flow]"),
			//leave these in for now as the template accesses the last field but it should'nt use any of these
			new RuleParameter("Cost impact", "cost_impact", "0.5 * TIME"),
			new RuleParameter("Comfort impact", "comfort_impact", "0.0"),
			new RuleParameter("Reliability impact", "reliability_impact", "1.0 * TIME"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-stuck-discharge-air-flow",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateUnchanging.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item73120", "Timeseries.csv", true);

		var insight1 = bugHelper.GenerateInsightForPoint(
			rule,
			"dtmi:com:willowinc:DischargeAirFlowSensor;1",
			"93560ff9-9a80-4afc-a00d-f5055042c048",
			new DateTime(2022, 11, 15),
			testcaseName: "TestRerun_Run1");

		insight1.Should().NotBeNull();

		insight1!.HasOverlappingOccurrences().Should().BeFalse();

		var insight2 = bugHelper.GenerateInsightForPoint(
			rule,
			"dtmi:com:willowinc:DischargeAirFlowSensor;1",
			"93560ff9-9a80-4afc-a00d-f5055042c048",
			new DateTime(2022, 11, 08),
			testcaseName: "TestRerun_Run2");

		insight2!.HasOverlappingOccurrences().Should().BeFalse();

		var insight3 = bugHelper.GenerateInsightForPoint(
			rule,
			"dtmi:com:willowinc:DischargeAirFlowSensor;1",
			"93560ff9-9a80-4afc-a00d-f5055042c048",
			new DateTime(2022, 11, 13),
			testcaseName: "TestRerun_Run3");

		insight3!.HasOverlappingOccurrences().Should().BeFalse();
	}
}
