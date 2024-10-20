using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using System;
using System.Threading.Tasks;
using System.Security.Claims;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item76777Tests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
	private Rule rule1;
	private Rule rule2;
	private TwinOverride equipment;
	private TwinOverride sensor;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

	[TestInitialize]
	public void TestSetup()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTime.With(0.9)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Chilled Water Valve", "CHWV", "OPTION([dtmi:com:willowinc:ChilledWaterValvePositionActuator;1],[dtmi:com:willowinc:ChilledWaterValvePositionSensor;1])"),
			new RuleParameter("CHWV1", "CHWV1", "MAX(OPTION([dtmi:com:willowinc:ChilledWaterValvePositionActuator;1],[dtmi:com:willowinc:ChilledWaterValvePositionSensor;1]), 3d)"),
			new RuleParameter("AHU CHWV Over Design Operation", "result", "[CHWV] > 90"),
		};

		rule1 = new Rule()
		{
			Id = "ahu-chwv-over-design-operation-r1",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		rule2 = new Rule()
		{
			Id = "ahu-chwv-over-design-operation-r2",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		equipment = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment");
		sensor = new TwinOverride("dtmi:com:willowinc:ChilledWaterValvePositionActuator;1", "sensor1", "133f9221-477f-46e3-b3ed-312eac94dfe1");
	}

	[TestMethod]
	public async Task OnlySingleRuleShouldExecute()
	{
		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule1, equipment, sensor);
		harness.OverrideCaches(rule2, equipment, sensor);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("SingleRuleExecution", "Timeseries.csv");

		(_, var actors, _) = await harness.ExecuteRules(filePath, ruleId: rule1.Id, assertSimulation: false);

		actors.Count.Should().Be(1);
		actors.First().IsValid.Should().BeTrue();
		actors.First().RuleId.Should().Be(rule1.Id);
	}

	[TestMethod]
	public async Task ActorShouldStillBeValidAfterFirstRealtime()
	{
		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule1, equipment, sensor);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("SingleRuleExecution", "Timeseries.csv");

		(_, var actors, _) = await harness.ExecuteRules(filePath,
			startDate: new DateTime(2023, 1, 23),
			endDate: new DateTime(2023, 2, 2),
			ruleId: rule1.Id,
			assertSimulation: false);

		actors.Count.Should().Be(1);
		actors.First().IsValid.Should().BeTrue();
		actors.First().RuleId.Should().Be(rule1.Id);

		//CHWV1 is a temporal, make sure its buffer is still in tact
		(_, actors, _) = await harness.ExecuteRules(filePath,
			startDate: new DateTime(2023, 2, 2),
			endDate: new DateTime(2023, 2, 3),
			ruleId: rule1.Id,
			assertSimulation: false);

		actors.Count.Should().Be(1);
		actors.First().IsValid.Should().BeTrue();
		actors.First().RuleId.Should().Be(rule1.Id);
	}

	[TestMethod]
	public async Task CalcPointAndOnlySingleRuleShouldExecute()
	{
		var harness = new ProcessorTestHarness();

		var calculatedPoint = new TwinOverride(sensor.modelId, "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION", trendId: "1ed922c2-741d-4e18-816a-175a329bed6c", valueExpression: "[calcsensor]");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "calcsensor", sensor.trendId);

		harness.OverrideCaches(calculatedPoint,
			new List<TwinOverride>()
			{
				sensor1
			});

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			Name = "MS-PS-B122-VSVAV.L3.91-SP-DEVIATION",
			ValueExpression = "[sensor1]",
			TrendId = "45733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		//this one should be ignored
		var calculatedPointOther = new TwinOverride(sensor.modelId, "WIL--SP-DEVIATION", trendId: "50158491-a62d-4b45-98be-f505b7181f0d", valueExpression: "[calcsensorother]");
		var sensorOther = new TwinOverride("dtmi:com:willowinc:OtherSensor;1", "calcsensorother", "567f9221-477f-46e3-b3ed-312eac94dfe1");

		harness.OverrideCaches(calculatedPointOther,
			new List<TwinOverride>()
			{
				sensorOther
			});

		var calculatedExpressionOther = new CalculatedPoint()
		{
			Id = "WIL--SP-DEVIATION",
			Name = "WIL--SP-DEVIATION",
			ValueExpression = "[calcsensorother]",
			TrendId = "45733f70-7f07-4753-901a-0e8ebdce2a50"
		};

		await harness.AddCalculatedPoint(calculatedExpressionOther);

		harness.OverrideCaches(rule1, equipment, calculatedPoint);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("SingleRuleExecution", "Timeseries.csv");

		(_, var actors, _) = await harness.ExecuteRules(filePath, ruleId: rule1.Id, assertSimulation: false);

		actors.Count.Should().Be(2);

		actors.Count(v => v.RuleId == "1ed922c2-741d-4e18-816a-175a329bed6c").Should().Be(1);
		actors.Count(v => v.RuleId == rule1.Id).Should().Be(1);
		actors.First(v => v.RuleId == rule1.Id).TimedValues.Any().Should().BeTrue();

		actors.First(v => v.RuleId == rule1.Id).TimedValues.Values.All(v1 => v1.Points.Any()).Should().BeTrue();

	}

	[TestMethod]
	public async Task ChainOfCalcPointsMustWork()
	{
		var harness = new ProcessorTestHarness();

		var calculatedPoint = new TwinOverride("dtmi:com:willowinc:SetPoint;1", "calctwin1", trendId: "1ed922c2-741d-4e18-816a-175a329bed6c", valueExpression: "[calcsensor]");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "calcsensor", sensor.trendId);

		var calculatedPoint2 = new TwinOverride(sensor.modelId, "calctwin2", trendId: "2ed922c2-741d-4e18-816a-175a329bed6c", valueExpression: "[calctwin1]");

		harness.OverrideCaches(calculatedPoint,
			new List<TwinOverride>()
			{
				sensor1
			});

		var calculatedExpression = new CalculatedPoint()
		{
			Id = calculatedPoint.twinId,
			Name = calculatedPoint.twinId,
			ValueExpression = calculatedPoint.valueExpression,
			TrendId = calculatedPoint.trendId
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		harness.OverrideCaches(calculatedPoint2,
			new List<TwinOverride>()
			{
				calculatedPoint
			});

		var calculatedExpression2 = new CalculatedPoint()
		{
			Id = calculatedPoint2.twinId,
			Name = calculatedPoint2.twinId,
			ValueExpression = calculatedPoint2.valueExpression,
			TrendId = calculatedPoint2.trendId
		};

		await harness.AddCalculatedPoint(calculatedExpression2);

		harness.OverrideCaches(rule1, equipment, calculatedPoint2);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("SingleRuleExecution", "Timeseries.csv");

		(_, var actors, _) = await harness.ExecuteRules(filePath, ruleId: rule1.Id, assertSimulation: false);

		actors.Count.Should().Be(3);

		actors.Count(v => v.RuleId == "1ed922c2-741d-4e18-816a-175a329bed6c").Should().Be(1);
		actors.First(v => v.RuleId == "1ed922c2-741d-4e18-816a-175a329bed6c").TimedValues.Values.All(v1 => v1.Points.Any()).Should().BeTrue();
		actors.Count(v => v.RuleId == "2ed922c2-741d-4e18-816a-175a329bed6c").Should().Be(1);
		actors.First(v => v.RuleId == "2ed922c2-741d-4e18-816a-175a329bed6c").TimedValues.Values.All(v1 => v1.Points.Any()).Should().BeTrue();
		actors.Count(v => v.RuleId == rule1.Id).Should().Be(1);
		actors.First(v => v.RuleId == rule1.Id).TimedValues.Any().Should().BeTrue();
		actors.First(v => v.RuleId == rule1.Id).TimedValues.Values.All(v1 => v1.Points.Any()).Should().BeTrue();
	}

	[TestMethod]
	public async Task AllRulesShouldExecute()
	{
		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule1, equipment, sensor);
		harness.OverrideCaches(rule2, equipment, sensor);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("SingleRuleExecution", "Timeseries.csv");

		(_, var actors, _) = await harness.ExecuteRules(filePath);

		actors.Count.Should().Be(2);
	}
}
