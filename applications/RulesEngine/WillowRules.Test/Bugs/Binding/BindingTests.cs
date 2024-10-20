using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs.Binding
{
	[TestClass]
	public class BindingTests
	{
		[TestMethod]
		public async Task ShouldBindMultipleInstancesForEquipmentAcrossMultipleRulesWithSameInheritance()
		{
			var elements = new List<RuleUIElement>()
			{
				Fields.OverHowManyHours.With(12),
				Fields.PercentageOfTime.With(0.11833333)
			};

			var parameters = new List<RuleParameter>()
			{
				new RuleParameter("mytwin", "mytwin", "this"),
				new RuleParameter("My Sensor", "sensor", "IF(mytwin.myprop > 10, 1, 0)"),
			};

			var rule1 = new Rule()
			{
				Id = "r1",
				PrimaryModelId = "dtmi:com:willowinc:EquipmentBase;1",
				TemplateId = RuleTemplateAnyFault.ID,
				Parameters = parameters,
				Elements = elements
			};

			var rule2 = new Rule()
			{
				Id = "r2",
				PrimaryModelId = "dtmi:com:willowinc:Equipment;1",
				TemplateId = RuleTemplateAnyFault.ID,
				Parameters = parameters,
				Elements = elements
			};

			
			var equipment = new TwinOverride("dtmi:com:willowinc:Equipment;1", "equipment");

			var sensors = new List<TwinOverride>()
			{
				new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "TwinId", trendId: Guid.NewGuid().ToString()),
			};

			var harness = new ProcessorTestHarness();

			await harness.AddToModelGraph(new ModelData()
			{
				Id = "dtmi:com:willowinc:EquipmentBase;1",
				DtdlModel = new DtdlModel()
				{
					extends = new StringList()
				}
			});

			await harness.AddToModelGraph(equipment, "dtmi:com:willowinc:EquipmentBase;1");

			harness.OverrideCaches(rule1, equipment, sensors);
			harness.OverrideCaches(rule2, equipment, sensors);

			var instances = await harness.GenerateRuleInstances();

			instances.Count.Should().Be(2);

			instances.Any(v => v.RuleId == "r1").Should().BeTrue();
			instances.Any(v => v.RuleId == "r2").Should().BeTrue();
			instances.All(v => v.EquipmentId == "equipment").Should().BeTrue();
		}

		[TestMethod]
		public async Task CanAccessTwinPropertyFromVariable()
		{
			var elements = new List<RuleUIElement>()
			{
				Fields.OverHowManyHours.With(12),
				Fields.PercentageOfTime.With(0.11833333)
			};

			var parameters = new List<RuleParameter>()
			{
				new RuleParameter("mytwin", "mytwin", "this"),
				new RuleParameter("My Sensor", "sensor", "IF(mytwin.myprop > 10, 1, 0)"),
			};

			var rule = new Rule()
			{
				Id = "terminal-unit-overcooling-metric",
				PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
				TemplateId = RuleTemplateAnyFault.ID,
				Parameters = parameters,
				Elements = elements
			};

			var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment", contents: new Dictionary<string, object>()
			{
				["myprop"] = 50
			});

			var sensors = new List<TwinOverride>()
			{
				new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "TwinId", trendId: Guid.NewGuid().ToString()),
			};

			var harness = new ProcessorTestHarness();

			harness.OverrideCaches(rule, equipment, sensors);

			var instances = await harness.GenerateRuleInstances();

			var instance = instances.Single();

			instance.RuleParametersBound.First(v => v.FieldId == "result").PointExpression.Serialize().Should().Be("FAILED(\"Missing 'result' expression\")");

			instance.Status.Should().Be(RuleInstanceStatus.BindingFailed);
		}

		[TestMethod]
		public async Task ShouldFailWithoutResultField()
		{
			var elements = new List<RuleUIElement>()
			{
				Fields.OverHowManyHours.With(12),
				Fields.PercentageOfTime.With(0.11833333)
			};

			var parameters = new List<RuleParameter>()
			{
				new RuleParameter("My Sensor", "sensor", "0"),
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

			var instances = await harness.GenerateRuleInstances();

			var instance = instances.Single();

			instance.RuleParametersBound.First(v => v.FieldId == "result").PointExpression.Serialize().Should().Be("FAILED(\"Missing 'result' expression\")");

			instance.Status.Should().Be(RuleInstanceStatus.BindingFailed);
		}

		[TestMethod]
		public async Task DescriptionMustBindToTwinProperties_87696()
		{
			var elements = new List<RuleUIElement>()
			{
				Fields.OverHowManyHours.With(12),
				Fields.PercentageOfTime.With(0.11833333)
			};

			var parameters = new List<RuleParameter>()
			{
				new RuleParameter("Controller", "controller", "this.ctrl", "0"),
				new RuleParameter("Zone Temperature Setpoint Deviation", "result", "0"),
			};

			var twinContent = new Dictionary<string, object>()
			{
				["ctrl"] = "equipment-001",
				["fanSpeed"] = 100
			};

			string description = "Controller is {controller}. Valid {this.fanSpeed}. Invalid {this.invalidProp}. Optional {OPTION(this.invalidProp, 10)}. Left alone {result}. Invalid {10 * 10}.";

			var rule = new Rule()
			{
				Id = "terminal-unit-overcooling-metric",
				PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
				TemplateId = RuleTemplateAnyFault.ID,
				Parameters = parameters,
				Description = description,
				Elements = elements
			};

			var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment", contents: twinContent);

			var sensors = new List<TwinOverride>()
			{
				new TwinOverride("dtmi:com:willowinc:ZoneAirTemperatureSensor;1", "TwinId", trendId: Guid.NewGuid().ToString()),
			};

			var harness = new ProcessorTestHarness();

			harness.OverrideCaches(rule, equipment, sensors);

			var instances = await harness.GenerateRuleInstances();

			var instance = instances.Single();

			instance.Description.Should().Be("Controller is equipment-001. Valid 100. Invalid {this.invalidProp}. Optional 10. Left alone {result}. Invalid {10 * 10}.");
		}
	}
}
