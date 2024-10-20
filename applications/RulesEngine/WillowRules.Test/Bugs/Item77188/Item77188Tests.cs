using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item77188Tests
{
	[TestMethod]
	public void Compression_Test_One()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(5),
			Fields.MaxTrigger.With(45),
			Fields.PercentageOfTime.With(0.08333333333333333),
			Fields.OverHowManyHours.With(12)
		};

		var parameters = new List<RuleParameter>()
		{
			 new RuleParameter("Zone Air Temperature Sensor", "result", "OPTION([dtmi:com:willowinc:ZoneAirTemperatureSensor;1])")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-out-of-range-metric",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var bugHelper = new BugHelper("Item77188", "Timeseries.csv");

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");
		var sensor = new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "sensor", trendId: Guid.NewGuid().ToString(), connectorId: "219bf492-48d2-4d5c-a1c6-73f7b599455c", externalId: "1220293AV101");

		bugHelper.GenerateInsightForPoint(rule, equipment, new List<TwinOverride>() { sensor });
	}
}
