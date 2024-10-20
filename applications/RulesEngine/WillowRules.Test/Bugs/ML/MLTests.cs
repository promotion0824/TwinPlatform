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
public class MLTests
{
	[TestMethod]
	public async Task PredictShouldExecute()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.MinTrigger.With(12),
			Fields.MaxTrigger.With(35),
			Fields.PercentageOfTime.With(0.01)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("setpoint", "setpoint", "OPTION([dtmi:com:willowinc:Setpoint;1])"),
			new RuleParameter("prediction", "prediction", "predict_v1([setpoint], [setpoint] + 1)"),//the model has 2 params
			new RuleParameter("Zone Temperature Setpoint", "result", "prediction > 0"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-setpoint-out-of-range",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		var dataPath = System.IO.Path.Combine(Environment.CurrentDirectory, "Data", "model.onnx");
		var data = System.IO.File.ReadAllBytes(dataPath);

		var model = harness.mlService.FillModel(new MLModel()
		{
			Id = "m1",
			FullName = "predict_v1",
			ModelData = data
		});

		harness.repositoryMLModel.Data.Add(model);

		harness.OverrideCaches(rule, "equipment", "dtmi:com:willowinc:Setpoint;1", "f9463069-6db6-465d-b3e1-96969ac30c0a");

		await harness.GenerateRuleInstances();

		var filePath = BugHelper.GetFullDataPath("ML", "Timeseries.csv");

		(var insights, var actorsList, _) = await harness.ExecuteRules(filePath);

		var actor = actorsList.Single();

		actor.TimedValues["prediction"].Points.Last().ValueDouble.Should().BeGreaterThan(0);//the mock sums all the inputs
		harness.mlService.lastModelName.Should().Be("predict_v1");
	}

	[TestMethod]
	public async Task PredictShouldFailValidation()
	{
		var elements = new List<RuleUIElement>()
		{
			Fields.OverHowManyHours.With(1),
			Fields.MinTrigger.With(12),
			Fields.MaxTrigger.With(35),
			Fields.PercentageOfTime.With(0.01)
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("setpoint", "setpoint", "OPTION([dtmi:com:willowinc:Setpoint;1])"),
			new RuleParameter("prediction", "prediction", "predict_v1(1)"),
			new RuleParameter("Zone Temperature Setpoint", "result", "prediction > 0"),
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-air-temp-setpoint-out-of-range",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyHysteresis.ID,
			Parameters = parameters,
			Elements = elements
		};

		var harness = new ProcessorTestHarness();

		harness.repositoryMLModel.Data.Add(new MLModel(
			"m1",
			"predict_v1",
			"predict",
			"v1",
			"",
			new byte[0])
		{
			ExtensionData = new MLModelExtensionData()
			{
				InputParams = new MLModelParam[]
				{
					new MLModelParam() { Name = "p1" },
					new MLModelParam() { Name = "p2" },
					new MLModelParam() { Name = "p3" }
				}
			}
		});

		harness.OverrideCaches(rule, "equipment", "dtmi:com:willowinc:Setpoint;1", "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var ris = await harness.GenerateRuleInstances();
		var ruleInstance = ris.Single();

		ruleInstance.RuleParametersBound[1].PointExpression.ToString().Should().Be("FAILED('Function 'predict_v1' parameter count mismatch source count 1 and function count 3',predict_v1(1))");
	}
}
