using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug91965Tests
{
	[TestMethod]
	public async Task Bug_91965_True_False_ToBoolShouldWork()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.1),
			Fields.OverHowManyHours.With(1),
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Expression", "result", "[dtmi:com:willowinc:SomeSensor;1]", "")
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
		var sensor1 = new TwinOverride("dtmi:com:willowinc:SomeSensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug91965", "Timeseries.csv");

		(var insights, var actors, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		var insight = insights[0];

		insight.Occurrences.Count.Should().BeGreaterThan(0);
		insight.Occurrences.Any(v => v.IsFaulted).Should().BeTrue();
	}
}
