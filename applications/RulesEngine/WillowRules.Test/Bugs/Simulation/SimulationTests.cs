using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using RulesEngine.Web;

namespace WillowRules.Test.Bugs;

[TestClass]
public class SimulationTests
{
	[TestMethod]
	public async Task SimulationMustUseRuleTimeZone()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.PercentageOfTimeOff.With(0.2),
			Fields.OverHowManyHours.With(20)//1
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Damper Position", "DMPR_POS", "[dtmi:com:willowinc:DamperPositionSensor;1] + 0"),
			new RuleParameter("the_hour", "the_hour", "HOUR(NOW)"),
			new RuleParameter("converted", "converted", "HOUR(NOW.Ticks)"),
			new RuleParameter("Expression", "result", "the_hour>12")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-is-on-while-ahu-is-off",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};


		var bugHelper = new BugHelper("Simulation", "Timeseries.csv");

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment", timeZone: "America/Chicago");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:DamperPositionSensor;1", "sensor1", trendId: "13804b98-c269-48d5-aebf-e69224007195");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:RunSensor;1", "sensor2", trendId: "573437a4-e732-42e2-bc15-89d2cd3492f9");

		bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor1, sensor2 });

		var actor = bugHelper.Actor!;
		var starTimeUTC = DateTime.SpecifyKind(new DateTime(2023, 05, 09), DateTimeKind.Utc);
		var endTimeUTC = DateTime.SpecifyKind(new DateTime(2023, 05, 10), DateTimeKind.Utc);

		var simuilationService = bugHelper.harness.CreateRuleSimulationService(BugHelper.GetFullDataPath("Simulation", "Timeseries.csv"));

		var result = await simuilationService.ExecuteRule(rule, equipment.twinId, starTimeUTC, endTimeUTC);

		//simulation must query based on timezone and not utc
		result.actor.TimedValues.All(v => v.Value.Points.All(v => v.Timestamp.Month >= 5 && v.Timestamp.Day >= 9)).Should().BeTrue();

		var tsResult = result.ruleInstance.GetTimeseriesDataForRuleInstance(result.actor, starTimeUTC, endTimeUTC);

		tsResult.Trendlines.All(v => v.Data.All(v => v.Timestamp.Month >= 5 && v.Timestamp.Day >= 9)).Should().BeTrue();
		tsResult.Trendlines.All(v => v.Data.All(v => v.Timestamp.Month >= 5 && v.Timestamp.Day <= 10)).Should().BeTrue();
	}
}
