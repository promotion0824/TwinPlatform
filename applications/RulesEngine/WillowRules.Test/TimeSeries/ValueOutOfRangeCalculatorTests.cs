using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Willow.Expressions;
using Willow.Rules.Model;

namespace Willow.Rules.Test;

[TestClass]
public class ValueOutOfRangeCalculatorTests
{
	[DataRow("model", "%", 100, false)]
	[DataRow("model", "%", 200, true)]
	[DataRow("SignalStrengthSensor;1", "%", 200, false)]
	[DataRow("model", "cfm", 1000, false)]
	[DataRow("CO2AirQualitySensor;1", "ppm", 1000, false)]
	[DataRow("CO2AirQualitySensor;1", "ppm", 20000, true)]
	[DataRow("model", "kWh", 8473, false)]
	[DataRow("model", "deg d'angle", 178, false)]
	[DataRow("model", "Btu / lb", 2500, false)]
	[DataRow("model", "KPA", 351, true)]
	[TestMethod]
	public void RangeChecks(string model, string unit, double value, bool result)
	{
		Console.WriteLine(model + " " + unit + " " + value);
		Unit.IsOutOfRange(unit, model, value).result.Should().Be(result);
	}
}
