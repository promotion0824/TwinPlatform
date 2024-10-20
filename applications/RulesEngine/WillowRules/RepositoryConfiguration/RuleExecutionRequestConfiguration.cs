using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for RuleExecutionRequest table serializing some complex fields as Json
/// </summary>
public class RuleExecutionRequestConfiguration : ConfigurationBase, IEntityTypeConfiguration<RuleExecutionRequest>
{
	public void Configure(EntityTypeBuilder<RuleExecutionRequest> builder)
	{
		builder.AsJsonAny(e => e.ExtendedData);
	}
}
