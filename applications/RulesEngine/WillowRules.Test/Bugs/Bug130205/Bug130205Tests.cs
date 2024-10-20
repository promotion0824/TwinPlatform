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
public class Bug130205Tests
{
	[TestMethod]
	public async Task Bug_130205_CorrectVariableText()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.48),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.48)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Expression", "result", "[dtmi:com:willowinc:airport:Sensor;1] == 1", "")
		};
		var scores = new List<RuleParameter>()
		{
			new RuleParameter("cooler_hours", "cooler_hours", "TIME/60/60", ""),
			new RuleParameter("time_out_of_target", "time_out_of_target", "IF(result, time_out_of_target + DELTA(cooler_hours) * 60, 0)", "")
		};

		var rule = new Rule()
		{
			Id = "plane-docked-at-gate",
			PrimaryModelId = "dtmi:com:willowinc:airport:Equipment;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements,
			ImpactScores = scores,
			Description = "tot is {time_out_of_target}"
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:airport:Equipment;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:airport:Sensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug130205", "Timeseries.csv");

		(var insights, var actors, _) = await harness.ExecuteRules(filePath);

		var insight = insights[0];

		var occ1 = insight.Occurrences[1];
		var occ2 = insight.Occurrences[2];
		var occ3 = insight.Occurrences[3];
		var occ5 = insight.Occurrences[5];

		occ1.IsValid.Should().BeFalse();
		occ1.Text.Should().Be("Result has 00:00:00 of data");

		occ2.IsValid.Should().BeTrue();
		occ2.Text.Should().Be("tot is 0.00");

		//as soon as it became faulted, result was already "false" again (not triggerring) but keep last triggered value
		occ3.IsFaulted.Should().BeTrue();
		occ3.Started.ToUniversalTime().Should().Be(DateTime.Parse("2022-08-03T12:41:47").ToUniversalTime());
		occ3.Text.Should().Be("tot is 30.00");

		occ5.IsFaulted.Should().BeTrue();
		//this is zero, becuase of the one trigger blip. At that point TIME is not tracking a single blip, but the impact score
		//uses DELTA(TIME) and because TIME hasn't changed, the DELTA is zero.
		occ5.Started.ToUniversalTime().Should().Be(DateTime.Parse("2022-08-18T12:00:45").ToUniversalTime());
		occ5.Text.Should().Be("tot is 10.00");
	}
}
