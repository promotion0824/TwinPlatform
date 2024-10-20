using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs.Other
{
	[TestClass]
	public class Bug76899Tests
	{
		[TestMethod]
		public async Task Metadata_Should_Contain_ScanError()
		{
			var elements = new List<RuleUIElement>()
			{
				Fields.OverHowManyHours.With(12),
				Fields.PercentageOfTime.With(0.11833333)
			};

			var parameters = new List<RuleParameter>()
			{
				new RuleParameter("Zone Temperature", "zone_temp", "OPTION([dtmi:com:willowinc:ZoneAirTemperatureSensor;1]"),
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
				new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "TwinId", trendId: Guid.NewGuid().ToString()),
			};

			var harness = new ProcessorTestHarness();

			harness.OverrideCaches(rule, equipment, sensors);

			try
			{
				var instances = await harness.GenerateRuleInstances();
			}
			catch (Exception)
			{
			}
			finally
			{
				var metadata = harness.repositoryRuleMetadata.Data[0];

				metadata.ScanError.Should().Be("Failed to parse expressions. Missing close parenthesis on call to OPTION");
			}
		}
	}
}
