using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using WillowRules.Test.Bugs.Mocks;

namespace WillowRules.Test.Bugs;

[TestClass]
public class CalculatedPointTests
{
	[TestMethod]
	[Timeout(10000)]//in case it stalls exit after 10s
	public async Task CalcPointShouldNotStallExpansion()
	{
		var harness = new ProcessorTestHarness();

		var calculatedPoint = new TwinOverride("dtmi:com:willowinc:Sensor;1", "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION", "45733f70-7f07-4753-901a-0e8ebdce2a50", valueExpression: "(invalid");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");

		harness.OverrideCaches(calculatedPoint,
			new List<TwinOverride>()
			{
				sensor1
			});

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			Name = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			ValueExpression = "(invalid",
			TrendId = "45733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		var calculatedPoint2 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION1", "15733f70-7f07-4753-901a-0e8ebdce2a50", valueExpression: "1");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor2", "19463069-6db6-465d-b3e1-96969ac30c0a");

		harness.OverrideCaches(calculatedPoint2,
			new List<TwinOverride>()
			{
				sensor2
			});

		var calculatedExpression1 = new CalculatedPoint()
		{
			Id = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION1",
			Name = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION1",
			ValueExpression = "1",
			TrendId = "15733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpression1);

		var ruleInstances = await harness.GenerateRuleInstances();

		ruleInstances.Count.Should().Be(1);
	}

	[TestMethod]
	public async Task CalcPointMustBeUsedInExecution()
	{
		var harness = new ProcessorTestHarness();

		var calculatedPoint = new TwinOverride("dtmi:com:willowinc:Sensor;1", "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION", "45733f70-7f07-4753-901a-0e8ebdce2a50", valueExpression: "sensor1 + 1");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");
		var relatedTwin = new TwinOverride("dtmi:com:willowinc:AHU;1", "twin1");

		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(1)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("s1", "s1", "[MS-PS-B122-VSVAV.L3.91-SP-DEVIATION] + 1"),
			new RuleParameter("result", "result", "([s1] > 10)"),
		};

		var rule = new Rule()
		{
			Id = "rule1",
			PrimaryModelId = "dtmi:com:willowinc:AHU;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		await harness.AddRule(rule);

		harness.OverrideCaches(calculatedPoint,
			new List<TwinOverride>()
			{
				sensor1
			});

		harness.OverrideCaches(relatedTwin,
			new List<TwinOverride>()
			{
				calculatedPoint
			});

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			Name = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			ValueExpression = calculatedPoint.valueExpression,
			TrendId = "45733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		var ruleInstances = await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("CalculatedPoint", "CalcPointExecution_Timeseries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		var cpactor = actorsList.First(v => v.Id == "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION");
		var actor = actorsList.First(v => v.Id == "twin1_rule1");

		var cppoints = cpactor.TimedValues["result"].Points.ToList();

		var points = actor.TimedValues["s1"].Points.ToList();

		points.Count.Should().BeGreaterThan(0);

		points.Select(v => v.NumericValue).Should().BeEquivalentTo(cppoints.TakeLast(points.Count).Select(v => v.NumericValue + 1));
	}

	[TestMethod]
	public async Task CalcPointCanExecuteWithoutPoints()
	{
		var harness = new ProcessorTestHarness();

		var relatedTwin = new TwinOverride("dtmi:com:willowinc:AHU;1", "twin1");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(1)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("val", "val", "NOW.Second", CumulativeType.Accumulate),
			new RuleParameter("result", "result", "val"),
		};

		var rule = new Rule()
		{
			Id = "rule1",
			PrimaryModelId = "dtmi:com:willowinc:ZoneAirTemperatureSensor;1",
			RelatedModelId = "dtmi:com:willowinc:AHU;1",			
			TemplateId = RuleTemplateCalculatedPoint.ID,
			Parameters = parameters,
			Elements = elements
		};

		await harness.AddRule(rule);

		harness.OverrideCaches(relatedTwin, new List<TwinOverride>()
		{
			sensor1
		});

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("CalculatedPoint", "CalcPointExecution_Timeseries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		var actor = actorsList.First();

		var points = actor.TimedValues["result"].Points.ToList();
		var p = points.Last();

		points.Count.Should().BeGreaterThan(1);

		points.All(v => v.NumericValue > 0);
	}

	[TestMethod]
	public async Task CalcPointReferencingCalcPointMustBeUsedInExecution()
	{
		var harness = new ProcessorTestHarness();

		var calculatedPoint1 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "calcp_twin1", Guid.NewGuid().ToString(), valueExpression: "sensor1 + 1");
		var calculatedPoint2 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "calcp_twin2", Guid.NewGuid().ToString(), valueExpression: "calcp_twin1 + 1");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");
		var relatedTwin = new TwinOverride("dtmi:com:willowinc:AHU;1", "twin1");

		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(1)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("s1", "s1", "[calcp_twin2] + 1"),
			new RuleParameter("result", "result", "([s1] > 10)"),
		};

		var rule = new Rule()
		{
			Id = "rule1",
			PrimaryModelId = "dtmi:com:willowinc:AHU;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		await harness.AddRule(rule);

		await harness.AddTwinMapping(calculatedPoint1);

		harness.OverrideCaches(calculatedPoint1,
			new List<TwinOverride>()
			{
				sensor1
			});

		harness.OverrideCaches(calculatedPoint2,
			new List<TwinOverride>()
			{
			});

		await harness.AddTwinMapping(calculatedPoint2);

		harness.OverrideCaches(relatedTwin,
			new List<TwinOverride>()
			{
				calculatedPoint2
			});

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "calcp_twin1",
			Name = "calcp_twin1",
			ValueExpression = calculatedPoint1.valueExpression,
			TrendId = "calcp_twin1_trendid"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		calculatedExpression = new CalculatedPoint()
		{
			Id = "calcp_twin2",
			Name = "calcp_twin2",
			ValueExpression = calculatedPoint2.valueExpression,
			TrendId = "calcp_twin2_trendid"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		var ruleInstances = await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("CalculatedPoint", "CalcToCalcPointExecution_Timeseries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		var cpactor1 = actorsList.First(v => v.Id == "calcp_twin1");
		var cpactor2 = actorsList.First(v => v.Id == "calcp_twin2");
		var actor = actorsList.First(v => v.Id == "twin1_rule1");

		var cppoints1 = cpactor1.TimedValues["result"].Points.ToList();
		var cppoints2 = cpactor2.TimedValues["result"].Points.ToList();

		var points = actor.TimedValues["s1"].Points.ToList();

		points.Count.Should().BeGreaterThan(0);

		//the 2nd point only start calculating once it has 2 points so Skip(1) on first list
		cppoints2.Select(v => v.NumericValue).Should().BeEquivalentTo(cppoints1.Skip(1).Select(v => v.NumericValue + 1));
		points.Select(v => v.NumericValue).Take(12).Should().BeEquivalentTo(cppoints2.TakeLast(points.Count).Select(v => v.NumericValue + 1).Take(12));
	}

	[TestMethod]
	public async Task CalcPointBiReferencingCalcPointWontExecute()
	{
		var harness = new ProcessorTestHarness();

		var calculatedPoint1 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "calcp_twin1", Guid.NewGuid().ToString(), valueExpression: "sensor1 + calcp_twin2");
		var calculatedPoint2 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "calcp_twin2", Guid.NewGuid().ToString(), valueExpression: "calcp_twin1");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");

		await harness.AddTwinMapping(calculatedPoint1);

		harness.OverrideCaches(calculatedPoint1,
			new List<TwinOverride>()
			{
				sensor1
			});

		harness.OverrideCaches(calculatedPoint2,
			new List<TwinOverride>()
			{
			});

		await harness.AddTwinMapping(calculatedPoint2);

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "calcp_twin1",
			Name = "calcp_twin1",
			ValueExpression = "sensor1",
			TrendId = "calcp_twin1_trendid"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		calculatedExpression = new CalculatedPoint()
		{
			Id = "calcp_twin2",
			Name = "calcp_twin2",
			ValueExpression = "sensor2",
			TrendId = "calcp_twin2_trendid"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		var ruleInstances = await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("CalculatedPoint", "CalcToCalcPointExecution_Timeseries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		actorsList.Count.Should().Be(2);

		actorsList.All(v => v.IsValid).Should().BeFalse();
	}

	[TestMethod]
	public async Task Referring_To_Same_Point_Twice_Should_Work()
	{
		var harness = new ProcessorTestHarness();

		var calculatedPoint = new TwinOverride("dtmi:com:willowinc:Sensor;1", "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION", "45733f70-7f07-4753-901a-0e8ebdce2a50", valueExpression: "sensor1 + sensor1");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");

		harness.OverrideCaches(calculatedPoint,
			new List<TwinOverride>()
			{
				sensor1
			});

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			Name = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			ValueExpression = calculatedPoint.valueExpression,
			TrendId = "45733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		var ruleInstances = await harness.GenerateRuleInstances();

		var ruleInstance = ruleInstances.FirstOrDefault(v => v.Id == calculatedExpression.Id);

		var filePath = BugHelper.GetFullDataPath("CalculatedPoint", "Aggregation_Timeseries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single();

		var points = actor.TimedValues["result"].Points.ToList();

		points.Count.Should().BeGreaterThan(0);
		points.All(v => v.ValueDouble > 1).Should().BeTrue();
	}

	[TestMethod]
	public async Task ShouldNotBeNan()
	{
		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION", trendId: "45733f70-7f07-4753-901a-0e8ebdce2a50", valueExpression: "[MS-PS-B122-VSVAV.L03.91-ROOM-TEMP] - [MS-PS-B122-VSVAV.L03.91-CTL-STPT]");

		var sensors = new List<TwinOverride>()
		{
			new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "MS-PS-B122-VSVAV.L03.91-ROOM-TEMP", "91d3655f-49bb-46cb-9fba-a90a5fb872e1"),
			new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSetpoint;1", "MS-PS-B122-VSVAV.L03.91-CTL-STPT", "2ac7f411-1792-4f90-b8b5-d6c147055a59"),
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(equipment, sensors);

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			Name = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			ValueExpression = equipment.valueExpression,
			TrendId = "45733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("CalculatedPoint", "NAN_TimeSeries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single();

		var values = actor.TimedValues["result"].Points.ToList();

		values.Count.Should().BeGreaterThan(0);
		values.Any(v => v.ValueDouble > 0 || v.ValueDouble < 0).Should().BeTrue();
	}

	[TestMethod]
	public async Task ShouldNotSendTrueFalse()
	{
		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION", trendId: "45733f70-7f07-4753-901a-0e8ebdce2a50", valueExpression: "([MS-PS-B122-VSVAV.L03.91-ROOM-TEMP] = [MS-PS-B122-VSVAV.L03.91-CTL-STPT])");

		var sensors = new List<TwinOverride>()
		{
			new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "MS-PS-B122-VSVAV.L03.91-ROOM-TEMP", "91d3655f-49bb-46cb-9fba-a90a5fb872e1"),
			new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSetpoint;1", "MS-PS-B122-VSVAV.L03.91-CTL-STPT", "2ac7f411-1792-4f90-b8b5-d6c147055a59"),
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(equipment, sensors);

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			Name = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			ValueExpression = "([MS-PS-B122-VSVAV.L03.91-ROOM-TEMP] = [MS-PS-B122-VSVAV.L03.91-CTL-STPT])",
			TrendId = "45733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("CalculatedPoint", "TrueFalse_TimeSeries.csv");

		(_, _, _) = await harness.ExecuteRules(filePath);

		harness.eventHubService.Output.Count.Should().BeGreaterThan(0);
		harness.eventHubService.Output.All(v => (double)v.ScalarValue! == 1 || (double)v.ScalarValue! == 0).Should().Be(true);
	}

	[TestMethod]
	public async Task ShouldNotBeNan_Bug77649()
	{
		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION", trendId: "45733f70-7f07-4753-901a-0e8ebdce2a50", valueExpression: "[MS-PS-B122-VSVAV.L03.91-CTL-STPT]/[MS-PS-B122-VSVAV.L03.91-ROOM-TEMP]");

		var sensors = new List<TwinOverride>()
		{
			new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "MS-PS-B122-VSVAV.L03.91-ROOM-TEMP", "91d3655f-49bb-46cb-9fba-a90a5fb872e1"),
			new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSetpoint;1", "MS-PS-B122-VSVAV.L03.91-CTL-STPT", "2ac7f411-1792-4f90-b8b5-d6c147055a59"),
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(equipment, sensors);

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			Name = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			ValueExpression = equipment.valueExpression,
			TrendId = "45733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("CalculatedPoint", "NAN_TimeSeries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single();

		var test = actor.OutputValues.Points;

		test[1].Text.Should().StartWith("Bad value for result=NaN from");

		var values = actor.TimedValues["result"].Points.ToList();

		values.Count.Should().BeGreaterThan(0);
		values.Any(v => v.ValueDouble > 0 || v.ValueDouble < 0).Should().BeTrue();
	}

	[TestMethod]
	public async Task Calc_Point_Must_Work_With_MultipleTwins()
	{
		var harness = new ProcessorTestHarness();

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment", "");
		var calculatedPoint = new TwinOverride("dtmi:com:willowinc:Sensor;1", "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION", "45733f70-7f07-4753-901a-0e8ebdce2a50", valueExpression: "SUM([dtmi:com:willowinc:ZoneAirTemperatureSensor;1])");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor2", "a9463069-6db6-465d-b3e1-96969ac30c0a");

		harness.OverrideCaches(equipment,
			new List<TwinOverride>()
			{
				sensor1,
				sensor2,
				calculatedPoint
			});

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			Name = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			ValueExpression = "SUM([dtmi:com:willowinc:ZoneAirTemperatureSensor;1])",
			TrendId = "45733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		var ruleInstances = await harness.GenerateRuleInstances();

		var ruleInstance = ruleInstances.FirstOrDefault(v => v.Id == calculatedExpression.Id);

		ruleInstance.Should().NotBeNull();

		ruleInstance!.RuleParametersBound.Count.Should().Be(1);

		var boundParam = ruleInstance.RuleParametersBound[0];

		var sum = (TokenExpressionSum)boundParam.PointExpression;
		var array = (TokenExpressionArray)sum.Child;

		var arrayString = array.ToString();

		arrayString.Should().ContainAny("{sensor1,sensor2}", "{sensor2,sensor1}");

		var filePath = BugHelper.GetFullDataPath("CalculatedPoint", "Aggregation_Timeseries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single();

		var points = actor.TimedValues["result"].Points.ToList();

		points.Count.Should().BeGreaterThan(0);
		points.All(v => v.ValueDouble > 0).Should().BeTrue();
	}

	[TestMethod]
	public async Task Calc_Point_Must_Work_With_Graph_Query()
	{
		var harness = new ProcessorTestHarness();

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment", "");
		var calculatedPoint = new TwinOverride("dtmi:com:willowinc:Sensor;1", "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION", "45733f70-7f07-4753-901a-0e8ebdce2a50", valueExpression: "SUM([dtmi:com:willowinc:TerminalUnit;1].[dtmi:com:willowinc:ZoneAirTemperatureSensor;1])");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor2", "a9463069-6db6-465d-b3e1-96969ac30c0a");

		harness.OverrideCaches(equipment,
			new List<TwinOverride>()
			{
				sensor1,
				sensor2,
				calculatedPoint
			});

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			Name = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			ValueExpression = "SUM([dtmi:com:willowinc:TerminalUnit;1].[dtmi:com:willowinc:ZoneAirTemperatureSensor;1])",
			TrendId = "45733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		var ruleInstances = await harness.GenerateRuleInstances();

		var ruleInstance = ruleInstances.FirstOrDefault(v => v.Id == calculatedExpression.Id);

		ruleInstance.Should().NotBeNull();

		ruleInstance!.RuleParametersBound.Count.Should().Be(1);

		var boundParam = ruleInstance.RuleParametersBound[0];

		var sum = (TokenExpressionSum)boundParam.PointExpression;
		var array = (TokenExpressionArray)sum.Child;

		array.Children.Length.Should().Be(2);

		array.Children.Any(v => ((TokenExpressionVariableAccess)v).VariableName == "sensor1").Should().BeTrue();
		array.Children.Any(v => ((TokenExpressionVariableAccess)v).VariableName == "sensor2").Should().BeTrue();

		var filePath = BugHelper.GetFullDataPath("CalculatedPoint", "Aggregation_Timeseries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single();

		var points = actor.TimedValues["result"].Points.ToList();

		points.Count.Should().BeGreaterThan(0);
		points.All(v => v.ValueDouble > 0).Should().BeTrue();
	}

	[TestMethod]
	public async Task Calc_Point_Must_Work_With_Model()
	{
		var harness = new ProcessorTestHarness();

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment", "");
		var calculatedPoint = new TwinOverride("dtmi:com:willowinc:Sensor;1", "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION", "45733f70-7f07-4753-901a-0e8ebdce2a50", valueExpression: "[dtmi:com:willowinc:ZoneAirTemperatureSensor;1] + 1");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");

		harness.OverrideCaches(equipment,
			new List<TwinOverride>()
			{
				sensor1,
				calculatedPoint
			});

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			Name = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			ValueExpression = calculatedPoint.valueExpression,
			TrendId = "45733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		var ruleInstances = await harness.GenerateRuleInstances();

		var ruleInstance = ruleInstances.FirstOrDefault(v => v.Id == calculatedExpression.Id);

		ruleInstance.Should().NotBeNull();

		ruleInstance!.RuleParametersBound.Count.Should().Be(1);

		var boundParam = ruleInstance.RuleParametersBound[0];

		boundParam.PointExpression.ToString().Should().Be("(sensor1 + 1)");

		var filePath = BugHelper.GetFullDataPath("CalculatedPoint", "Aggregation_Timeseries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single();

		var points = actor.TimedValues["result"].Points.ToList();

		points.Count.Should().BeGreaterThan(0);
		points.All(v => v.ValueDouble > 1).Should().BeTrue();
	}

	[TestMethod]
	public async Task TestRuleInstancesCalcPointsMatch()
	{
		var harness = new ProcessorTestHarness();
		var equipment = new TwinOverride("dtmi:com:willowinc:AirGrille;1", "equipment", "");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:CO2AirQualitySensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");

		harness.OverrideCaches(equipment, new List<TwinOverride>() { sensor1 });

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Expression", "result", "50 > 0")
		};

		var rule = new Rule()
		{
			Id = "calcpointrule1",
			PrimaryModelId = "dtmi:com:willowinc:CO2AirQualitySensor;1",
			RelatedModelId = "dtmi:com:willowinc:AirGrille;1",
			TemplateId = RuleTemplateCalculatedPoint.ID,
			Parameters = parameters
		};

		await harness.AddRule(rule);

		var ruleInstances = await harness.GenerateRuleInstances();

		ruleInstances.Count.Should().BeGreaterThan(0);

		var cpPoints = harness.repositoryCalculatedPoint.Data;

		cpPoints.Count.Should().Be(ruleInstances.Count);

		foreach (var cpPoint in cpPoints)
		{
			cpPoint.ActionRequired.Should().Be(ADTActionRequired.Upsert);
			cpPoint.ActionStatus.Should().Be(ADTActionStatus.NoTwinExist);
		}

		var riUpdated = ruleInstances[0];
		riUpdated.Disabled = true;
		//await harness.repositoryRuleInstances.UpsertOne(riUpdated);

		_ = await harness.GenerateRuleInstances();

		cpPoints = harness.repositoryCalculatedPoint.Data;

		foreach (var cpPoint in cpPoints.Where(cp => cp.Id == riUpdated.Id))
		{
			//cpPoint.ActionRequired.Should().Be(ADTActionRequired.Delete);
			cpPoint.ActionStatus.Should().Be(ADTActionStatus.NoTwinExist);
		}
	}
}
