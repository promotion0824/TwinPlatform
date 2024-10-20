using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Bugs.Item1319507;

[TestClass]
public class Item1319507Tests
{
	[TestMethod]
	public async Task FindAllWithoutProps()
	{
		var parameters = new List<RuleParameter>()
		{
			new("Expression", "result", "FINDALL([dtmi:com:willowinc:TerminalUnit;1] & UNDER(this))")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-hot",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters
		};

		var harness = new ProcessorTestHarness();

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "e1");
		var sensor = new TwinOverride("dtmi:com:willowinc:Fan;1", "s1");

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>() { sensor });

		var instances = await harness.GenerateRuleInstances();

		harness.twinService.LastADTQuery.Should().Be("SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId = 'e1' AND IS_OF_MODEL(twin,'dtmi:com:willowinc:TerminalUnit;1')");

		//the mock just returns all the twins in its list
		instances[0].RuleParametersBound.First().PointExpression.Serialize().Should().Be("{s1,[e1]}");
	}

	[TestMethod]
	public async Task UNDERMustUsePropertyFromThis()
	{
		var parameters = new List<RuleParameter>()
		{
			new("Expression", "result", "FINDALL([dtmi:com:willowinc:TerminalUnit;1] & UNDER(this.someid))")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-hot",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters
		};

		var harness = new ProcessorTestHarness();


		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "e1", contents: new Dictionary<string, object>()
		{
			["someid"] = "M1"
		});

		var sensor = new TwinOverride("dtmi:com:willowinc:Fan;1", "s1");

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>() { sensor });

		var instances = await harness.GenerateRuleInstances();

		harness.twinService.LastADTQuery.Should().Be("SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId = 'M1' AND IS_OF_MODEL(twin,'dtmi:com:willowinc:TerminalUnit;1')");

		//the mock just returns all the twins in its list
		instances[0].RuleParametersBound.First().PointExpression.Serialize().Should().Be("{s1,[e1]}");
	}

	[TestMethod]
	public async Task UNDERMustHandleArraysResult()
	{
		var parameters = new List<RuleParameter>()
		{
			new("Expression", "result", "FINDALL([dtmi:com:willowinc:TerminalUnit;1] & UNDER({this.someid, this.otherid}))")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-hot",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters
		};

		var harness = new ProcessorTestHarness();


		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "e1", contents: new Dictionary<string, object>()
		{
			["someid"] = "M1",
			["otherid"] = "O1"
		});

		var sensor = new TwinOverride("dtmi:com:willowinc:Fan;1", "s1");

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>() { sensor });

		var instances = await harness.GenerateRuleInstances();

		harness.twinService.LastADTQuery.Should().Be("SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId IN ['M1', 'O1'] AND IS_OF_MODEL(twin,'dtmi:com:willowinc:TerminalUnit;1')");

		//the mock just returns all the twins in its list
		instances[0].RuleParametersBound.First().PointExpression.Serialize().Should().Be("{s1,[e1]}");
	}

	[TestMethod]
	public async Task UNDERMustHandleTwinResultFromVariable()
	{
		var parameters = new List<RuleParameter>()
		{
			new("mytwin", "mytwin", "this"),
			new("Expression", "result", "FINDALL([dtmi:com:willowinc:TerminalUnit;1] & UNDER(mytwin))")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-hot",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters
		};

		var harness = new ProcessorTestHarness();


		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "e1", contents: new Dictionary<string, object>()
		{
			["someid"] = "M1",
			["otherid"] = "O1"
		});

		var sensor = new TwinOverride("dtmi:com:willowinc:Fan;1", "s1");

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>() { sensor });

		var instances = await harness.GenerateRuleInstances();

		harness.twinService.LastADTQuery.Should().Be("SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId = 'e1' AND IS_OF_MODEL(twin,'dtmi:com:willowinc:TerminalUnit;1')");

		//the mock just returns all the twins in its list
		instances[0].RuleParametersBound.Last().PointExpression.Serialize().Should().Be("{s1,[e1]}");
	}

	[TestMethod]
	public async Task FindAllWitProps()
	{
		var parameters = new List<RuleParameter>()
		{
			new("Expression", "result", "FINDALL(t, [dtmi:com:willowinc:TerminalUnit;1] & UNDER(this) & OPTION(t.myprop = 10, false)")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-hot",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters
		};

		var harness = new ProcessorTestHarness();

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "e1", contents: new Dictionary<string, object>()
		{
			["myprop"] = 10
		});

		var sensor = new TwinOverride("dtmi:com:willowinc:Fan;1", "s1");

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>() { sensor });

		var instances = await harness.GenerateRuleInstances();

		harness.twinService.LastADTQuery.Should().Be("SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId = 'e1' AND (IS_OF_MODEL(twin,'dtmi:com:willowinc:TerminalUnit;1'))");

		//the mock just returns all the twins in its list
		instances[0].RuleParametersBound.First().PointExpression.Serialize().Should().Be("{[e1]}");

		equipment.contents!["myprop"] = 9;

		instances = await harness.GenerateRuleInstances();

		harness.twinService.LastADTQuery.Should().Be("SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId = 'e1' AND (IS_OF_MODEL(twin,'dtmi:com:willowinc:TerminalUnit;1'))");

		//the mock just returns all the twins in its list
		instances[0].RuleParametersBound.First().PointExpression.Serialize().Should().Contain("No results found for filter");
	}

	[TestMethod]
	public async Task FindAllWithInvalidPropsMustFail()
	{
		var parameters = new List<RuleParameter>()
		{
			new("Expression", "result", "FINDALL([dtmi:com:willowinc:TerminalUnit;1] & UNDER(this) & OPTION(t.myprop = 10, false)")
		};

		var rule = new Rule()
		{
			Id = "terminal-unit-zone-hot",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters
		};

		var harness = new ProcessorTestHarness();

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "e1", contents: new Dictionary<string, object>()
		{
			["myprop"] = 10
		});

		var sensor = new TwinOverride("dtmi:com:willowinc:Fan;1", "s1");

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>() { sensor });

		var instances = await harness.GenerateRuleInstances();

		harness.twinService.LastADTQuery.Should().Be("SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId = 'e1' AND (IS_OF_MODEL(twin,'dtmi:com:willowinc:TerminalUnit;1'))");

		instances[0].RuleParametersBound.First().PointExpression.Serialize().Should().Be("FAILED(\"First argument must be a variable\",FINDALL(([dtmi:com:willowinc:TerminalUnit;1] & UNDER(this)) & OPTION(t.myprop == 10,False)))");
	}
}
