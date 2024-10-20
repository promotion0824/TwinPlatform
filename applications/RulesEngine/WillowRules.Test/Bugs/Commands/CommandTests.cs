using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class CommandTests
{
	[TestMethod]
	public async Task CommandShouldSave()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.MinTrigger.With(12),
			Fields.MaxTrigger.With(35),
			Fields.PercentageOfTime.With(0.01)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("setpoint", "setpoint", "OPTION([dtmi:com:willowinc:Setpoint;1])"),
			new RuleParameter("Zone Temperature Setpoint", "result", "[setpoint] + 1"),
		};

		var triggers = new List<RuleTrigger>()
		{
			new RuleTrigger(RuleTriggerType.TriggerCommand)
			{
				CommandType = CommandType.AtLeast,
				Condition = new RuleParameter("condition", "condition", "[setpoint] > 1"),
				Name = "command 1",
				Point = new RuleParameter("point", "point", "[dtmi:com:willowinc:Setpoint;1]"),
				Value = new RuleParameter("value", "value", "[setpoint] + 1")
				{
					Units = "%"
				},
			}
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-setpoint-out-of-range",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			RuleTriggers = triggers,
			Elements = elements,
			CommandEnabled = true
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, "equipment", "dtmi:com:willowinc:Setpoint;1", "f9463069-6db6-465d-b3e1-96969ac30c0a");

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Commands", "Timeseries.csv");

		(var insights, var actorsList, _) = await harness.ExecuteRules(filePath, endDate: new DateTime(2022, 08, 25, 23, 0, 0, DateTimeKind.Utc));

		var actor = actorsList.Single();

		actor.OutputValues.Commands.Count.Should().Be(1);

		harness.repositoryCommand.Data.Should().HaveCount(1);

		var output = actor.OutputValues.Commands["command-1"];

		var command = harness.repositoryCommand.Data.Single();

		output.Points.Count.Should().BeGreaterThan(0);

		command.Occurrences.Count().Should().Be(output.Points.Count);

		command.StartTime.Should().BeAfter(new DateTime(2022, 08, 25, 22, 0, 0, DateTimeKind.Utc));
		command.EndTime.Should().BeNull();
		command.IsTriggered.Should().BeTrue();

		//this range become "untriggered". starttime triggers shoudl stop movin forward after the first untriggerred time
		await harness.ExecuteRules(filePath, startDate: new DateTime(2022, 08, 25, 23, 0, 0, DateTimeKind.Utc), endDate: new DateTime(2022, 08, 26, 12, 0, 0, DateTimeKind.Utc));

		command = harness.repositoryCommand.Data.Single();

		var startTime = command.StartTime;
		var endTime = command.StartTime;
		startTime.Should().BeAfter(new DateTime(2022, 08, 25, 23, 0, 0, DateTimeKind.Utc));
		startTime.Should().BeBefore(new DateTime(2022, 08, 26, 12, 0, 0, DateTimeKind.Utc));
		endTime.Should().BeExactly(command.StartTime);
		command.IsTriggered.Should().BeFalse();

		//and again, not movement of dates
		await harness.ExecuteRules(filePath, startDate: new DateTime(2022, 08, 26, 0, 0, 0, DateTimeKind.Utc));

		command = harness.repositoryCommand.Data.Single();
		//the dates should stay on the last "untriggered" time
		command.StartTime.Should().BeCloseTo(startTime, TimeSpan.FromSeconds(1));
		command.EndTime.Should().BeExactly(endTime);
		command.IsTriggered.Should().BeFalse();
	}

	[TestMethod]
	public async Task CommandPointShouldBindFromVariable()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.MinTrigger.With(12),
			Fields.MaxTrigger.With(35),
			Fields.PercentageOfTime.With(0.01)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("setpoint", "setpoint", "OPTION([dtmi:com:willowinc:Setpoint;1])"),
			new RuleParameter("Zone Temperature Setpoint", "result", "[setpoint] + 1"),
		};

		var triggers = new List<RuleTrigger>()
		{
			new RuleTrigger(RuleTriggerType.TriggerCommand)
			{
				CommandType = CommandType.AtLeast,
				Condition = new RuleParameter("condition", "condition", "[setpoint] > 1"),
				Name = "command 1",
				Point = new RuleParameter("point", "point", "setpoint"),
				Value = new RuleParameter("value", "value", "[setpoint] + 1")
				{
					Units = "%"
				},
			}
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-setpoint-out-of-range",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			RuleTriggers = triggers,
			Elements = elements,
			CommandEnabled = true
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, "equipment", "dtmi:com:willowinc:Setpoint;1", "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var ri = (await harness.GenerateRuleInstances()).Single();

		var boundTrigger = ri.RuleTriggersBound.Single();

		boundTrigger.Point.PointExpression.Serialize().Should().Be("[f9463069-6db6-465d-b3e1-96969ac30c0a]");

		boundTrigger.Relationships.Count().Should().Be(1);

		boundTrigger.Relationships.First().TwinId.Should().Be("equipment");
		boundTrigger.Relationships.First().RelationshipType.Should().Be("isCapabilityOf");

	}

	[TestMethod]
	public async Task OldCommandMustGetDeleted()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.MinTrigger.With(12),
			Fields.MaxTrigger.With(35),
			Fields.PercentageOfTime.With(0.01)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("setpoint", "setpoint", "OPTION([dtmi:com:willowinc:Setpoint;1])"),
			new RuleParameter("Zone Temperature Setpoint", "result", "[setpoint] + 1"),
		};

		var triggers = new List<RuleTrigger>()
		{
			new RuleTrigger(RuleTriggerType.TriggerCommand)
			{
				CommandType = CommandType.AtLeast,
				Condition = new RuleParameter("condition", "condition", "[setpoint] > 1"),
				Name = "command 1",
				Point = new RuleParameter("point", "point", "[dtmi:com:willowinc:Setpoint;1]"),
				Value = new RuleParameter("value", "value", "[setpoint] + 1")
				{
					Units = "%"
				},
			}
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-setpoint-out-of-range",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			RuleTriggers = triggers,
			Elements = elements,
			CommandEnabled = true
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, "equipment", "dtmi:com:willowinc:Setpoint;1", "f9463069-6db6-465d-b3e1-96969ac30c0a");

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Commands", "Timeseries.csv");

		await harness.ExecuteRules(filePath, endDate: new DateTime(2022, 08, 25, 23, 0, 0));

		rule.RuleTriggers.Clear();

		await harness.GenerateRuleInstances();

		await harness.ExecuteRules(filePath, startDate: new DateTime(2022, 08, 25, 23, 0, 0));

		harness.repositoryCommand.Data.Count.Should().Be(0);
	}
}
