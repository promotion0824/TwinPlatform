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
public class InsightSyncTests
{
	[TestMethod]
	public async Task Bug_115920_All_Occurrences_Should_Send_ToCommand()
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

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("InsightSync", "TimeSeries.csv");

		int maxOutputs = 1;//default value in prod is 1
		(var insights, var actors, _) = await harness.ExecuteRules(filePath, maxOutputvaluesToKeep: maxOutputs, assertSimulation: false);
		
		insights.First().Occurrences.Count.Should().Be(3);
		//keep very little on actor (1 point)
		actors.First().OutputValues.Points.Count.Should().Be(maxOutputs);
		harness.commandInsightService.LastOccurrenceCount.Should().Be(3);
	}

	[TestMethod]
	public async Task Should_Send_ToCommand_EvenIfCurrentlyInvalid()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.1),
			Fields.OverHowManyHours.With(1),
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Expression", "result", "[dtmi:com:willowinc:SomeSensor;1] = 0", "")
		};

		var rule = new Rule()
		{
			Id = "floor-unoccupied-when-overtime-air-requested",
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

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("InsightSync", "InvalidTimeSeries.csv");

		//only keep the last output to mimick full invalid list for the second run
		int maxOutputs = 2;

		//muist be a batch to run in insights
		(var insights, _, _) = await harness.ExecuteRules(filePath, maxOutputvaluesToKeep: maxOutputs, endDate: DateTime.Parse("2022-08-24T15:32:45").ToUniversalTime(), assertSimulation: false, isRealtime: false);

		var insight = insights.First();

		insight.Occurrences.Count().Should().BeGreaterThanOrEqualTo(maxOutputs);
		//confirm second last should be valid
		insight.Occurrences[insight.Occurrences.Count - 2].IsValid.Should().BeTrue();
		//sync while last is invalid
		insight.Occurrences.Last().IsValid.Should().BeFalse();
		harness.commandInsightService.WasCalled.Should().BeTrue();

		maxOutputs = 1;

		harness.commandInsightService.WasCalled = false;

		(insights, _, _) = await harness.ExecuteRules(filePath, maxOutputvaluesToKeep: maxOutputs, startDate: DateTime.Parse("2022-08-24T15:34:45").ToUniversalTime(), assertSimulation: false, isRealtime: true);

		insight = insights.First();

		insight.Occurrences.Last().IsValid.Should().BeFalse();

		//back to realtime and shouldn't sync invalid "only" entry
		harness.commandInsightService.WasCalled.Should().BeFalse();
	}
}
