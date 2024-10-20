using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item85435Tests
{
	[TestMethod]
	public async Task ShouldBeAccessOtherMacroInDifferentOrder()
	{
		var otherGlobal = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "othermacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("result", "result", "1 + 1")
			}
		};

		var global = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("result", "result", "othermacro + 2")
			}
		};

		//5 * 10
		//1000 + 10 + 10 + 5 * 10 + 10 + 10 + 5 * 10 + 1000 + 1000
		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(5),
			Fields.MaxTrigger.With(45),
			Fields.PercentageOfTime.With(0.08333333333333333),
			Fields.OverHowManyHours.With(12)
		};

		var parameters = new List<RuleParameter>()
		{
			 new RuleParameter("macro1", "macro1", "othermacro"),
			 new RuleParameter("macro", "macro", "mymacro"),
			 new RuleParameter("Zone Air Temperature Sensor", "result", "macro1 + macro")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-out-of-range-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		//put dependent one first in foreach
		harness.repositoryGlobalVariable.Data.Add(global);
		harness.repositoryGlobalVariable.Data.Add(otherGlobal);

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");
		var sensor = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor", trendId: Guid.NewGuid().ToString(), connectorId: "219bf492-48d2-4d5c-a1c6-73f7b599455c", externalId: "1220293AV101");

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>() { sensor });

		var ri = (await harness.GenerateRuleInstances(optimizeExpressions: false))[0];

		ri.RuleParametersBound[0].PointExpression.Serialize().Should().Be("1 + 1");
		ri.RuleParametersBound[1].PointExpression.Serialize().Should().Be("1 + 1 + 2");
		ri.RuleParametersBound[2].PointExpression.Serialize().Should().Be("macro1 + macro");

		ri = (await harness.GenerateRuleInstances(optimizeExpressions: true))[0];

		ri.RuleParametersBound[0].PointExpression.Serialize().Should().Be("2");
		ri.RuleParametersBound[1].PointExpression.Serialize().Should().Be("4");
		ri.RuleParametersBound[2].PointExpression.Serialize().Should().Be("6");
	}

	[TestMethod]
	public async Task CircularGlobalWontWork()
	{
		var otherGlobal = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "othermacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("result", "result", "mymacro + 1")
			}
		};

		var global = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("result", "result", "othermacro + 2")
			}
		};

		//5 * 10
		//1000 + 10 + 10 + 5 * 10 + 10 + 10 + 5 * 10 + 1000 + 1000
		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(5),
			Fields.MaxTrigger.With(45),
			Fields.PercentageOfTime.With(0.08333333333333333),
			Fields.OverHowManyHours.With(12)
		};

		var parameters = new List<RuleParameter>()
		{
			 new RuleParameter("macro1", "macro1", "othermacro"),
			 new RuleParameter("macro", "macro", "mymacro"),
			 new RuleParameter("Zone Air Temperature Sensor", "result", "macro1 + macro")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-out-of-range-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		harness.repositoryGlobalVariable.Data.Add(otherGlobal);
		harness.repositoryGlobalVariable.Data.Add(global);

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");
		var sensor = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor", trendId: Guid.NewGuid().ToString(), connectorId: "219bf492-48d2-4d5c-a1c6-73f7b599455c", externalId: "1220293AV101");

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>() { sensor });

		var ri = (await harness.GenerateRuleInstances())[0];

		ri.Status.Should().Be(RuleInstanceStatus.BindingFailed);
		ri.RuleParametersBound[0].PointExpression.Serialize().Should().Contain("Circular references are not allowed");
		ri.RuleParametersBound[1].PointExpression.Serialize().Should().Contain("Circular references are not allowed");
		ri.RuleParametersBound[2].PointExpression.Serialize().Should().Contain("macro1 + macro");
	}

	[TestMethod]
	public void MustUseMergeMultipleExpressionsAndReferenceOtherGlobal()
	{
		var otherGlobal = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "othermacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("e1", "e1", "10"),
				new RuleParameter("result", "result", "p2 + e1 + 10")
			},
			Parameters = new List<FunctionParameter>()
			{
				new FunctionParameter("p2", "", "")
			}
		};

		var global = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("e1", "e1", "1000"),
				new RuleParameter("result", "result", "othermacro(e1) + othermacro(p1) + p1 + e1 + 1000")
			},
			Parameters = new List<FunctionParameter>()
			{
				new FunctionParameter("p1", "", "")
			}
		};
		//5 * 10
		//1000 + 10 + 10 + 5 * 10 + 10 + 10 + 5 * 10 + 1000 + 1000
		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(5),
			Fields.MaxTrigger.With(45),
			Fields.PercentageOfTime.With(0.08333333333333333),
			Fields.OverHowManyHours.With(12)
		};

		var parameters = new List<RuleParameter>()
		{
			 new RuleParameter("sensor", "sensor", "[dtmi:com:willowinc:ZoneAirTemperatureSensor;1]"),
			 new RuleParameter("macro", "macro", "mymacro(5 * 10)"),
			 new RuleParameter("Zone Air Temperature Sensor", "result", "sensor + macro")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-out-of-range-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item85435", "Timeseries.csv");

		bugHelper.harness.repositoryGlobalVariable.Data.Add(otherGlobal);
		bugHelper.harness.repositoryGlobalVariable.Data.Add(global);

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");
		var sensor = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor", trendId: Guid.NewGuid().ToString(), connectorId: "219bf492-48d2-4d5c-a1c6-73f7b599455c", externalId: "1220293AV101");

		bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor }, optimizeExpressions: false);

		bugHelper.RuleInstance!.RuleParametersBound[1].PointExpression.Serialize().Should().Be("1000 + 10 + 10 + 5 * 10 + 10 + 10 + 5 * 10 + 1000 + 1000");
		bugHelper.RuleInstance!.RuleParametersBound[2].PointExpression.Serialize().Should().Be("sensor + macro");

		bugHelper.RuleInstance = null;

		bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor }, optimizeExpressions: true);

		bugHelper.RuleInstance!.RuleParametersBound[1].PointExpression.Serialize().Should().Be("3140");
		bugHelper.RuleInstance!.RuleParametersBound[2].PointExpression.Serialize().Should().Be("sensor + macro");

		bugHelper.Actor!.TimedValues["result"].Points.All(v => v.ValueDouble > (5 * 10 + 2000)).Should().BeTrue();
	}

	[TestMethod]
	public void MustUseMergeMultipleExpressions()
	{
		var global = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("e1", "e1", "1000"),
				new RuleParameter("result", "result", "p1 + e1 + 1000")
			},
			Parameters = new List<FunctionParameter>()
			{
				new FunctionParameter("p1", "", "")
			}
		};

		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(5),
			Fields.MaxTrigger.With(45),
			Fields.PercentageOfTime.With(0.08333333333333333),
			Fields.OverHowManyHours.With(12)
		};

		var parameters = new List<RuleParameter>()
		{
			 new RuleParameter("sensor", "sensor", "[dtmi:com:willowinc:ZoneAirTemperatureSensor;1]"),
			 new RuleParameter("macro", "macro", "mymacro(5 * 10)"),
			 new RuleParameter("Zone Air Temperature Sensor", "result", "sensor + macro")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-out-of-range-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item85435", "Timeseries.csv");

		bugHelper.harness.repositoryGlobalVariable.Data.Add(global);

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");
		var sensor = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor", trendId: Guid.NewGuid().ToString(), connectorId: "219bf492-48d2-4d5c-a1c6-73f7b599455c", externalId: "1220293AV101");

		bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor }, optimizeExpressions: false);

		bugHelper.RuleInstance!.RuleParametersBound[1].PointExpression.Serialize().Should().Be("5 * 10 + 1000 + 1000");
		bugHelper.RuleInstance!.RuleParametersBound[2].PointExpression.Serialize().Should().Be("sensor + macro");

		bugHelper.RuleInstance = null;

		bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor }, optimizeExpressions: true);

		bugHelper.RuleInstance!.RuleParametersBound[1].PointExpression.Serialize().Should().Be("2050");
		bugHelper.RuleInstance!.RuleParametersBound[2].PointExpression.Serialize().Should().Be("sensor + macro");

		bugHelper.Actor!.TimedValues["result"].Points.All(v => v.ValueDouble > (5 * 10 + 2000)).Should().BeTrue();
	}

	[TestMethod]
	public void MustUseGlobalMacroWithArgs()
	{
		var global = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("result", "result", "p1 + 1000")
			},
			Parameters = new List<FunctionParameter>()
			{
				new FunctionParameter("p1", "", "")
			}
		};

		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(5),
			Fields.MaxTrigger.With(45),
			Fields.PercentageOfTime.With(0.08333333333333333),
			Fields.OverHowManyHours.With(12)
		};

		var parameters = new List<RuleParameter>()
		{
			 new RuleParameter("sensor", "sensor", "[dtmi:com:willowinc:ZoneAirTemperatureSensor;1]"),
			 new RuleParameter("macro", "macro", "mymacro(5 * 10)"),
			 new RuleParameter("Zone Air Temperature Sensor", "result", "sensor + macro")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-out-of-range-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item85435", "Timeseries.csv");

		bugHelper.harness.repositoryGlobalVariable.Data.Add(global);

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");
		var sensor = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor", trendId: Guid.NewGuid().ToString(), connectorId: "219bf492-48d2-4d5c-a1c6-73f7b599455c", externalId: "1220293AV101");

		bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor }, optimizeExpressions: false);

		bugHelper.RuleInstance!.RuleParametersBound[1].PointExpression.Serialize().Should().Be("5 * 10 + 1000");
		bugHelper.RuleInstance!.RuleParametersBound[2].PointExpression.Serialize().Should().Be("sensor + macro");

		bugHelper.RuleInstance = null;

		bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor }, optimizeExpressions: true);

		bugHelper.RuleInstance!.RuleParametersBound[1].PointExpression.Serialize().Should().Be("1050");
		bugHelper.RuleInstance!.RuleParametersBound[2].PointExpression.Serialize().Should().Be("sensor + macro");

		bugHelper.Actor!.TimedValues["result"].Points.All(v => v.ValueDouble > (5 * 10 + 1000)).Should().BeTrue();
	}

	[TestMethod]
	public void MustUseGlobalMacroWithNoArgs()
	{
		var global = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("result", "result", "1000")
			},
		};

		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(5),
			Fields.MaxTrigger.With(45),
			Fields.PercentageOfTime.With(0.08333333333333333),
			Fields.OverHowManyHours.With(12)
		};

		var parameters = new List<RuleParameter>()
		{
			 new RuleParameter("sensor", "sensor", "[dtmi:com:willowinc:ZoneAirTemperatureSensor;1]"),
			 new RuleParameter("macro", "macro", "mymacro"),
			 new RuleParameter("Zone Air Temperature Sensor", "result", "sensor + macro")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-out-of-range-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item85435", "Timeseries.csv");

		bugHelper.harness.repositoryGlobalVariable.Data.Add(global);

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");
		var sensor = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor", trendId: Guid.NewGuid().ToString(), connectorId: "219bf492-48d2-4d5c-a1c6-73f7b599455c", externalId: "1220293AV101");

		bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor });

		bugHelper.RuleInstance!.RuleParametersBound[1].PointExpression.Serialize().Should().Be("1000");
		bugHelper.RuleInstance!.RuleParametersBound[2].PointExpression.Serialize().Should().Be("sensor + macro");

		bugHelper.Actor!.TimedValues["result"].Points.All(v => v.ValueDouble > (1000)).Should().BeTrue();
	}

	[TestMethod]
	public void MacroMustSubstituteModelWithTwin()
	{
		var global = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("result", "result", "[dtmi:com:willowinc:ZoneAirTemperatureSensor;1]")
			},
		};

		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(5),
			Fields.MaxTrigger.With(45),
			Fields.PercentageOfTime.With(0.08333333333333333),
			Fields.OverHowManyHours.With(12)
		};

		var parameters = new List<RuleParameter>()
		{
			 new RuleParameter("val", "val", "mymacro"),
			 new RuleParameter("Zone Air Temperature Sensor", "result", "val + 1000")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-out-of-range-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item85435", "Timeseries.csv");

		bugHelper.harness.repositoryGlobalVariable.Data.Add(global);

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");
		var sensor = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor", trendId: Guid.NewGuid().ToString(), connectorId: "219bf492-48d2-4d5c-a1c6-73f7b599455c", externalId: "1220293AV101");

		bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor });

		bugHelper.RuleInstance!.RuleParametersBound[0].PointExpression.Serialize().Should().Be("sensor");
		bugHelper.RuleInstance!.RuleParametersBound[1].PointExpression.Serialize().Should().Be("val + 1000");

		bugHelper.Actor!.TimedValues["result"].Points.All(v => v.ValueDouble >= (1000)).Should().BeTrue();
	}

	[TestMethod]
	public void MustWorkSubsittueVariableInTemporal()
	{
		var global = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("v1", "v1", "6"),
				new RuleParameter("result", "result", "MAX(p1, (v1)h)")
			},
			Parameters = new List<FunctionParameter>()
			{
				new FunctionParameter("p1", "", "")
			}
		};

		var harness = new ProcessorTestHarness();

		var env = Env.Empty.Push();

		env = harness.ruleService.AddGlobalsToEnv(env, new GlobalVariable[]
		{
			global
		});


		var func = (RegisteredFunction)env.GetBoundValue("mymacro")!.Value.Value;

		func.Body!.Serialize().Should().Be("MAX(p1, 6[h])");
	}

	[TestMethod]
	public void MustSubstituteArray()
	{
		var global = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("result", "result", "p1 + 1")
			},
			Parameters = new List<FunctionParameter>()
			{
				new FunctionParameter("p1", "", "")
			}
		};

		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(5),
			Fields.MaxTrigger.With(45),
			Fields.PercentageOfTime.With(0.08333333333333333),
			Fields.OverHowManyHours.With(12)
		};

		var parameters = new List<RuleParameter>()
		{
			 new RuleParameter("val", "val", "mymacro({1, 2})", "array"),
			 new RuleParameter("vals", "vals", "SUM(val) + [dtmi:com:willowinc:ZoneAirTemperatureSensor;1]"),
			 new RuleParameter("Zone Air Temperature Sensor", "result", "vals + 1000")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-out-of-range-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item85435", "Timeseries.csv");

		bugHelper.harness.repositoryGlobalVariable.Data.Add(global);

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");
		var sensor = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor", trendId: Guid.NewGuid().ToString(), connectorId: "219bf492-48d2-4d5c-a1c6-73f7b599455c", externalId: "1220293AV101");

		bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor });

		bugHelper.RuleInstance!.RuleParametersBound[0].PointExpression.Serialize().Should().Be("{2,3}");

		bugHelper.Actor!.TimedValues["result"].Points.All(v => v.ValueDouble >= (1000 + 5)).Should().BeTrue();
	}
}
