using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item125373Tests
{
	[TestMethod]
	public void Item125373_HandleJson()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.5)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("delta_input", "delta_input", "[dtmi:com:willowinc:Event;1]"),
			new RuleParameter("delta_1", "delta_1", "DELTA(delta_input, (delta_input)h, -((NOW - [dtmi:com:willowinc:Event;1].date).TotalDays)d)"),
			new RuleParameter("other_value2", "other_value2", "[sensor1].other.value"),
			new RuleParameter("my_value", "my_value", "[dtmi:com:willowinc:Event;1].the_value"),
			new RuleParameter("sensor_value", "sensor_value", "IFNAN(sensor_value, 0) + [dtmi:com:willowinc:Event;1]"),
			new RuleParameter("other_object", "other_object", "[dtmi:com:willowinc:Event;1].other"),
			new RuleParameter("other_value", "other_value", "[dtmi:com:willowinc:Event;1].other.value"),
			new RuleParameter("other_value_1", "other_value_1", "other_object.value"),
			new RuleParameter("datetest_1", "datetest_1", "([sensor1].date - [sensor1].date2).TotalHours"),
			new RuleParameter("datetest_2", "datetest_2", "([dtmi:com:willowinc:Event;1].date - [dtmi:com:willowinc:Event;1].date2).TotalHours"),
			new RuleParameter("Expression", "result", "1"),
		};

		var rule = new Rule()
		{
			Id = "food-display-case-evaporator-fan-over-design",
			PrimaryModelId = "dtmi:com:willowinc:Equipment;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item125373", "Timeseries.csv");

		var sensors = new List<TwinOverride>()
		{
			new TwinOverride("dtmi:com:willowinc:Event;1", "sensor1","50158491-a62d-4b45-98be-f505b7181f0d"),
		};

		var equipment = new TwinOverride(rule.PrimaryModelId, "equipment");

		bugHelper.GenerateInsightForPoint(rule, equipment, sensors, limitUntracked: false, enableCompression: false);

		var actor = bugHelper.Actor!;
		actor.TimedValues.Values.All(v => v.Points.Count() > 0).Should().BeTrue();
		var points = actor.TimedValues["my_value"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();

		points = actor.TimedValues["other_value2"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();

		points = actor.TimedValues["sensor_value"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();

		points = actor.TimedValues["other_object"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue == 0).Should().BeTrue();
		points.Points.All(v => JsonConvert.SerializeObject(v.ValueText) is not null).Should().BeTrue();

		points = actor.TimedValues["other_value"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();

		points = actor.TimedValues["other_value_1"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();

		points = actor.TimedValues["datetest_1"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();

		points = actor.TimedValues["delta_1"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.Any(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();
	}

	[TestMethod]
	public void Item125373_HandleJsonAsThis()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.5)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("my_value1", "my_value1", "this"),
			new RuleParameter("delta_1", "delta_1", "DELTA(my_value1, (my_value1)h, -((NOW - this.date).TotalDays)d)"),
			new RuleParameter("my_value", "my_value", "this.the_value"),
			new RuleParameter("sensor_value", "sensor_value", "IFNAN(sensor_value, 0) + this"),
			new RuleParameter("other_object", "other_object", "this.other"),
			new RuleParameter("other_value", "other_value", "this.other.value"),
			new RuleParameter("other_value_1", "other_value_1", "other_object.value"),
			new RuleParameter("datetest_1", "datetest_1", "(this.date - this.date2).TotalHours"),
			new RuleParameter("Expression", "result", "1"),
		};

		var rule = new Rule()
		{
			Id = "food-display-case-evaporator-fan-over-design",
			PrimaryModelId = "dtmi:com:willowinc:Event;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item125373", "Timeseries.csv");

		var sensors = new List<TwinOverride>()
		{
			//needs at least one relationship to build up a graph for binding
			new TwinOverride("dtmi:com:willowinc:SomeSensor;1", "sensor2","sensor2"),
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:Event;1", "sensor1", "50158491-a62d-4b45-98be-f505b7181f0d");

		bugHelper.GenerateInsightForPoint(rule, equipment, sensors, limitUntracked: false, enableCompression: false);

		var actor = bugHelper.Actor!;
		actor.TimedValues.Values.All(v => v.Points.Count() > 0).Should().BeTrue();
		var points = actor.TimedValues["my_value"];

		points = actor.TimedValues["my_value"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();

		points = actor.TimedValues["sensor_value"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();

		points = actor.TimedValues["other_object"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue == 0).Should().BeTrue();
		points.Points.All(v => JsonConvert.SerializeObject(v.ValueText) is not null).Should().BeTrue();

		points = actor.TimedValues["other_value"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();

		points = actor.TimedValues["other_value_1"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();

		points = actor.TimedValues["datetest_1"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();

		points = actor.TimedValues["delta_1"];
		points.Points.Count().Should().BeGreaterThan(0);
		points.Points.Any(v => v.NumericValue > 0).Should().BeTrue();
		points.Points.All(v => string.IsNullOrEmpty(v.ValueText)).Should().BeTrue();
	}

	[TestMethod]
	public void DescriptionMustBindToTelemtryJsonProperties_87696()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.5)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("my_value1", "my_value1", "this.other.value"),
			new RuleParameter("text1", "text1", "this.text"),
			new RuleParameter("date2", "date2", "this.date2"),
			new RuleParameter("Expression", "result", "1"),
		};

		var rule = new Rule()
		{
			Id = "food-display-case-evaporator-fan-over-design",
			PrimaryModelId = "dtmi:com:willowinc:Event;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements,
			Description = "My Value is {my_value1}. My date is {date2}. My text is {text1}",
			Recommendations = "My recommendation is {my_value1}. My date is {date2}. My text is {text1}"
		};

		var bugHelper = new BugHelper("Item125373", "Timeseries.csv");

		var sensors = new List<TwinOverride>()
		{
			//needs at least one relationship to build up a graph for binding
			new TwinOverride("dtmi:com:willowinc:SomeSensor;1", "sensor2","sensor2"),
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:Event;1", "sensor1", "50158491-a62d-4b45-98be-f505b7181f0d");

		var insight = bugHelper.GenerateInsightForPoint(rule, equipment, sensors, limitUntracked: false, enableCompression: false);

		insight.Text.Should().Be("My Value is 3.00. My date is 2023-04-19T19:30:15. My text is xyz");
		insight.RuleRecomendations.Should().Be("My recommendation is 3.00. My date is 2023-04-19T19:30:15. My text is xyz");
	}
}
