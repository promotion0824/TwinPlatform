using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item68997Tests
{
	[TestMethod]
	public void Item68997_Test()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTime.With(0.11833333)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Zone Temperature", "zone_temp", "OPTION([dtmi:com:willowinc:ZoneAirTemperatureSensor;1])"),
			new RuleParameter("Zone Temperature Setpoint", "zone_temp_sp", "OPTION([dtmi:com:willowinc:EffectiveCoolingZoneAirTemperatureSetpoint;1],[dtmi:com:willowinc:EffectiveZoneAirTemperatureSetpoint;1],[dtmi:com:willowinc:ZoneAirTemperatureSetpoint;1])"),
			new RuleParameter("Zone Temperature Setpoint Deviation", "zone_temp_sp_deviation", "[zone_temp] - [zone_temp_sp]"),
			new RuleParameter("Cooling", "cooling", "OPTION([dtmi:com:willowinc:ChilledWaterValvePositionActuator;1],[dtmi:com:willowinc:CoolingLevelActuator;1],[dtmi:com:willowinc:CoolingLevelSensor;1] > 0.1)"),
			new RuleParameter("Overcooling", "result", "([zone_temp_sp_deviation] < -1.5) & [cooling]"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-overcooling-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item68997", "Timeseries.csv");

		var sensors = new List<TwinOverride>()
		{
			new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "ZAT-1","f9463069-6db6-465d-b3e1-96969ac30c0a"),
			new TwinOverride("dtmi:com:willowinc:EffectiveCoolingZoneAirTemperatureSetpoint;1", "ZST-1", "66a15104-dbd1-4b43-85b6-28841feee531"),
			new TwinOverride("dtmi:com:willowinc:ChilledWaterValvePositionActuator;1", "CWVP-1", "0baadc31-c684-44aa-95a3-68fc5f18eb7f"),
			new TwinOverride("dtmi:com:willowinc:SomeOtherModel;1", "OTH-1", "52500934-6a77-4d70-80cb-f30729cc0a43")
		};

		var equipment = new TwinOverride(rule.PrimaryModelId, "equipment");

		var insight = bugHelper.GenerateInsightForPoint(rule, equipment, sensors, assertSimulation: false);

		//insight.Should().NotBeNull();
		//
		//insight!.Occurrences.Count(v => v.IsFaulted).Should().Be(1);
	}
}
