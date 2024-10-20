using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Messaging.EventHubs.Consumer;
using FluentAssertions;
using Kusto.Cloud.Platform.Utils;
using Microsoft.AspNetCore.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item75098Tests
{
	[TestMethod]
	public void MustUpdateDescription()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.PercentageOfTimeOff.With(0.5),
			Fields.OverHowManyHours.With(1)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Discharge Air Flow", "discharge_air_flow", "OPTION([dtmi:com:willowinc:DischargeAirFlowSensor;1])", "degF"),
			new RuleParameter("Discharge Air Flow is stuck", "result", "[discharge_air_flow] == 0"),
		};

		var scores = new List<RuleParameter>()
		{
			new RuleParameter("Cost impact", "cost_impact", "0.5 * TIME"),
			new RuleParameter("Comfort impact", "comfort_impact", "0.0"),
			new RuleParameter("Reliability impact", "reliability_impact", "(2.0 * TIME) + 1", "%"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-stuck-discharge-air-flow",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			ImpactScores = scores,
			Elements = elements,
			Description = "{discharge_air_flow} {reliability_impact} {TIME} FAULTYTEXT(faulty)"
		};

		var bugHelper = new BugHelper("Item75098", "Timeseries.csv", true);

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:DischargeAirFlowSensor;1", "93560ff9-9a80-4afc-a00d-f5055042c048");

		int index = 0;

		bugHelper.Actor!.OutputValues.Points.Count(v => v.IsValid).Should().BeGreaterThan(0);
		bugHelper.Actor!.OutputValues.Points.Count(v => v.Faulted).Should().BeGreaterThan(0);

		foreach (var output in bugHelper.Actor!.OutputValues.Points)
		{
			if(!output.IsValid)
			{
				output.Variables.Count().Should().Be(0);
				continue;
			}

			//rule says only fault if == 0 so we should not have non-faulty values, ie values greater than 0, for faulty outputs
			if (output.Faulted)
			{
				((float)output.Variables.First(v => v.Key == "discharge_air_flow").Value).Should().Be(0);
			}
			else
			{
				((float)output.Variables.First(v => v.Key == "discharge_air_flow").Value).Should().BeGreaterThan(0);
			}
		}

		insight.Occurrences.Count.Should().Be(bugHelper.Actor!.OutputValues.Points.Count);

		foreach (var occ in insight.Occurrences)
		{
			var lookup = bugHelper.Actor!.OutputValues.Points[index].Variables.ToDictionary(v => v.Key, v => v.Value);

			if(occ.IsFaulted)
			{
				occ.Text.Should().Be($"0.00 {lookup["reliability_impact"]:0.00} % {lookup["TIME"]:0.00} s faulty");
			}
			else if(occ.IsValid)
			{
				occ.Text.Should().Be($"{lookup["discharge_air_flow"]:0.00} {lookup["reliability_impact"]:0.00} % {lookup["TIME"]:0.00} s");
			}
			
			index++;
		}

		insight.Occurrences.Last().Text.Should().Be("129.00 657588.60 % 328793.80 s");

		//last text should be last faulty value if there are faulty occurrences
		insight!.Text.Should().Be("0.00 657588.60 % 328793.80 s faulty");
	}
}
