using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using System.Threading.Tasks;
using System.Linq;
using FluentAssertions.Primitives;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item89776Tests
{
	[DataTestMethod]
	[DataRow("MAX([dtmi:com:willowinc:sensor;1], 1d, -1d)", new double[]
	{
		1,
		1,
		1,
		1,
		3,
		3,
		5,
		5,
		5,
		5,
		5,
		5,
		5,
		5,
		5,
		7,
		17,
		17,
		19,
		19,
		22,
		22,
		24,
		24,
		26,
		26,
		27,
		27,
		4,
		3,
		3,
		3,
	}, false)]
	[DataRow("MAX([dtmi:com:willowinc:sensor;1], 2d, -1wk)", new double[]
	{
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		1,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		24,
		7,
		7,
	}, true)]
	public async Task TemporalTestsWithAnchor(string expression, double[] values, bool assertCount)
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.05),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.05)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("temporal", "temporal", expression),
			new RuleParameter("result", "result", "temporal")
		};

		var rule = new Rule()
		{
			Id = "r1",
			PrimaryModelId = "dtmi:com:willowinc:equipment;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:equipment;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:sensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		var simulationService = harness.CreateRuleSimulationService(BugHelper.GetFullDataPath("Item89776", "TimeSeries.csv"));

		var result = await simulationService.ExecuteRule(
			rule,
			equipment.twinId,
			new DateTime(2023, 7, 1),
			new DateTime(2023, 10, 1),
			enableCompression: false,
			optimizeCompression: false);

		var actor = result.actor;

		var points = actor.TimedValues["temporal"].Points
			.Where(v => v.Timestamp >= new DateTime(2023, 8, 1))
			.ToList();

		var buffer = points.Select(v => Math.Round(v.NumericValue)).ToList();

		if (assertCount)
		{
			buffer.Count.Should().Be(values.Length);
		}

		for (var i = 0; i < values.Length; i++)
		{
			var val = values[i];
			Math.Round(buffer[i]).Should().BeApproximately(val, 2);
		}
	}
}
