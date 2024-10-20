using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

#nullable disable

namespace WillowRules.Test.Bugs;

[TestClass]
public class ToleranceTests
{
	private TwinOverride equipment;
	private TwinOverride sensor1;
	private TwinOverride sensor2;

	[TestInitialize]
	public void Setup()
	{
		equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment", "");
		sensor1 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor1", "f9463069-6db6-465d-b3e1-96969ac30c0a");
		sensor2 = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor2", "a9463069-6db6-465d-b3e1-96969ac30c0a");
	}

	[TestMethod]
	public async Task BufferShouldBeInvalidAfterTolernaceChanges()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(0.1),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var rule = new Rule()
		{
			Id = "tolerance",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = new List<RuleParameter>()
			{
				new RuleParameter("tolerance", "tolerance", "[sensor1]"),
				new RuleParameter("result", "result", "1"),
			},
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		await harness.AddToModelGraph(sensor1, "dtmi:com:willowinc:TerminalUnit;1");

		await harness.AddToModelGraph(sensor2, "dtmi:com:willowinc:TerminalUnit;1");

		harness.OverrideCaches(rule, equipment, sensor1, sensor2);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Expressions", "TolerantOptionTimeseries.csv");

		//execute before missing data
		(_, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single(v => v.RuleId == "tolerance");

		var texts = actor.OutputValues.Points
			.Where(v => !v.IsValid);

		//no error similar to:
		//"Variable 'sensor2' does not have sufficient data for period 01:00:00"
		texts.Any(v => v.Text.Contains("Missing value: [sensor1]")).Should().BeTrue();

		actor.IsValid.Should().BeTrue();
	}

	[TestMethod]
	public async Task ImpactScoresShouldNotFailTemporalAfterTolernaceChanges()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(0.1),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var rule = new Rule()
		{
			Id = "tolerance",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = new List<RuleParameter>()
			{
				new RuleParameter("tolerance", "tolerance", "[sensor1]"),
				new RuleParameter("result", "result", "1"),
			},
			ImpactScores = new List<RuleParameter>()
			{
				new RuleParameter("score", "score", "MAX([sensor2], 1h)")
			},
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		await harness.AddToModelGraph(sensor1, "dtmi:com:willowinc:TerminalUnit;1");

		await harness.AddToModelGraph(sensor2, "dtmi:com:willowinc:TerminalUnit;1");

		harness.OverrideCaches(rule, equipment, sensor1, sensor2);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Expressions", "TolerantOptionTimeseries.csv");

		//execute before missing data
		(_, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single(v => v.RuleId == "tolerance");

		var texts = actor.OutputValues.Points
			.Where(v => !v.IsValid)
			.Where(v => !v.Text.Contains("Missing value"));

		//no error similar to:
		//"Variable 'sensor2' does not have sufficient data for period 01:00:00"
		texts.Any(v => v.Text.Contains("sensor2")).Should().BeFalse();

		actor.IsValid.Should().BeTrue();
	}

	[TestMethod]
	public async Task NoToleranceShouldFail()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(0.1),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var rule = new Rule()
		{
			Id = "tolerance",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = new List<RuleParameter>()
			{
				new RuleParameter("no_tolerance", "no_tolerance", "MAX([dtmi:com:willowinc:TerminalUnit;1].[dtmi:com:willowinc:ZoneAirTemperatureSensor;1])"),
				new RuleParameter("tolerance", "tolerance", "TOLERANCE(MAX([dtmi:com:willowinc:TerminalUnit;1].[dtmi:com:willowinc:ZoneAirTemperatureSensor;1]), 0.5)"),
				new RuleParameter("result", "result", "1"),
			},
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		await harness.AddToModelGraph(sensor1, "dtmi:com:willowinc:TerminalUnit;1");

		await harness.AddToModelGraph(sensor2, "dtmi:com:willowinc:TerminalUnit;1");

		harness.OverrideCaches(rule, equipment, sensor1, sensor2);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Expressions", "ToleranceTimeseries.csv");

		//execute before missing data
		(_, var actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T12:50:00").ToUniversalTime());

		var actor = actorsList.Single(v => v.RuleId == "tolerance");

		var points = actor.TimedValues["tolerance"].Points.ToList();
		var last = points.Last();

		last.ValueDouble.Should().Be(2);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T12:40:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();

		//now go past missing data but before data comes back
		(_, actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T14:15:00").ToUniversalTime());

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();
		last = points.Last();

		last.ValueDouble.Should().Be(2);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T13:30:00").ToUniversalTime());
		actor.IsValid.Should().BeFalse();
		actor.OutputValues.Points.Last().Text.Should().Be("Missing value: [sensor2] 70.0 min ago");

		//now all of it. It ends with both points having data
		(_, actorsList, _) = await harness.ExecuteRules(filePath);

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();

		last = points.Last();
		last.ValueDouble.Should().Be(2);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T14:45:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();
	}

	[TestMethod]
	[DataRow(0.5)]
	public async Task ToleranceOK(double tolerance)
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(0.1),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var rule = new Rule()
		{
			Id = "tolerance",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = new List<RuleParameter>()
			{
				new RuleParameter("tolerance", "tolerance", $"TOLERANCE(MAX([dtmi:com:willowinc:TerminalUnit;1].[dtmi:com:willowinc:ZoneAirTemperatureSensor;1]), {tolerance})"),
				new RuleParameter("result", "result", "1"),
			},
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		await harness.AddToModelGraph(sensor1, "dtmi:com:willowinc:TerminalUnit;1");

		await harness.AddToModelGraph(sensor2, "dtmi:com:willowinc:TerminalUnit;1");

		harness.OverrideCaches(rule, equipment, sensor1, sensor2);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Expressions", "ToleranceTimeseries.csv");

		//execute before missing data
		(_, var actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T12:50:00").ToUniversalTime());

		var actor = actorsList.Single(v => v.RuleId == "tolerance");

		var points = actor.TimedValues["tolerance"].Points.ToList();
		var last = points.Last();

		last.ValueDouble.Should().Be(2);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T12:40:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();

		//now go past missing data but before data comes back
		(_, actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T14:15:00").ToUniversalTime());

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();
		last = points.Last();

		last.ValueDouble.Should().Be(1);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T14:00:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();

		//now all of it. It ends with both points having data
		(_, actorsList, _) = await harness.ExecuteRules(filePath);

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();

		//make sure the MAX of 1 is in the list
		points.Last(v => v.ValueDouble == 1).Timestamp.Should().Be(DateTime.Parse("2022-08-24T14:15:00").ToUniversalTime());

		last = points.Last();
		last.ValueDouble.Should().Be(2);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T14:45:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();
	}

	[TestMethod]
	public async Task ToleranceWith0ShouldREturnNaN()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(0.1),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var rule = new Rule()
		{
			Id = "tolerance",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = new List<RuleParameter>()
			{
				//sensor2 has a big , should return nan
				new RuleParameter("tolerance", "tolerance", "IFNAN(TOLERANCE(MAX({[sensor2]}), 0), 5)"),
				new RuleParameter("result", "result", "1"),
			},
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		await harness.AddToModelGraph(sensor1, "dtmi:com:willowinc:TerminalUnit;1");

		await harness.AddToModelGraph(sensor2, "dtmi:com:willowinc:TerminalUnit;1");

		harness.OverrideCaches(rule, equipment, sensor1, sensor2);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Expressions", "ToleranceTimeseries.csv");

		//execute before missing data
		(_, var actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T12:50:00").ToUniversalTime());

		var actor = actorsList.Single(v => v.RuleId == "tolerance");

		var points = actor.TimedValues["tolerance"].Points.ToList();
		var last = points.Last();

		last.ValueDouble.Should().Be(2);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T12:40:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();

		//now go past missing data but before data comes back
		(_, actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T14:15:00").ToUniversalTime());

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();
		last = points.Last();

		last.ValueDouble.Should().Be(5);//falls back to 5 due to the IFNAN chk
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T14:00:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();

		//now all of it. It ends with both points having data
		(_, actorsList, _) = await harness.ExecuteRules(filePath);

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();

		last = points.Last();
		last.ValueDouble.Should().Be(2);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T14:45:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();
	}

	[TestMethod]
	[DataRow(0.6)]//just not enough
	[DataRow(1)]//acts like no tolerance
	[DataRow(2)]//acts like no tolerance
	public async Task ToleranceNotOK(double tolerance)
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(0.1),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var rule = new Rule()
		{
			Id = "tolerance",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = new List<RuleParameter>()
			{
				new RuleParameter("tolerance", "tolerance", $"TOLERANCE(MAX([dtmi:com:willowinc:TerminalUnit;1].[dtmi:com:willowinc:ZoneAirTemperatureSensor;1]), {tolerance})"),
				new RuleParameter("result", "result", "1"),
			},
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		await harness.AddToModelGraph(sensor1, "dtmi:com:willowinc:TerminalUnit;1");

		await harness.AddToModelGraph(sensor2, "dtmi:com:willowinc:TerminalUnit;1");

		harness.OverrideCaches(rule, equipment, sensor1, sensor2);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Expressions", "ToleranceTimeseries.csv");

		//execute before missing data
		(_, var actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T12:50:00").ToUniversalTime());

		var actor = actorsList.Single(v => v.RuleId == "tolerance");

		var points = actor.TimedValues["tolerance"].Points.ToList();
		var last = points.Last();

		last.ValueDouble.Should().Be(2);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T12:40:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();

		//now go past missing data but before data comes back
		(_, actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T14:15:00").ToUniversalTime());

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();
		last = points.Last();

		last.ValueDouble.Should().Be(2);
		//around this time the other sensors is not timely
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T13:30:00").ToUniversalTime());
		actor.IsValid.Should().BeFalse();
		actor.OutputValues.Points.Last().Text.Should().Be("Missing value: [sensor2] 70.0 min ago");

		//now all of it. It ends with both points having data
		(_, actorsList, _) = await harness.ExecuteRules(filePath);

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();

		last = points.Last();
		last.ValueDouble.Should().Be(2);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T14:45:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();
	}

	[TestMethod]
	public async Task TolerantOptionOK()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(0.1),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var rule = new Rule()
		{
			Id = "tolerance",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = new List<RuleParameter>()
			{
				new RuleParameter("tolerance", "tolerance", "TOLERANTOPTION([sensor2], [sensor1], -5)"),
				new RuleParameter("result", "result", "1"),
			},
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		await harness.AddToModelGraph(sensor1, "dtmi:com:willowinc:TerminalUnit;1");

		await harness.AddToModelGraph(sensor2, "dtmi:com:willowinc:TerminalUnit;1");

		harness.OverrideCaches(rule, equipment, sensor1, sensor2);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Expressions", "TolerantOptionTimeseries.csv");

		//execute before missing data
		(_, var actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T12:50:00").ToUniversalTime());

		var actor = actorsList.Single(v => v.RuleId == "tolerance");

		var points = actor.TimedValues["tolerance"].Points.ToList();
		var last = points.Last();

		last.ValueDouble.Should().Be(2);//sensor2
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T12:40:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();

		//now go past missing data but before data comes back
		(_, actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T14:15:00").ToUniversalTime());

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();
		last = points.Last();

		last.ValueDouble.Should().Be(1);
		//around this time the other sensors is not timely
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T14:00:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();

		//now go past all sensor data missing
		(_, actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T18:15:00").ToUniversalTime());

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();
		last = points.Last();

		last.ValueDouble.Should().Be(-5);//the 3rd option static value
										 //around this time the other sensors is not timely
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T18:00:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();

		//now all of it. It ends with both points having data
		(_, actorsList, _) = await harness.ExecuteRules(filePath);

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();

		last = points.Last();
		last.ValueDouble.Should().Be(2);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T19:15:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();
	}

	[TestMethod]
	public async Task TolerantOptionNotOK()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(0.1),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var rule = new Rule()
		{
			Id = "tolerance",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = new List<RuleParameter>()
			{
				new RuleParameter("tolerance", "tolerance", "TOLERANTOPTION([sensor2], [sensor1])"),
				new RuleParameter("result", "result", "1"),
			},
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		await harness.AddToModelGraph(sensor1, "dtmi:com:willowinc:TerminalUnit;1");

		await harness.AddToModelGraph(sensor2, "dtmi:com:willowinc:TerminalUnit;1");

		harness.OverrideCaches(rule, equipment, sensor1, sensor2);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Expressions", "TolerantOptionTimeseries.csv");

		//execute before missing data
		(_, var actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T12:50:00").ToUniversalTime());

		var actor = actorsList.Single(v => v.RuleId == "tolerance");

		var points = actor.TimedValues["tolerance"].Points.ToList();
		var last = points.Last();

		last.ValueDouble.Should().Be(2);//sensor2
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T12:40:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();

		//now go past missing data but before data comes back
		(_, actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T14:15:00").ToUniversalTime());

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();
		last = points.Last();

		last.ValueDouble.Should().Be(1);
		//around this time the other sensors is not timely
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T14:00:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();

		//now go past all sensor data missing
		(_, actorsList, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2022-08-24T18:15:00").ToUniversalTime());

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();
		last = points.Last();

		last.ValueDouble.Should().Be(1);//the 3rd option static value
										//around this time the other sensors is not timely
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T14:00:00").ToUniversalTime());
		actor.IsValid.Should().BeFalse();
		actor.OutputValues.Points.Last().Text.Should().Be("Missing values: [sensor2] 310.0 min ago, [sensor1] 0.0 min ago");

		//now all of it. It ends with both points having data
		(_, actorsList, _) = await harness.ExecuteRules(filePath);

		actor = actorsList.Single(v => v.RuleId == "tolerance");

		points = actor.TimedValues["tolerance"].Points.ToList();

		last = points.Last();
		last.ValueDouble.Should().Be(2);
		last.Timestamp.Should().Be(DateTime.Parse("2022-08-24T19:15:00").ToUniversalTime());
		actor.IsValid.Should().BeTrue();
	}
}
