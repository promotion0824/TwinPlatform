using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item79033Tests
{
	[TestMethod]
	public void MustStayFaultedDueToHysteresis()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.PercentageOfTimeOff.With(0.2),
			Fields.OverHowManyHours.With(20)//1
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Damper Position", "DMPR_POS", "[dtmi:com:willowinc:DamperPositionSensor;1]"),
			new RuleParameter("AHU Status", "AHU_STS", "[dtmi:com:willowinc:RunSensor;1]}"),
			new RuleParameter("Expression", "result", "AHU_STS=0 & DMPR_POS > 10")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-is-on-while-ahu-is-off",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item79033", "Timeseries.csv");

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:DamperPositionSensor;1", "sensor1", trendId: "13804b98-c269-48d5-aebf-e69224007195");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:RunSensor;1", "sensor2", trendId: "573437a4-e732-42e2-bc15-89d2cd3492f9");

		var insight = bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor1, sensor2 }, assertSimulation: false);

		var actor = bugHelper.Actor!;

		var faulted = actor.OutputValues.Points.Find(v => v.Faulted);

		faulted.Should().NotBeNull();
	}
}
