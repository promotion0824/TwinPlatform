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

public class Bug92091Tests
{
	[TestMethod]
	public async Task Bug_92091_FaultedBands()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.05),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.05)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Expression", "result", "!(!([dtmi:com:willowinc:airport:AircraftDockState;1]))", "")
		};

		var rule = new Rule()
		{
			Id = "plane-docked-at-gate",
			PrimaryModelId = "dtmi:com:willowinc:airport:PassengerBoardingBridge;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:airport:PassengerBoardingBridge;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:airport:AircraftDockState;1", "sensor1", trendId: "037947b9-29db-4376-b346-7e2abc621e29");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug92091", "TimeSeries.csv");

		(var insights, var actors, _) = await harness.ExecuteRules(filePath, startDate: DateTime.Parse("2023-05-28T00:05:00Z"), endDate: DateTime.Parse("2023-10-25T00:05:00Z"), assertSimulation: false);

		var insight = insights[0];

		insight.Occurrences.Count.Should().BeGreaterThan(0);
		insight.Occurrences.Any(v => v.IsFaulted).Should().BeTrue();

		var bugHelper = new BugHelper("Bug92091", "TimeSeries.csv");

		bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:airport:AircraftDockState;1", "037947b9-29db-4376-b346-7e2abc621e29", startDate: DateTime.Parse("2023-05-28T00:05:00Z"), endDate: DateTime.Parse("2023-10-25T00:05:00Z"), assertSimulation: false, outputImagesOnly: false);
	}
}
