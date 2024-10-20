using Abodit.Graph;
using Abodit.Mutable;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

#nullable disable

namespace WillowRules.Test.Bugs;

[TestClass]
public class RuleDepencyTests
{
	[TestMethod]
	public async Task MustCreateReferencedCapabilityDependency()
	{
		var rule = new Rule()
		{
			Id = "rule",
			Name = "rule",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			Parameters = new List<RuleParameter>()
			{
				new RuleParameter("sensor1", "sensor1", "[dtmi:com:willowinc:sensor1model;1]"),
				new RuleParameter("result", "result", "1")
			},
			Dependencies = new List<RuleDependency>()
			{
				new RuleDependency("rule-dep", RuleDependencyRelationships.ReferencedCapability)
			},
			TemplateId = RuleTemplateUnchanging.ID,
			Elements = new List<RuleUIElement>()
			{
				Fields.OverHowManyHours.With(24),
			}
		};

		var ruleDependency = new Rule()
		{
			Id = "rule-dep",
			Name = "rule-dep",
			PrimaryModelId = "dtmi:com:willowinc:Capability;1",
			TemplateId = RuleTemplateUnchanging.ID
		};

		var sensor1baseModel = new ModelData()
		{
			Id = "dtmi:com:willowinc:Capability;1",
			DtdlModel = new DtdlModel()
			{
			}
		};

		var sensor1model = new ModelData()
		{
			Id = "dtmi:com:willowinc:sensor1model;1",
			DtdlModel = new DtdlModel()
			{
				extends = new StringList()
				{
					"dtmi:com:willowinc:Capability;1"
				}
			}
		};
		
		var harness = new ProcessorTestHarness();

		await harness.AddToModelGraph(sensor1baseModel);

		await harness.AddToModelGraph(sensor1model);

		await harness.AddRule(ruleDependency);

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");

		var sensor1 = new TwinOverride("dtmi:com:willowinc:sensor1model;1", "sensor1", trendId: Guid.NewGuid().ToString());

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>()
		{
			sensor1
		});

		var instances = await harness.GenerateRuleInstances();

		instances.Count.Should().Be(1);

		var instance = instances[0];

		instance.RuleDependenciesBound.Count.Should().Be(1);

		var dependency = instance.RuleDependenciesBound[0];

		dependency.TwinId.Should().Be("sensor1");
		dependency.RuleInstanceId.Should().Be("sensor1_rule-dep");
		dependency.Relationship.Should().Be(RuleDependencyRelationships.ReferencedCapability);
	}

	[TestMethod]
	public async Task MustCreateSiblingDependency()
	{
		var rule = new Rule()
		{
			Id = "rule",
			Name = "rule",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			Parameters = new List<RuleParameter>(),
			Dependencies = new List<RuleDependency>()
			{
				new RuleDependency("rule-dep", RuleDependencyRelationships.Sibling)
			},
			TemplateId = RuleTemplateUnchanging.ID,
			Elements = new List<RuleUIElement>()
			{
				Fields.OverHowManyHours.With(24),
			}
		};

		var ruleDependency = new Rule()
		{
			Id = "rule-dep",
			Name = "rule-dep",
			PrimaryModelId = "dtmi:com:willowinc:base;1",
			TemplateId = RuleTemplateUnchanging.ID
		};

		var baseModel = new ModelData()
		{
			Id = "dtmi:com:willowinc:base;1",
			DtdlModel = new DtdlModel()
			{
				contents = new Content[]
				{
					new Content()
					{
						name = "type",
						target = "dtmi:com:willowinc:TerminalUnit;1",
						type = "Relationship"
					}
				}
			}
		};

		var terminalUnitmodel = new ModelData()
		{
			Id = "dtmi:com:willowinc:TerminalUnit;1",
			DtdlModel = new DtdlModel()
			{
				extends = new StringList()
				{
					"dtmi:com:willowinc:base;1"
				}
			}
		};

		var harness = new ProcessorTestHarness();

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");

		var sensor1 = new TwinOverride("dtmi:com:willowinc:sensor1model;1", "sensor1", trendId: Guid.NewGuid().ToString());

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>()
		{
			sensor1
		});

		await harness.AddToModelGraph(baseModel);

		await harness.AddToModelGraph(terminalUnitmodel);

		await harness.AddRule(ruleDependency);

		var instances = await harness.GenerateRuleInstances();

		instances.Count.Should().Be(1);

		var instance = instances[0];

		instance.RuleDependenciesBound.Count.Should().Be(1);

		var dependency = instance.RuleDependenciesBound[0];

		dependency.TwinId.Should().Be("equipment");
		dependency.RuleInstanceId.Should().Be("equipment_rule-dep");
		dependency.Relationship.Should().Be(RuleDependencyRelationships.Sibling);
	}

	[TestMethod]
	public async Task MustCreateFedByDependency()
	{
		var rule = new Rule()
		{
			Id = "rule",
			Name = "rule",
			PrimaryModelId = "dtmi:com:willowinc:TerminalUnit;1",
			Parameters = new List<RuleParameter>(),
			Dependencies = new List<RuleDependency>()
			{
				new RuleDependency("rule-dep", RuleDependencyRelationships.RelatedTo)
			},
			TemplateId = RuleTemplateUnchanging.ID,
			Elements = new List<RuleUIElement>()
			{
				Fields.OverHowManyHours.With(24),
			}
		};

		var ruleDependency = new Rule()
		{
			Id = "rule-dep",
			Name = "rule-dep",
			PrimaryModelId = "dtmi:com:willowinc:hvac;1",
			TemplateId = RuleTemplateUnchanging.ID
		};

		var harness = new ProcessorTestHarness();

		var equipment = new TwinOverride("dtmi:com:willowinc:TerminalUnit;1", "equipment");

		var sensor1 = new TwinOverride("dtmi:com:willowinc:sensor1model;1", "sensor1", trendId: Guid.NewGuid().ToString());

		harness.OverrideCaches(rule, equipment, new List<TwinOverride>()
		{
			sensor1
		});

		var hvac = new TwinOverride("dtmi:com:willowinc:hvac;1", "hvac1");

		harness.OverrideCaches(ruleDependency, hvac, new List<TwinOverride>());

		await harness.AddForwardEdge(equipment.twinId, new Edge()
		{
			RelationshipType = "isFedBy",
			Destination = new BasicDigitalTwinPoco()
			{
				Id = hvac.twinId,
				name = hvac.twinId,
				Metadata = new DigitalTwinMetadataPoco()
				{
					ModelId = hvac.modelId
				}
			}
		});

		var instances = await harness.GenerateRuleInstances();

		instances.Count.Should().Be(1);

		var instance = instances[0];

		instance.RuleDependenciesBound.Count.Should().Be(1);

		var dependency = instance.RuleDependenciesBound[0];

		dependency.TwinId.Should().Be("hvac1");
		dependency.RuleInstanceId.Should().Be("hvac1_rule-dep");
		dependency.Relationship.Should().Be(RuleDependencyRelationships.RelatedTo);
	}
}
