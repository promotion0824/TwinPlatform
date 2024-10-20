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
public class Bug77954Tests
{
	[TestMethod]
	public async Task Bug_77954_24_ShouldWriteTwinWithNoDtId()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(50.0),
			Fields.OverHowManyHours.With(1),
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Occupancy", "Occupancy", "[dtmi:com:willowinc:PeopleCountSensor;1]"),
			new RuleParameter("Is on Overtime Schedule", "OvertimeSched", "[dtmi:com:willowinc:OvertimeScheduleOccupiedState;1]", ""),
			new RuleParameter("Expression", "result", "[OvertimeSched] & ([Occupancy] <= 1)", "")
		};

		var rule = new Rule()
		{
			Id = "floor-unoccupied-when-overtime-air-requested",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:PeopleCountSensor;1", "sensor1", trendId: "c3d65fac-6444-47dd-940c-a0346f633e29", connectorId: "fd6f026f-0b6d-4026-a7ad-fe11e96d6563", externalId: "BPY-1MW-FACIT-Floor_27-Occupancy");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:OvertimeScheduleOccupiedState;1", "sensor2", trendId: "8c77180c-5e5d-4a5c-b2ff-dd537e6063bc");

		var sensors = new List<TwinOverride>()
		{
			sensor1,
			sensor2
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug77954", "Timeseries.csv");

		//var timeSeriesManagerMock = new TimeSeriesManagerMock77954(harness.repositoryTimeSeriesBuffer, harness.repositoryTimeSeriesMapping, sensor1);

		var sensorMapping = harness.repositoryTimeSeriesMapping.Data.First(v => v.Id == sensor1.twinId);
		harness.repositoryTimeSeriesMapping.Data.Remove(sensorMapping);

		(var insights, var actors, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		var timeseries = harness.repositoryTimeSeriesBuffer.Data.Find(v => v.Id == $"{sensor1.externalId}_{sensor1.connectorId}");

		var actor = actors.Single();
		var insight = insights.Single();

		timeseries.Should().NotBeNull();

		var missingText = "Missing value: [sensor1] never (1)";

		timeseries!.DtId.Should().BeNull();
		var outputValues = actor.OutputValues;

		//twin id is now mapped, but actor still didn't find sensor by twin id, but the next realtime run finds it
		outputValues.Points.Count.Should().Be(1);
        outputValues.Points[0].Text.Should().Contain(missingText);

		//once a cache refresh occurred, the mapping is now there and should update twinid on timeseries manager load
		harness.repositoryTimeSeriesMapping.Data.Add(sensorMapping!);

		(insights, actors, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		timeseries!.DtId.Should().Be("sensor1");

		actor = actors.Single();

		actor.OutputValues.Points.Count.Should().Be(5);
		actor.OutputValues.Points.Last().IsValid.Should().BeTrue();
		actor.OutputValues.Points.Any(v => v.Text.Contains(missingText)).Should().BeFalse();
	}

	[TestMethod]
	public async Task Bug_85177_ShoulRemoveTiumeSeriesForDeletedTwin()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(50.0),
			Fields.OverHowManyHours.With(1),
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Occupancy", "Occupancy", "[dtmi:com:willowinc:PeopleCountSensor;1]"),
			new RuleParameter("Expression", "result", "[Occupancy] <= 1", "")
		};

		var rule = new Rule()
		{
			Id = "floor-unoccupied-when-overtime-air-requested",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:PeopleCountSensor;1", "sensor1", trendId: "c3d65fac-6444-47dd-940c-a0346f633e29", connectorId: "fd6f026f-0b6d-4026-a7ad-fe11e96d6563", externalId: "BPY-1MW-FACIT-Floor_27-Occupancy");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:OvertimeScheduleOccupiedState;1", "sensor2", trendId: "8c77180c-5e5d-4a5c-b2ff-dd537e6063bc");

		var sensors = new List<TwinOverride>()
		{
			sensor1,
			sensor2
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug77954", "Timeseries.csv");

		(var insights, var actors, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		var timeseries = harness.repositoryTimeSeriesBuffer.Data.Find(v => v.Id == "BPY-1MW-FACIT-Floor_27-Occupancy_fd6f026f-0b6d-4026-a7ad-fe11e96d6563");

		harness.repositoryTimeSeriesBuffer.Data.Count.Should().Be(2);

		timeseries.Should().NotBeNull();
		timeseries!.DtId.Should().Be("sensor1");
		var sensorMapping = harness.repositoryTimeSeriesMapping.Data.First(v => v.Id == sensor1.twinId);
		harness.repositoryTimeSeriesMapping.Data.Remove(sensorMapping);

		//go forward startdate wise otherwise it'll just re-add the timeseries, but with no twin linked
		(insights, actors, _) = await harness.ExecuteRules(filePath, assertSimulation: false, startDate: DateTime.Now);

		//here's stays deleted
		timeseries = harness.repositoryTimeSeriesBuffer.Data.Find(v => v.Id == "BPY-1MW-FACIT-Floor_27-Occupancy_fd6f026f-0b6d-4026-a7ad-fe11e96d6563");

		timeseries.Should().BeNull();

		harness.repositoryTimeSeriesBuffer.Data.Count.Should().Be(1);

		(insights, actors, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		//here it gets re-added but orhpaned
		timeseries = harness.repositoryTimeSeriesBuffer.Data.Find(v => v.Id == "BPY-1MW-FACIT-Floor_27-Occupancy_fd6f026f-0b6d-4026-a7ad-fe11e96d6563");

		timeseries.Should().NotBeNull();
		//it has been re-added but not linked to a twin anymore
		timeseries!.DtId.Should().BeNull();

		harness.repositoryTimeSeriesBuffer.Data.Count.Should().Be(2);
	}
}
