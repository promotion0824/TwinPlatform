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
public class Bug78669Tests
{
	[TestMethod]
	public async Task Bug_78669_ShouldIgnoreInvalidScores()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(-10.0),
			Fields.MaxTrigger.With(10.0),
			Fields.PercentageOfTime.With(0.1),
			Fields.OverHowManyHours.With(1)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Chiller Status", "CH_STS", "[dtmi:com:willowinc:RunSensor;1]"),
			new RuleParameter("Expression", "result", "[CH_STS]"),
		};

		var impactScores = new List<RuleParameter>()
		{
			new RuleParameter("Daily Avoidable Energy", "daily_avoidable_energy", "1 / TIME", "kWh"),
		};

		var rule = new Rule()
		{
			Id = "chiller-high-approach",
			PrimaryModelId = "dtmi:com:willowinc:Chiller;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			ImpactScores = impactScores,
			Elements = elements
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:Chiller;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:RunSensor;1", "sensor1", trendId: "316e2987-26a5-4243-8dd1-ee5931a2d790");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug78669", "Timeseries.csv");

		var endDate = DateTime.Parse("2023-04-25T22:16:21.6578017Z");

		(var insights, var actors, _) = await harness.ExecuteRules(filePath, assertSimulation: false);

		var actor = actors.Single();

		var actorScores = actor.TimedValues["daily_avoidable_energy"].Points;

		actorScores.Any().Should().BeTrue();

		actorScores.Any(v => double.IsInfinity(v.NumericValue)).Should().BeFalse();
	}
}
