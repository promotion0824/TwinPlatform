using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item124199Tests
{
	[TestMethod]
	public void Item124199_Test()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.5)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Fan Speed", "fan_speed", "IFNAN(fan_speed, 0) + IF(ISNAN(fan_speed), 0, fan_speed)  + [dtmi:com:willowinc:FanCurrentSensor;1]"),
			new RuleParameter("Delta Time", "deltatime", "DELTA_TIME_S"),
			new RuleParameter("Expression", "result", "fan_speed > 1"),
		};

		var rule = new Rule()
		{
			Id = "food-display-case-evaporator-fan-over-design",
			PrimaryModelId = "dtmi:com:willowinc:RefrigeratedFoodDisplayCase;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item124199", "Timeseries.csv");

		var sensors = new List<TwinOverride>()
		{
			new TwinOverride("dtmi:com:willowinc:FanCurrentSensor;1", "944-CKTS-BC-C03b-MD-MEAT-EVAP-FAN-AMPS","50158491-a62d-4b45-98be-f505b7181f0d"),
		};

		var equipment = new TwinOverride(rule.PrimaryModelId, "equipment");

		var insight = bugHelper.GenerateInsightForPoint(rule, equipment, sensors, limitUntracked: false, enableCompression: false);

		var points = bugHelper.Actor!.TimedValues["fan_speed"].Points.ToList();
		var deltaPoints = bugHelper.Actor!.TimedValues["deltatime"].Points.ToList();

		deltaPoints.Count.Should().BeGreaterThan(1);
		points.Count.Should().BeGreaterThan(1);

		var value = points.First().NumericValue;

		foreach (var p in points)
		{
			p.NumericValue.Should().Be(value);

			value += value + 1;
		}

		//first one is zero when actor is created
		deltaPoints.Skip(1).All(v => v.NumericValue > 0).Should().BeTrue();
	}
}
