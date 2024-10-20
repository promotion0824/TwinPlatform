using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using WillowRules.Extensions;
using WillowRules.Utils;

namespace Willow.Rules.Test;

[TestClass]
public class UtilityTests
{
	[TestMethod]
	public void TrimModelIdMustWork()
	{
		var modelId = "dtmi:com:willowinc:TerminalUnit;1".TrimModelId();

		modelId.Should().Be("TerminalUnit");
	}

	[TestMethod]
	public void TestGuidUtilityConsistency()
	{
		var modelId = "dtmi:com:willowinc:TerminalUnit;1";
		var ruleId = "rule1";
		var expressionCollection = new List<string>
		{
			"(32 + [WIL-220FA-FCU-L04-01-ZoneAirTempSensor-134AI11] * 1.8) - (32 + [WIL-220FA-FCU-L04-01-ZoneAirTempSp-134AV43] * 1.8)",
			"FAHRENHEIT(FAILED(\"No twin matches found\",[dtmi:com:willowinc:ZoneAirTemperatureSensor;1])) - FAHRENHEIT(FAILED(\"No twin matches found\",[dtmi:com:willowinc:ZoneAirTemperatureSetpoint;1]))"
		};

		var output1 = ($"{modelId}_{ruleId}_{GuidUtility.Create(string.Join("", expressionCollection))}");
		var output2 = ($"{modelId}_{ruleId}_{GuidUtility.Create(string.Join("", expressionCollection))}");

		output1.Should().Be(output2);

		expressionCollection = new List<string>
		{
			"[WIL-220FA-FCU-L04-01-ZoneAirTempSensor-134AI11]",
			"[WIL-220FA-FCU-L04-01-ZoneAirTempSensor-134AI11]"
		};

		var output3 = ($"{modelId}_{ruleId}_{GuidUtility.Create(string.Join("", expressionCollection))}");
		var output4 = ($"{modelId}_{ruleId}_{GuidUtility.Create(string.Join("", expressionCollection))}");

		output3.Should().NotBe(output2);
		output4.Should().Be(output3);

		modelId = "dtmi:com:ActiveElectricalPowerSensor;1";

		var output5 = ($"{modelId}_{ruleId}_{GuidUtility.Create(string.Join("", expressionCollection))}");
		var output6 = ($"{modelId}_{ruleId}_{GuidUtility.Create(string.Join("", expressionCollection))}");

		output5.Should().NotBe(output2);
		output5.Should().NotBe(output3);
		output5.Should().Be(output6);
	}
}
