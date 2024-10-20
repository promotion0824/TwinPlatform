using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

#nullable disable

namespace WillowRules.Test.Bugs;

[TestClass]
public class MinMaxTests
{
	private TwinOverride equipment;
	private TwinOverride sensor1;
	private TwinOverride sensor2;

	[TestInitialize]
	public void Setup()
	{
		equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment", "");
		sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");
		sensor2 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor2", "a9463069-6db6-465d-b3e1-96969ac30c0a");
	}

	[TestMethod]
	public async Task Min_Test()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Min Temperature", "result", "MIN([dtmi:com:willowinc:TerminalUnit;1].[dtmi:com:willowinc:ZoneAirTemperatureSensor;1])"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-overcooling-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		await harness.AddToModelGraph(new ModelData()
		{
			Id = sensor1.modelId,
			DtdlModel = new DtdlModel()
			{
				extends = new StringList()
				{
					"dtmi:com:willowinc:TerminalUnit;1"
				}
			}
		});

		await harness.AddToModelGraph(new ModelData()
		{
			Id = sensor2.modelId,
			DtdlModel = new DtdlModel()
			{
				extends = new StringList()
				{
					"dtmi:com:willowinc:TerminalUnit;1"
				}
			}
		});


		harness.OverrideCaches(rule, equipment,
			new List<TwinOverride>()
			{
				sensor1,
				sensor2
			});

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Expressions", "MinMaxTimeseries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single();

		var points = actor.TimedValues["result"].Points.ToList();

		points.Select(x => x.ValueDouble).Should().BeEquivalentTo(new[] { 3.0, 5.0, 7.0, 1.0, 1.0 });
	}

	[TestMethod]
	public async Task Max_Test()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Max Temperature", "result", "MAX([dtmi:com:willowinc:TerminalUnit;1].[dtmi:com:willowinc:ZoneAirTemperatureSensor;1])"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-overcooling-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		await harness.AddToModelGraph(new ModelData()
		{
			Id = sensor1.modelId,
			DtdlModel = new DtdlModel()
			{
				extends = new StringList()
				{
					"dtmi:com:willowinc:TerminalUnit;1"
				}
			}
		});

		await harness.AddToModelGraph(new ModelData()
		{
			Id = sensor2.modelId,
			DtdlModel = new DtdlModel()
			{
				extends = new StringList()
				{
					"dtmi:com:willowinc:TerminalUnit;1"
				}
			}
		});

		harness.OverrideCaches(rule, equipment,
			new List<TwinOverride>()
			{
				sensor1,
				sensor2
			});

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Expressions", "MinMaxTimeseries.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single();

		var points = actor.TimedValues["result"].Points.ToList();

		points.Select(x => x.ValueDouble).Should().BeEquivalentTo(new[] { 4.0, 5.0, 9.0, 9.0, 5.0 });
	}
}
