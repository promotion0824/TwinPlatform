using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for <see cref="RuleMetadata"/> table to use JSON on the progress dto object
/// </summary>
public class RuleMetadataConfiguration : ConfigurationBase, IEntityTypeConfiguration<RuleMetadata>
{
	public void Configure(EntityTypeBuilder<RuleMetadata> builder)
	{
		builder.AsJsonAny(v => v.ExtensionData);
	}
}
