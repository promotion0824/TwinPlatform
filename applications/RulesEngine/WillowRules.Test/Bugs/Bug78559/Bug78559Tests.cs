using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug78559Tests
{
	[TestMethod]
	public async Task Bug_78559_ShouldNotWriteDuplicateImpactScore()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.MinTrigger.With(-10.0),
			Fields.MaxTrigger.With(10.0),
			Fields.PercentageOfTime.With(0.25),
			Fields.OverHowManyHours.With(3)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Chiller Status", "CH_STS", "IF([dtmi:com:willowinc:RunSensor;1]>0,1,0)"),
			new RuleParameter("Condenser Entering Water Temperature", "COND_EWT", "[dtmi:com:willowinc:EnteringCondenserWaterTemperatureSensor;1]"),
			new RuleParameter("Condenser Refrigerant Temperature", "COND_REF_T", "OPTION([dtmi:com:willowinc:CompressorLeavingRefrigerantTemperatureSensor;1], [dtmi:com:willowinc:CondenserWaterTemperatureSensor;1])"),
			new RuleParameter("Temperature Differential", "DIFF", "COND_REF_T - COND_EWT"),
			new RuleParameter("Expression", "result", "IF(CH_STS,DIFF,0)"),
		};

		var impactScores = new List<RuleParameter>()
		{
			new RuleParameter("Total Energy Impact", "total_energy_to_date", "AREA_OUTSIDE", "kWh"),
			new RuleParameter("Daily Avoidable Energy", "daily_avoidable_energy", "total_energy_to_date / TIME", "kWh"),
			new RuleParameter("Total Cost to Date", "total_cost_to_date", "total_energy_to_date * 0.18", "USD"),
			new RuleParameter("Daily Avoidable Cost", "daily_avoidable_cost", "total_cost_to_date / TIME", "USD"),
			new RuleParameter("Priority", "priority_impact", "80"),
			new RuleParameter("Reliability impact", "reliability_impact", "DELTA(TIME,7d)/60/60/24")
		};

		var rule = new Rule()
		{
			Id = "chiller-high-approach",
			PrimaryModelId = "dtmi:com:willowinc:Chiller;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			ImpactScores = impactScores,
			Elements = elements
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:Chiller;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:CompressorLeavingRefrigerantTemperatureSensor;1", "sensor1", trendId: "5bfcbc0a-c605-45eb-a7b5-838a4af50608");
		var sensor2 = new TwinOverride("dtmi:com:willowinc:EnteringCondenserWaterTemperatureSensor;1", "sensor2", trendId: "75ef3df3-dcc6-49cc-aa6d-e17fa44f162a");
		var sensor3 = new TwinOverride("dtmi:com:willowinc:RunSensor;1", "sensor3", trendId: "316e2987-26a5-4243-8dd1-ee5931a2d790");

		var sensors = new List<TwinOverride>()
		{
			sensor1,
			sensor2,
			sensor3
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("Bug78559", "Timeseries.csv");


		var endDate = DateTime.Parse("2023-04-25T22:16:21.6578017Z");

		(var insights, var actors, _) = await harness.ExecuteRules(filePath, endDate: endDate, assertSimulation: false);

		var insight = insights.Single();

		var scores = insight.ImpactScores.ToList();

		scores.Count.Should().Be(6);
		scores.Count(v => v.Name == "Total Energy Impact").Should().Be(1);
		scores.Count(v => v.FieldId == "total_energy_to_date").Should().Be(1);

		impactScores[0].Name = "Total Energy to Date";

		await harness.GenerateRuleInstances();

		(insights, _, _) = await harness.ExecuteRules(filePath, startDate: endDate.AddMinutes(15), assertSimulation: false);

		insight = insights.Single();

		scores = insight.ImpactScores.ToList();

		scores.Count.Should().Be(6);
		scores.Count(v => v.Name == "Total Energy Impact").Should().Be(0);
		scores.Count(v => v.Name == "Total Energy to Date").Should().Be(1);
		scores.Count(v => v.FieldId == "total_energy_to_date").Should().Be(1);

	}
}
