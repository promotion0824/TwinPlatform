using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug75887Tests
{
	[TestMethod]
	public void Bug75887_Test()
	{
		double percentage = 0.666;

		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(percentage)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Water Flow", "water_flow", "OPTION([dtmi:com:willowinc:WaterFlowSensor;1])"),
			new RuleParameter("Leak Detection", "result", "([water_flow] > 10)"),
		};

		var rule = new Rule()
		{
			Id = "meter-water-flow-leak-detection-metric",
			PrimaryModelId = "dtmi:com:willowinc:WaterMeter;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Bug75887", "Timeseries.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:WaterFlowSensor;1", "6f483d75-7de9-479d-9779-1bb620f7b5d4");

		insight.Should().NotBeNull();

		insight!.IsFaulty.Should().BeTrue();

		insight.Occurrences.Count(v => v.IsFaulted).Should().Be(2);

		bugHelper.Actor!.TimedValues["percentage_faulted"].Points.All(v => v.ValueDouble >= percentage).Should().BeTrue();
	}
}
