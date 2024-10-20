using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class ConcurrencyTests
{
	[TestMethod]
	public async Task CalculatedValuesOrder()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Zone Temperature", "zone_temp", "OPTION([dtmi:com:willowinc:ZoneAirTemperatureSensor;1])"),
			new RuleParameter("Zone Temperature Setpoint", "zone_temp_sp", "OPTION([dtmi:com:willowinc:EffectiveCoolingZoneAirTemperatureSetpoint;1])"),
			new RuleParameter("Zone Temperature Setpoint Deviation", "result", "zone_temp + zone_temp_sp", CumulativeType.Accumulate),
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
			new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "ZAT-1",  "f9463069-6db6-465d-b3e1-96969ac30c0a"),
			new TwinOverride("dtmi:com:willowinc:EffectiveCoolingZoneAirTemperatureSetpoint;1", "ZST-1",  "66a15104-dbd1-4b43-85b6-28841feee531")
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Concurrency", "CalculatedValuesOrder.csv");

		(_, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single();

		var values = actor.TimedValues["result"].Points.ToList();

		values[0].ValueDouble.Should().Be(7);
		values[1].ValueDouble.Should().Be(16);
		values[2].ValueDouble.Should().Be(27);
		values[3].ValueDouble.Should().Be(40);
		values[4].ValueDouble.Should().Be(55);
		values[5].ValueDouble.Should().Be(72);
	}
}
