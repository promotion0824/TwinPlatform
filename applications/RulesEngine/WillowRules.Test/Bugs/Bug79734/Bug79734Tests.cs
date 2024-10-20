using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug79734Tests
{
	[TestMethod]
	public async Task Bug_79734_ShouldWriteTwinWithUpdatedDtIdButSameTrendId()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(50.0),
			Fields.OverHowManyHours.With(1),
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Occupancy", "Occupancy", "[dtmi:com:willowinc:PeopleCountSensor;1]"),
			new RuleParameter("Occupancy1", "Occupancy1", "[dtmi:com:willowinc:PeopleCountSensor;2]"),
			new RuleParameter("Expression", "result", "[Occupancy] > 0 AND [Occupancy1] > 0", "")
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
		var sensor1 = new TwinOverride("dtmi:com:willowinc:PeopleCountSensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:PeopleCountSensor;2", "sensor2", trendId: "a9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1,
			sensor2
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug79734", "Timeseries.csv");

		(var insights, var actors, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		var actor = actors.Single();
		var insight = insights.Single();

		actor.OutputValues.Points.Any(v => v.IsValid).Should().BeTrue();

		harness.repositoryActorState.Data.Clear();

		//now mimick a cache update with a new twinid for the sensor for the same trendid
		sensor1 = new TwinOverride("dtmi:com:willowinc:PeopleCountSensor;1", "sensor5", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		sensors = new List<TwinOverride>()
		{
			sensor1,
			sensor2
		};

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		harness.repositoryTimeSeriesBuffer.Data.Any(v => v.DtId == "sensor1").Should().BeTrue();
		//dtid is remapped on load
		(insights, actors, _) = await harness.ExecuteRules(filePath, assertSimulation: false);
		actor = actors.Single();
		actor.OutputValues.Points.Any(v => v.IsValid).Should().BeTrue();
		harness.repositoryTimeSeriesBuffer.Data.Any(v => v.DtId == "sensor5").Should().BeTrue();
		harness.repositoryTimeSeriesBuffer.Data.Any(v => v.DtId == "sensor1").Should().BeFalse();
	}
}
