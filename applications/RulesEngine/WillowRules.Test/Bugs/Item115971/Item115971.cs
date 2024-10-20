using FluentAssertions;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Item115971
{
	[TestMethod]
	public async Task AutoVariableAndSelfReferencingVariableShouldWorkV1()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.3),
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTimeOff.With(0.25)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("expr1", "expr1", "AVERAGE([dtmi:com:willowinc:CompressorEnteringRefrigerantStaticPressureSensor;1] + expr1, 1d)"),
			new RuleParameter("Expression", "result", "expr1 = 0")
		};

		var rule = new Rule()
		{
			Id = "low-suction-pressure-at-compressor-rack",
			PrimaryModelId = "dtmi:com:willowinc:RefrigerationCompressorGroup;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:RefrigerationCompressorGroup;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:CompressorEnteringRefrigerantStaticPressureSensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();
		harness.OverrideCaches(rule, equipment, sensors);

		var instances = await harness.GenerateRuleInstances();
		var instance = instances.Single();
		Assert.IsNotNull(instance);

		instance.RuleParametersBound.Count.Should().Be(3);
		var p1 = instance.RuleParametersBound[0];
		var p2 = instance.RuleParametersBound[1];

		p1.PointExpression.Serialize().Should().Be($"sensor1 + {p1.FieldId}");
		p2.PointExpression.Serialize().Should().Be($"AVERAGE({p1.FieldId}, 1[d])");
	}

	[TestMethod]
	public async Task AutoVariableAndSelfReferencingVariableShouldWorkV2()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.3),
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTimeOff.With(0.25)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("expr1", "expr1", "AVERAGE([dtmi:com:willowinc:CompressorEnteringRefrigerantStaticPressureSensor;1] + 1, 1d) + expr1"),
			new RuleParameter("Expression", "result", "expr1 = 0")
		};

		var rule = new Rule()
		{
			Id = "low-suction-pressure-at-compressor-rack",
			PrimaryModelId = "dtmi:com:willowinc:RefrigerationCompressorGroup;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:RefrigerationCompressorGroup;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:CompressorEnteringRefrigerantStaticPressureSensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();
		harness.OverrideCaches(rule, equipment, sensors);

		var instances = await harness.GenerateRuleInstances();
		var instance = instances.Single();
		Assert.IsNotNull(instance);

		instance.RuleParametersBound.Count.Should().Be(3);
		var p1 = instance.RuleParametersBound[0];
		var p2 = instance.RuleParametersBound[1];

		p1.PointExpression.Serialize().Should().Be($"sensor1 + 1");
		p2.PointExpression.Serialize().Should().Be($"AVERAGE({p1.FieldId}, 1[d]) + expr1");
	}

	[TestMethod]
	public async Task ExpansionShouldIncludeDynamicParameter()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.3),
			Fields.OverHowManyHours.With(12),
			Fields.PercentageOfTimeOff.With(0.25)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Suction Pressure", "suction_pressure", "[dtmi:com:willowinc:CompressorEnteringRefrigerantStaticPressureSensor;1]", "kWh"),
			new RuleParameter("Avg Suction Pressure Over 180 Days", "avg_suction_pressure", "AVERAGE(suction_pressure + 1, 180d)"),
			new RuleParameter("Expression", "result", "suction_pressure < (avg_suction_pressure * 0.75)")
		};

		var rule = new Rule()
		{
			Id = "low-suction-pressure-at-compressor-rack",
			PrimaryModelId = "dtmi:com:willowinc:RefrigerationCompressorGroup;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:RefrigerationCompressorGroup;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:CompressorEnteringRefrigerantStaticPressureSensor;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();
		harness.OverrideCaches(rule, equipment, sensors);

		var instances = await harness.GenerateRuleInstances();
		var instance = instances.Single();
		Assert.IsNotNull(instance);

		instance.RuleParametersBound.Count.Should().Be(4);
		instance.RuleParametersBound.Where(i => i.IsAutoGenerated).Count().Should().Be(1);
	}

	[TestMethod]
	public async Task DynamicParameterShouldNotBeImpactScore()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.5)
		};

		var parameters = new List<RuleParameter>()
		{
			new("Expression", "result", "NOW.HOUR > 5")
		};

		var scores = new List<RuleParameter>()
		{
			new RuleParameter("ImpactScore", "impactScore", "TIMER(result)"),
		};

		var rule = new Rule()
		{
			Id = "food-display-case-evaporator-fan-over-design",
			PrimaryModelId = "dtmi:com:willowinc:RefrigeratedFoodDisplayCase;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			ImpactScores = scores,
			Elements = elements
		};

		var sensors = new List<TwinOverride>()
		{
			new TwinOverride(
				"dtmi:com:willowinc:FanCurrentSensor;1",
				"944-CKTS-BC-C03b-MD-MEAT-EVAP-FAN-AMPS",
				trendId: "50158491-a62d-4b45-98be-f505b7181f0d",
				externalId: "PNTQCtHWKrdfFUkQNcyoXsEKs",
				connectorId: "00000000-35c5-4415-a4b3-7b798d0568e8"
				)
		};

		var equipment = new TwinOverride(rule.PrimaryModelId, "equipment");

		var harness = new ProcessorTestHarness();
		harness.OverrideCaches(rule, equipment, sensors);

		var instances = await harness.GenerateRuleInstances();
		var instance = instances.Single();
		instance.Should().NotBeNull();

		var lastRP = instance.RuleParametersBound.Last();
		var firstIS = instance.RuleImpactScoresBound.First();
		instance.RuleParametersBound.Count.Should().Be(2);
		instance.RuleImpactScoresBound.Count.Should().Be(1);
		instance.RuleImpactScoresBound.Count(rp => rp.IsAutoGenerated).Should().Be(0);
		firstIS.PointExpression.Should().BeOfType<TokenExpressionVariableAccess>();
		firstIS.PointExpression.As<TokenExpressionVariableAccess>().VariableName.Should().Be(lastRP.FieldId);
	}

	[TestMethod]
	public void TimerFunctionBasic()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.5)
		};

		var parameters = new List<RuleParameter>()
		{
			new("Condition1", "condition1", "[dtmi:com:willowinc:FanCurrentSensor;1] > 1"),
			new("Expression1", "expression1", "TIMER([dtmi:com:willowinc:FanCurrentSensor;1] > 1, h)"),
			new("Expression2", "result", "IF(condition1, IFNAN(result,0) + DELTA_TIME_S / 60 / 60, 0)")
		};

		var rule = new Rule()
		{
			Id = "food-display-case-evaporator-fan-over-design",
			PrimaryModelId = "dtmi:com:willowinc:RefrigeratedFoodDisplayCase;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var sensors = new List<TwinOverride>()
		{
			new TwinOverride(
				"dtmi:com:willowinc:FanCurrentSensor;1",
				"944-CKTS-BC-C03b-MD-MEAT-EVAP-FAN-AMPS",
				trendId: "50158491-a62d-4b45-98be-f505b7181f0d",
				externalId: "PNTQCtHWKrdfFUkQNcyoXsEKs",
				connectorId: "00000000-35c5-4415-a4b3-7b798d0568e8"
				)
		};

		var equipment = new TwinOverride(rule.PrimaryModelId, "equipment");

		var bugHelper = new BugHelper("Item115971", "Timeseries.csv");
		_ = bugHelper.GenerateInsightForPoint(rule, equipment!, sensors!, limitUntracked: false, assertSimulation: false, applyLimits: false, enableCompression: false);

		bugHelper.Actor.Should().NotBeNull();
		bugHelper.RuleInstance.Should().NotBeNull();

		bugHelper.RuleInstance!.RuleParametersBound.Where(p => p.PointExpression is TokenExpressionTernary).Count().Should().BeGreaterThan(0);

		var autoParam = bugHelper.RuleInstance.RuleParametersBound.Single(rp => rp.IsAutoGenerated);
		autoParam.Should().NotBeNull();

		var autoParamTS = bugHelper.Actor!.TimedValues[autoParam.FieldId];

		var resultTS = bugHelper.Actor!.TimedValues["result"];

		autoParamTS.Points.Count().Should().BeGreaterThan(0);
		autoParamTS.Points.All(v => v.NumericValue > 0).Should().BeTrue();
		autoParamTS.Points.Count().Should().BeGreaterThan(0);
		autoParamTS.Points.Should().BeEquivalentTo(resultTS.Points);
	}

	[TestMethod]
	public void TimerFunctionBasic1()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.5),
			Fields.OverHowManyHours.With(1),
			Fields.PercentageOfTimeOff.With(0.5)
		};

		var parameters = new List<RuleParameter>()
		{
			new("Expression1", "expression1", "TIMER([dtmi:com:willowinc:FanCurrentSensor;1] > 1, d)"),
			new("Expression2", "result", "expression1")
		};

		var rule = new Rule()
		{
			Id = "food-display-case-evaporator-fan-over-design",
			PrimaryModelId = "dtmi:com:willowinc:RefrigeratedFoodDisplayCase;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var sensors = new List<TwinOverride>()
		{
			new TwinOverride(
				"dtmi:com:willowinc:FanCurrentSensor;1",
				"944-CKTS-BC-C03b-MD-MEAT-EVAP-FAN-AMPS",
				trendId: "50158491-a62d-4b45-98be-f505b7181f0d",
				externalId: "PNTQCtHWKrdfFUkQNcyoXsEKs",
				connectorId: "00000000-35c5-4415-a4b3-7b798d0568e8"
				)
		};

		var equipment = new TwinOverride(rule.PrimaryModelId, "equipment");

		var bugHelper = new BugHelper("Item115971", "Timeseries.csv");
		_ = bugHelper.GenerateInsightForPoint(rule, equipment!, sensors!, limitUntracked: false, assertSimulation: false, applyLimits: false, enableCompression: false);

		bugHelper.Actor.Should().NotBeNull();
		bugHelper.RuleInstance.Should().NotBeNull();

		var param1 = bugHelper.RuleInstance!.RuleParametersBound[0];
		var param2 = bugHelper.RuleInstance!.RuleParametersBound[1];

		param1.IsAutoGenerated.Should().BeTrue();
		param1.PointExpression.Serialize().Should().Be($"IF([944-CKTS-BC-C03b-MD-MEAT-EVAP-FAN-AMPS] > 1, IFNAN({param1.FieldId},0) + ((DELTA_TIME_S / 60) / 60) / 24, 0)");
		param2.IsAutoGenerated.Should().BeFalse();
		param2.PointExpression.Serialize().Should().Be(param1.FieldId);
	}
}

