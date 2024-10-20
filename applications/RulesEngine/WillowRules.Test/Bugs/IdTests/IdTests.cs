using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class IdTests
{
	[TestMethod]
	public async Task RawDataWithoutTrendIdMustWork()
	{
		var elements = new List<RuleUIElement>()
	{
		Fields.OverHowManyHours.With(12),
		Fields.PercentageOfTime.With(0.11833333)
	};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Zone Temperature", "zone_temp", "OPTION([dtmi:com:willowinc:ZoneAirTemperatureSensor;1])"),
			new RuleParameter("Zone Temperature Setpoint Deviation", "result", "zone_temp"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-overcooling-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");

		var sensors = new List<TwinOverride>()
		{
			//trend id isirrelevant as the timeseries data will not have one, but still provide the id for rule instance creation
			new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "TwinId", trendId: Guid.NewGuid().ToString(), connectorId: "df8e01bc-9340-4d31-a5f9-a5cf059152ae", externalId: "3036579AV7934"),
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("IdTests", "NotTrendId.csv");

		(_, _, var timeSeriesList) = await harness.ExecuteRules(filePath);

		timeSeriesList.Count.Should().Be(1);

		var timeSeries = timeSeriesList[0];

		timeSeries.Id.Should().NotBeNullOrEmpty();
	}
}
