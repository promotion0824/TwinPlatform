using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for Rule table serializing some complex fields as Json
/// </summary>
public class RuleConfiguration : ConfigurationBase, IEntityTypeConfiguration<Rule>
{
	public void Configure(EntityTypeBuilder<Rule> builder)
	{
		builder.ArrayAsJson(e => e.Parameters);
		builder.ArrayAsJson(e => e.ImpactScores);
		builder.ArrayAsJson(e => e.Filters);
		builder.ArrayAsJson(e => e.Elements);
		builder.ArrayAsJson(e => e.RuleTriggers);
		builder.ArrayAsJson(e => e.Tags);
		builder.ArrayAsJson(e => e.Dependencies);
		builder.AsDictionaryJson(e => e.LanguageNames);
		builder.AsDictionaryJson(e => e.LanguageDescriptions);
		builder.AsDictionaryJson(e => e.LanguageRecommendations);
	}
}
