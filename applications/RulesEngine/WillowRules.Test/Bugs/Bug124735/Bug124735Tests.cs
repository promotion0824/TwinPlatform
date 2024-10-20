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
public class Bug124735Tests
{
	[TestMethod]
	public async Task Bug_124735InsightShouldBeDeletedIfFiltered()
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
			Id = "rule1",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements,
			CommandEnabled = true
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:SomeSensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		var equipment2 = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment2");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:SomeSensor;1", "sensor2", trendId: "a9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors2 = new List<TwinOverride>()
		{
			sensor2
		};

		var rule2 = new Rule()
		{
			Id = "rule2",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements,
			CommandEnabled = true
		};

		harness.OverrideCaches(rule, equipment2, sensors2);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug124735", "TimeSeries.csv");

		(var insights, var actors, _) = await harness.ExecuteRules(filePath, maxOutputvaluesToKeep: 1, assertSimulation: false);

		insights.Count.Should().Be(2);

		rule.Filters.Add(new RuleParameter("Expression", "filter", "this.Id = 'equipment'", ""));

		await harness.GenerateRuleInstances();

		(insights, actors, _) = await harness.ExecuteRules(filePath, maxOutputvaluesToKeep: 1, assertSimulation: false);

		insights.Count.Should().Be(1);
		//this one was not filtered
		insights.First().Id.Should().Be("equipment_rule1");
	}
}
