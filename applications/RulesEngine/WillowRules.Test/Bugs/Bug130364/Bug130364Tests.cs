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
public class Bug130364Tests
{
	[TestMethod]
	public async Task Bug_130364_CorrectOccurrenceStartAndEndDate()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.25),
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTimeOff.With(0.25)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Expression", "result", "[dtmi:com:willowinc:airport:Sensor;1] == 1", "")
		};

		var rule = new Rule()
		{
			Id = "plane-docked-at-gate",
			PrimaryModelId = "dtmi:com:willowinc:airport:Equipment;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
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

		var filePath = BugHelper.GetFullDataPath("Bug130364", "Timeseries.csv");

		(var insights, var actors, _) = await harness.ExecuteRules(filePath);

		var insight = insights[0];

		var occ1 = insight.Occurrences[1];
		var occ2 = insight.Occurrences[2];
		var occ3 = insight.Occurrences[3];
		var occ5 = insight.Occurrences[5];

		
		occ3.IsFaulted.Should().BeTrue();
		//start time must align to previous 0->1 transition (trigger) on result
		occ3.Started.ToUniversalTime().Should().Be(DateTime.Parse("2022-07-02T04:30:46").ToUniversalTime());
		//end time must align to previous 1->0 transition (untrigger) on result
		occ3.Ended.ToUniversalTime().Should().Be(DateTime.Parse("2022-07-02T13:27:45").ToUniversalTime());

		occ5.IsFaulted.Should().BeTrue();
		//start time must align to previous 0->1 transition (trigger) on result
		occ5.Started.ToUniversalTime().Should().Be(DateTime.Parse("2022-07-03T11:37:45").ToUniversalTime());
		//end time must align to previous 1->0 transition (untrigger) on result
		occ5.Ended.ToUniversalTime().Should().Be(DateTime.Parse("2022-07-04T01:27:45").ToUniversalTime());
	}
}
