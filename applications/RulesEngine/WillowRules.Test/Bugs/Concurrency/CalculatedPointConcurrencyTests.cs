using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class CalculatedPointConcurrencyTests
{
	[TestMethod]
	public async Task ShouldNotGetRaceConditions()
	{
		/*
		 * Collection modified unit test. 
		 * The Rule Instance's actor is being modified on the wrong queue. 
		 * It is being modified on the queue of the Calc point's incoming sensor. 
		 * It may only be modified on its own incoming points' queue
		 */

		var harness = new ProcessorTestHarness();

		var inputSensorForCalcPoint = new TwinOverride("dtmi:com:willowinc:Sensor;1", "HP-SOFI-ST-L02-VAV2-7-8-OccupancyCmd-69MV3001461", "c140fcc3-b00b-49d9-88d6-2bdb9ac12ec1");

		var calculatedPoint = new TwinOverride("dtmi:com:willowinc:Sensor;1", "HP-SOFI-ST-L02-VAV2-7-8-OCC-STATE", "e1ac403e-88f6-44ba-b8a5-268bbc5e5d60", valueExpression: "[HP-SOFI-ST-L02-VAV2-7-8-OccupancyCmd-69MV3001461]");

		var otherSensorForRule1 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "HP-SOFI-ST-L02-VAV2-7-8-DischargeAirDamperSensor-69AO3001453", "c66efe9c-8c31-4bc9-bb86-23b4b1d391c4", externalId: "69AO3001453");
		var otherSensorForRule2 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "HP-SOFI-ST-L02-VAV2-7-8-ZoneAirTempSensor-69AI3001449", "07df841f-cfb6-4f13-87dc-fafc435cf7cf", externalId: "69AI3001449");
		var otherSensorForRule3 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "HP-SOFI-ST-L02-VAV2-7-8-ZoneAirTempSp-69AV3001464", "b8bde293-f879-4d8b-a0c2-04060b726cec", externalId: "69AV3001464");

		var ruleEquipment = new TwinOverride("dtmi:com:willowinc:AHU;1", "twin1");

		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTime.With(1)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("s1", "s1", "[HP-SOFI-ST-L02-VAV2-7-8-OCC-STATE]")
			{
				CumulativeSetting = CumulativeType.Accumulate
			},
			new RuleParameter("s2", "s2", "[HP-SOFI-ST-L02-VAV2-7-8-DischargeAirDamperSensor-69AO3001453]"),
			new RuleParameter("s3", "s3", "[HP-SOFI-ST-L02-VAV2-7-8-ZoneAirTempSensor-69AI3001449]"),
			new RuleParameter("s4", "s4", "[HP-SOFI-ST-L02-VAV2-7-8-ZoneAirTempSp-69AV3001464]"),
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
				inputSensorForCalcPoint
			});

		harness.OverrideCaches(ruleEquipment,
			new List<TwinOverride>()
			{
				calculatedPoint,
				otherSensorForRule1,
				otherSensorForRule2,
				otherSensorForRule3
			});

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "HP-SOFI-ST-L02-VAV2-7-8-OCC-STATE",
			Name = "HP-SOFI-ST-L02-VAV2-7-8-OCC-STATE",
			ValueExpression = "([HP-SOFI-ST-L02-VAV2-7-8-OccupancyCmd-69MV3001461] = 1)",
			TrendId = "e1ac403e-88f6-44ba-b8a5-268bbc5e5d60"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Concurrency", "CalculatedPointConcurrency.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		
		actorsList.Count.Should().Be(2);

		//2023-08-02T11:16:02
		var ruleActor = actorsList.First(v => v.Id == "twin1_rule1");

		foreach (var result in ruleActor.TimedValues.Values)
		{
			result.GetLastSeen().UtcDateTime.Should().BeAfter(new DateTime(2023, 8, 2, 0, 0, 00));
		}
	}

	[TestMethod]
	public async Task ShouldNotGetRaceConditionsForSinglePoint()
	{
		/*
		 * Collection modified unit test. 
		 * The Rule Instance's actor is being modified on the wrong queue. 
		 * It is being modified on the queue of the Calc point's incoming sensor. 
		 * It may only be modified on its own incoming points' queue
		 */

		var harness = new ProcessorTestHarness();

		var inputSensorForCalcPoint = new TwinOverride("dtmi:com:willowinc:Sensor;1", "HP-SOFI-ST-L02-VAV2-7-8-OccupancyCmd-69MV3001461", "c140fcc3-b00b-49d9-88d6-2bdb9ac12ec1");

		var calculatedPoint = new TwinOverride("dtmi:com:willowinc:Sensor;1", "HP-SOFI-ST-L02-VAV2-7-8-OCC-STATE", "e1ac403e-88f6-44ba-b8a5-268bbc5e5d60", valueExpression: "[HP-SOFI-ST-L02-VAV2-7-8-OccupancyCmd-69MV3001461]");

		var otherSensorForRule1 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "HP-SOFI-ST-L02-VAV2-7-8-DischargeAirDamperSensor-69AO3001453", "c66efe9c-8c31-4bc9-bb86-23b4b1d391c4", externalId: "69AO3001453");
		var otherSensorForRule2 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "HP-SOFI-ST-L02-VAV2-7-8-ZoneAirTempSensor-69AI3001449", "07df841f-cfb6-4f13-87dc-fafc435cf7cf", externalId: "69AI3001449");
		var otherSensorForRule3 = new TwinOverride("dtmi:com:willowinc:Sensor;1", "HP-SOFI-ST-L02-VAV2-7-8-ZoneAirTempSp-69AV3001464", "b8bde293-f879-4d8b-a0c2-04060b726cec", externalId: "69AV3001464");

		var ruleEquipment = new TwinOverride("dtmi:com:willowinc:AHU;1", "twin1");

		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTime.With(1)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("s1", "s1", "[HP-SOFI-ST-L02-VAV2-7-8-OCC-STATE]")
			{
				CumulativeSetting = CumulativeType.Accumulate
			},
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
				inputSensorForCalcPoint
			});

		harness.OverrideCaches(ruleEquipment,
			new List<TwinOverride>()
			{
				calculatedPoint,
				otherSensorForRule1,
				otherSensorForRule2,
				otherSensorForRule3
			});

		var calculatedExpression = new CalculatedPoint()
		{
			Id = "HP-SOFI-ST-L02-VAV2-7-8-OCC-STATE",
			Name = "HP-SOFI-ST-L02-VAV2-7-8-OCC-STATE",
			ValueExpression = "([HP-SOFI-ST-L02-VAV2-7-8-OccupancyCmd-69MV3001461] = 1)",
			TrendId = "e1ac403e-88f6-44ba-b8a5-268bbc5e5d60"
		};

		await harness.AddCalculatedPoint(calculatedExpression);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Concurrency", "CalculatedPointConcurrency.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath, assertSimulation: false);


		actorsList.Count.Should().Be(2);

		//2023-08-02T11:16:02
		var ruleActor = actorsList.First(v => v.Id == "twin1_rule1");

		foreach (var result in ruleActor.TimedValues.Values)
		{
			result.GetLastSeen().UtcDateTime.Should().BeAfter(new DateTime(2023, 8, 2, 0, 0, 00));
		}
	}
}
