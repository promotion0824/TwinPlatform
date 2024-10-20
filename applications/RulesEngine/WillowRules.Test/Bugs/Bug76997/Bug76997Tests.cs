using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using System;
using System.Threading.Tasks;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug76997Tests
{
	[TestMethod]
	public async Task Bug_76997_Should_Only_Count_Faulted_Insights()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTime.With(0.9),
			Fields.PercentageOfTimeOff.With(0.9)
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

		var harness = new ProcessorTestHarness();

		var equipment = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment");
		var sensor = new TwinOverride("dtmi:com:willowinc:ChilledWaterValvePositionActuator;1", "sensor1", "133f9221-477f-46e3-b3ed-312eac94dfe1");

		harness.OverrideCaches(rule, equipment, sensor);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug76997", "Timeseries.csv");

		(var insights, _, _) = await harness.ExecuteRules(filePath, endDate: DateTime.Parse("2023-01-27T03:31:24.9061845Z"));

		var insight = insights.First();

		var metadata = harness.repositoryRuleMetadata.Data[0];

		metadata.InsightsGenerated.Should().Be(0);
		insight.IsFaulty.Should().BeFalse();

		(insights, _, _) = await harness.ExecuteRules(filePath, startDate: DateTime.Parse("2023-01-27T03:31:24.9061845Z"), endDate: DateTime.Parse("2023-01-27T04:17:30.5998506Z"));

		insight = insights.First();

		metadata.InsightsGenerated.Should().Be(1);
		insight.IsFaulty.Should().BeTrue();

		(insights, _, _) = await harness.ExecuteRules(filePath, startDate: DateTime.Parse("2023-01-27T04:17:30.5998506Z"));

		insight = insights.First();

		metadata.InsightsGenerated.Should().Be(0);
		insight.IsFaulty.Should().BeFalse();
	}
}
