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
public class Bug115033Tests
{
	[TestMethod]
	public async Task COUNT_MustWorkDuringExpansion()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.1),
			Fields.OverHowManyHours.With(1),
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("building", "building", "[dtmi:com:willowinc:Building;1]", ""),
			new RuleParameter("count", "count", "COUNT_BINDINGS(building)", ""),
			new RuleParameter("result", "result", "[dtmi:com:willowinc:SomeSensor;1]", "")
		};

		var filters = new List<RuleParameter>()
		{ 
			new RuleParameter("count", "count", "COUNT_BINDINGS(building) > 0", ""),
		};

		var rule = new Rule()
		{
			Id = "floor-unoccupied-when-overtime-air-requested",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Filters = filters,
			Elements = elements,
			CommandEnabled = true
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment");
		var building = new TwinOverride("dtmi:com:willowinc:Building;1", "building1");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:SomeSensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.AddForwardEdge(equipment.twinId, new Edge()
		{
			RelationshipType = "isPartOf",
			Destination = new BasicDigitalTwinPoco()
			{
				Id = building.twinId,
				name = building.twinId,
				Metadata = new DigitalTwinMetadataPoco()
				{
					ModelId = building.modelId
				}
			}
		});

		var ris = await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug115033", "TimeSeries.csv");

		(var insights, var actors, _) = await harness.ExecuteRules(filePath);

		var ruleinstance = ris.Single();

		ruleinstance.RuleParametersBound[0].PointExpression.Serialize().Should().Be("[building1]");
		ruleinstance.RuleParametersBound[1].PointExpression.Serialize().Should().Be("1");

		//"building" should not be in the liust, becuase it does not inherit from capability
		ruleinstance.PointEntityIds.Count.Should().Be(1);
		ruleinstance.PointEntityIds[0].Id.Should().Be("sensor1");

		var actor = actors[0];

		//should be calculatin nans, so no buffer should exist
		actor.TimedValues.ContainsKey("building").Should().BeFalse();
	}

	[TestMethod]
	public async Task InstanceWitouhtPointsShouldExecute()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.1),
			Fields.OverHowManyHours.With(1),
		};

		var parameters1 = new List<RuleParameter>()
		{
			new RuleParameter("building", "building", "COUNT([dtmi:com:willowinc:Building;1])", ""),
			new RuleParameter("result", "result", "[building] = 1", "")
		};

		var rule1 = new Rule()
		{
			Id = "floor-unoccupied-when-overtime-air-requested1",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters1,
			Elements = elements,
			CommandEnabled = true
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment");
		var building = new TwinOverride("dtmi:com:willowinc:Building;1", "building1");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:SomeSensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule1, equipment, sensors);

		await harness.AddForwardEdge(equipment.twinId, new Edge()
		{
			RelationshipType = "isPartOf",
			Destination = new BasicDigitalTwinPoco()
			{
				Id = building.twinId,
				name = building.twinId,
				Metadata = new DigitalTwinMetadataPoco()
				{
					ModelId = building.modelId
				}
			}
		});

		var ris = await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug115033", "TimeSeries.csv");

		(var insights, var actors, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		actors.Count.Should().Be(1);

		var actor = actors.Find(v => v.Id == "equipment_floor-unoccupied-when-overtime-air-requested1");

		actor.Should().NotBeNull();

		actor!.TimedValues["result"].Points.Any().Should().BeTrue();
		actor.TimedValues["result"].Points.All(v => v.BoolValue == 1).Should().BeTrue();
	}
}
