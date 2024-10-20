using FluentAssertions;
using Kusto.Cloud.Platform.Utils;
using Microsoft.Extensions.Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug76191Tests
{
	[TestMethod]
	public void Bug76191_InsightShouldUpdateIfActorEmpty()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTime.With(0.9)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Chilled Water Valve", "CHWV", "OPTION([dtmi:com:willowinc:ChilledWaterValvePositionActuator;1],[dtmi:com:willowinc:ChilledWaterValvePositionSensor;1])"),
			new RuleParameter("AHU CHWV Over Design Operation", "result", "[CHWV] > 90"),
		};

		var rule = new Rule()
		{
			Id = "ahu-chwv-over-design-operation",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Bug76191", "Timeseries.csv");

		//sensor went green after the 27th
		DateTime date = new DateTime(2023, 1, 29);

		bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:ChilledWaterValvePositionActuator;1", "133f9221-477f-46e3-b3ed-312eac94dfe1", endDate: date);

		//now clear out actor
		bugHelper.Actors.Clear();

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:ChilledWaterValvePositionActuator;1", "133f9221-477f-46e3-b3ed-312eac94dfe1", startDate: date);

		var lastOccurrence = insight.Occurrences.LastOrDefault();
		lastOccurrence.Should().NotBeNull();
		//confirms that it has data up to the end of the timeseries data
		lastOccurrence!.Ended.Should().BeAfter(new DateTime(2023, 2, 2).ToUtc());
	}

	[TestMethod]
	public void Bug76191_Test()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTime.With(0.9)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Chilled Water Valve", "CHWV", "OPTION([dtmi:com:willowinc:ChilledWaterValvePositionActuator;1],[dtmi:com:willowinc:ChilledWaterValvePositionSensor;1])"),
			new RuleParameter("AHU CHWV Over Design Operation", "result", "[CHWV] > 90"),
		};

		var rule = new Rule()
		{
			Id = "ahu-chwv-over-design-operation",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Bug76191", "Timeseries.csv");

		var insight = bugHelper.GenerateInsightForPoint(rule, "dtmi:com:willowinc:ChilledWaterValvePositionActuator;1", "133f9221-477f-46e3-b3ed-312eac94dfe1");

		var faultedOccurrence = insight!.Occurrences.FirstOrDefault(v => v.IsFaulted);
		faultedOccurrence.Should().NotBeNull();
		faultedOccurrence!.Started.Should().BeAfter(new DateTime(2023, 1, 26, 19, 0, 0, DateTimeKind.Utc));
		faultedOccurrence!.Ended.Should().BeBefore(new DateTime(2023, 1, 28, 0, 0, 0, DateTimeKind.Utc));
	}
}
