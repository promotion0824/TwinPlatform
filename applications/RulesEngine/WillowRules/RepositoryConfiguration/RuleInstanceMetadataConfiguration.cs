using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for <see cref="RuleInstanceMetadata"/> table to use JSON on the progress dto object
/// </summary>
public class RuleInstanceMetadataConfiguration : ConfigurationBase, IEntityTypeConfiguration<RuleInstanceMetadata>
{
	public void Configure(EntityTypeBuilder<RuleInstanceMetadata> builder)
	{
		builder.ArrayAsJson(v => v.Comments);
		builder.ArrayAsJson(v => v.Tags);
	}
}
