using FluentAssertions;
using Kusto.Cloud.Platform.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug125430Tests
{
	[TestMethod]
	public async Task Bug_125430_MacroSimulationMustGenerateTemporal()
	{
		var global = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro",
			Parameters = new List<FunctionParameter>()
			{
				new FunctionParameter("TerminalUnit", "test", "")
			},
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("IsHeating", "IsHeating", "[dtmi:com:willowinc:Sensor;1]"),
				new RuleParameter("result", "result", "MAX(sensor1 > 1, 1h)")
			}
		};


		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");

		var sensor1 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(equipment, sensors);

		var simulationService = harness.CreateRuleSimulationService(BugHelper.GetFullDataPath("Bug125430", "Timeseries.csv"));

		var rule = new Rule()
		{
			Id = "test",
			Name = "test",
			TemplateId = RuleTemplateCalculatedPoint.ID,
			Parameters = new List<RuleParameter>()
			{
				new RuleParameter("result", "result", "mymacro(this)")
			}
		};

		(_, var ruleInstance, var actor, _, _, _) = await simulationService.ExecuteRule(
			rule,
			equipment.twinId,
			DateTime.Parse("2022-08-24T12:26:45.3460834Z").ToUtc(),
			DateTime.Parse("2022-08-24T15:34:45.4481877Z").ToUtc(),
			global: global,
			enableCompression: false);

		ruleInstance.PointEntityIds.Count.Should().Be(1);
		ruleInstance.PointEntityIds[0].Id.Should().Be("sensor1");

		actor.Should().NotBeNull();
		actor.TimedValues.Should().NotBeNull();
		actor.TimedValues["result"].Should().NotBeNull();
		actor.TimedValues["result"].Points.Should().NotBeNull();

		var results = actor.TimedValues["result"].Points.ToList();

		results.Count.Should().BeGreaterThan(0);

		results.Any(v => v.NumericValue == 1);
		results.Any(v => v.NumericValue == 0);
		results.All(v => v.NumericValue == 0 || v.NumericValue == 1);
	}
}
