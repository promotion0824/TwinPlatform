using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs;

[TestClass]
public class Bug89697Tests
{
	[TestMethod]
	public async Task RuleInstanceShouldFailIfOneChildExpressionFailsAndTheOtherSucceeds()
	{
		//invalid expression
		var global = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("result", "result", "[dtmi:com:willowinc:SomeSensor;1] - [dtmi:com:willowinc:SomeSensor;1]")
			}
		};

		//valid expression
		var globalConst = new GlobalVariable()
		{
			VariableType = GlobalVariableType.Macro,
			Name = "mymacro1",
			Expression = new List<RuleParameter>()
			{
				new RuleParameter("result", "result", "6")
			}
		};

		var elements = new List<RuleUIElement>()
		{
			Fields.PercentageOfTime.With(0.1),
			Fields.OverHowManyHours.With(1),
		};

		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Expression", "result", "mymacro > mymacro1", "")
		};

		var rule = new Rule()
		{
			Id = "floor-unoccupied-when-overtime-air-requested",
			PrimaryModelId = "dtmi:com:willowinc:AirHandlingUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = elements
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "equipment");
		var sensor1 = new TwinOverride("dtmi:com:willowinc:OtherSensonr;1", "sensor1", trendId: "f9463069-6db6-465d-b3e1-96969ac30c0a");

		var sensors = new List<TwinOverride>()
		{
			sensor1
		};

		var harness = new ProcessorTestHarness();


		//put dependent one first in foreach
		harness.repositoryGlobalVariable.Data.Add(global);
		harness.repositoryGlobalVariable.Data.Add(globalConst);


		harness.OverrideCaches(rule, equipment, sensors);

		var ruleInstances = await harness.GenerateRuleInstances();

		var ri = ruleInstances[0];

		ri.Status.Should().Be(RuleInstanceStatus.BindingFailed);
	}
}
