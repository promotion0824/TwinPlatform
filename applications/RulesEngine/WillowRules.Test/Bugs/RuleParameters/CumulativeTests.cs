using FluentAssertions;
using IdentityModel.Client;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Willow.Expressions.Visitor;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

#nullable disable

namespace WillowRules.Test.Bugs;

[TestClass]
public class CumulativeTests
{
	private Rule rule;
	private Rule uc1Rule;

	[TestInitialize]
	public void Setup()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(24),
		};

		rule = new Rule()
		{
			Id = "terminal-unit-stuck-discharge-air-flow",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateUnchanging.ID,
			Elements = elements
		};

		var uc1Elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.25),
			Fields.OverHowManyHours.With(11)
		};

		uc1Rule = new Rule()
		{
			Id = "accumulateTimeSeconds-test",
			PrimaryModelId = "dtmi:com:willowinc:OccupancyZone;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Elements = uc1Elements
		};
	}

	[TestMethod]
	public void Accumulate_Test()
	{
		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Param1", "discharge_air_flow_cumulative", "OPTION([dtmi:com:willowinc:DischargeAirFlowSensor;1])", CumulativeType.Accumulate),
			new RuleParameter("Param3", "result_Accumulate", "[discharge_air_flow_cumulative] + 1"),
			new RuleParameter("Param3", "result", "1")
		};

		rule.Parameters = parameters;

		var bugHelper = new BugHelper("RuleParameters", "Timeseries.csv", true);

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:DischargeAirFlowSensor;1", "93560ff9-9a80-4afc-a00d-f5055042c048", testcaseName: "Accumulate");

		bugHelper.Actor.TimedValues["result_Accumulate"].GetLastValueDouble().Should().Be(919);
	}

	[TestMethod]
	public void Accumulate_ImpactScore_Test()
	{
		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Param1", "discharge_air_flow", "OPTION([dtmi:com:willowinc:DischargeAirFlowSensor;1])"),
			new RuleParameter("Param3", "result_Accumulate", "[discharge_air_flow]"),
			new RuleParameter("Param3", "result", "1")
		};

		var scores = new List<RuleParameter>()
		{
			new RuleParameter("Param1", "discharge_air_flow_cumulative", "[result_Accumulate]", CumulativeType.Accumulate),
			new RuleParameter("Param3", "total_to_Date", "[discharge_air_flow_cumulative] + 1")
		};

		rule.Parameters = parameters;
		rule.ImpactScores = scores;

		var bugHelper = new BugHelper("RuleParameters", "Timeseries.csv", true);

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:DischargeAirFlowSensor;1", "93560ff9-9a80-4afc-a00d-f5055042c048", testcaseName: "Accumulate");

		bugHelper.Actor.TimedValues["total_to_Date"].GetLastValueDouble().Should().Be(919);
	}

	[TestMethod]
	public void AccumulateTimeSeconds_Test()
	{
		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Param1", "discharge_air_flow_cumulative", "OPTION([dtmi:com:willowinc:DischargeAirFlowSensor;1])", CumulativeType.AccumulateTimeSeconds),
			new RuleParameter("Param3", "result_AccumulateTimeSeconds", "[discharge_air_flow_cumulative] + 1"),
			new RuleParameter("Param3", "result", "1")
		};

		rule.Parameters = parameters;

		var bugHelper = new BugHelper("RuleParameters", "Timeseries.csv", true);

		_ = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:DischargeAirFlowSensor;1", "93560ff9-9a80-4afc-a00d-f5055042c048", testcaseName: "AccumulateTimeSeconds");

		bugHelper.Actor.TimedValues["result_AccumulateTimeSeconds"].GetLastValueDouble().Should().Be(330480 + 1);
	}

	[TestMethod]
	public void AccumulateTimeMinutes_Test()
	{
		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Param1", "discharge_air_flow_cumulative", "OPTION([dtmi:com:willowinc:DischargeAirFlowSensor;1])", CumulativeType.AccumulateTimeMinutes),
			new RuleParameter("Param3", "result_AccumulateTimeMinutes", "[discharge_air_flow_cumulative] + 1"),
			new RuleParameter("Param3", "result", "1")
		};

		rule.Parameters = parameters;

		var bugHelper = new BugHelper("RuleParameters", "Timeseries.csv", true);

		_ = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:DischargeAirFlowSensor;1", "93560ff9-9a80-4afc-a00d-f5055042c048", testcaseName: "AccumulateTimeMinutes");

		bugHelper.Actor.TimedValues["result_AccumulateTimeMinutes"].GetLastValueDouble().Should().Be(5508 + 1);
	}

	[TestMethod]
	public void AccumulateTimeHours_Test()
	{
		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Param1", "discharge_air_flow_cumulative", "OPTION([dtmi:com:willowinc:DischargeAirFlowSensor;1])", CumulativeType.AccumulateTimeHours),
			new RuleParameter("Param3", "result_AccumulateTimeHours", "[discharge_air_flow_cumulative] + 1"),
			new RuleParameter("Param3", "result", "1")
		};

		rule.Parameters = parameters;

		var bugHelper = new BugHelper("RuleParameters", "Timeseries.csv", true);

		_ = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:DischargeAirFlowSensor;1", "93560ff9-9a80-4afc-a00d-f5055042c048", testcaseName: "AccumulateTimeHours");

		bugHelper.Actor.TimedValues["result_AccumulateTimeHours"].GetLastValueDouble().Should().BeGreaterThan(1);
	}

	/// <summary>
	/// People Count
	/// </summary>
	[TestMethod]
	public void UC1_Accumulate_Test()
	{
		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("People Count", "people_count_cumulative", "[dtmi:com:willowinc:PeopleCountSensor;1]", CumulativeType.Accumulate),
			new RuleParameter("Expression", "result", "people_count_cumulative > 50")
		};

		uc1Rule.Parameters = parameters;

		var bugHelper = new BugHelper("RuleParameters", "UC1_Timeseries.csv", assertOutputFiles: true);

		_ = bugHelper.GenerateInsightForPoint(uc1Rule, "dtmi:com:willowinc:PeopleCountSensor;1", "1ed922c2-741d-4e18-816a-175a329bed6c", testcaseName: "UC1_Accumulate", assertSimulation: false);

		bugHelper.Actor.TimedValues["result"].GetLastValueBool().Should().BeTrue();
	}

	[TestMethod]
	public void UC1_AccumulateTimeSeconds_Test()
	{
		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("People Count", "people_count_cumulative", "[dtmi:com:willowinc:PeopleCountSensor;1]", CumulativeType.AccumulateTimeSeconds),
			new RuleParameter("Expression", "result", "people_count_cumulative > 50"),
		};

		uc1Rule.Parameters = parameters;

		var bugHelper = new BugHelper("RuleParameters", "UC1_Timeseries.csv", assertOutputFiles: true);

		_ = bugHelper.GenerateInsightForPoint(uc1Rule, "dtmi:com:willowinc:PeopleCountSensor;1", "1ed922c2-741d-4e18-816a-175a329bed6c", testcaseName: "UC1_AccumulateTimeSeconds", assertSimulation: false);

		bugHelper.Actor.TimedValues["result"].GetLastValueBool().Should().BeTrue();
	}

	[TestMethod]
	public void UC1_AccumulateTimeMinutes_Test()
	{
		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("People Count", "people_count_cumulative", "[dtmi:com:willowinc:PeopleCountSensor;1]", CumulativeType.AccumulateTimeMinutes),
			new RuleParameter("Expression", "result", "people_count_cumulative > 50"),
		};

		uc1Rule.Parameters = parameters;

		var bugHelper = new BugHelper("RuleParameters", "UC1_Timeseries.csv", assertOutputFiles: true);

		_ = bugHelper.GenerateInsightForPoint(uc1Rule, "dtmi:com:willowinc:PeopleCountSensor;1", "1ed922c2-741d-4e18-816a-175a329bed6c", testcaseName: "UC1_AccumulateTimeMinutes", assertSimulation: false);

		bugHelper.Actor.TimedValues["result"].GetLastValueBool().Should().BeTrue();
	}

	[TestMethod]
	public void UC1_AccumulateTimeHours_Test()
	{
		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("People Count", "people_count_cumulative", "[dtmi:com:willowinc:PeopleCountSensor;1]", CumulativeType.AccumulateTimeHours),
			new RuleParameter("Expression", "result", "people_count_cumulative > 50"),
		};

		uc1Rule.Parameters = parameters;

		var bugHelper = new BugHelper("RuleParameters", "UC1_Timeseries.csv", assertOutputFiles: true);

		_ = bugHelper.GenerateInsightForPoint(uc1Rule, "dtmi:com:willowinc:PeopleCountSensor;1", "1ed922c2-741d-4e18-816a-175a329bed6c", testcaseName: "UC1_AccumulateTimeHours", assertSimulation: false);

		bugHelper.Actor.TimedValues["result"].GetLastValueBool().Should().BeTrue();
	}
}
