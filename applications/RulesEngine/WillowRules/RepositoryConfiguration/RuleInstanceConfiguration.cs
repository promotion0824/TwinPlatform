using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for RuleInstance table to use JSON on two fields
/// </summary>
public class RuleInstanceConfiguration : ConfigurationBase, IEntityTypeConfiguration<RuleInstance>
{
	public void Configure(EntityTypeBuilder<RuleInstance> builder)
	{
		builder.ArrayAsJson(e => e.PointEntityIds);
		builder.ArrayAsJson(e => e.RuleParametersBound);
		builder.ArrayAsJson(e => e.RuleImpactScoresBound);
		builder.ArrayAsJson(e => e.RuleFiltersBound);
		builder.ArrayAsJson(e => e.RuleDependenciesBound);
		builder.ArrayAsJson(e => e.RuleTriggersBound);
		builder.ArrayAsJson(e => e.RuleTags);
		builder.ArrayAsJson(e => e.FedBy);
		builder.ArrayAsJson(e => e.Feeds);
		builder.ArrayAsJson(e => e.TwinLocations);
	}
}
