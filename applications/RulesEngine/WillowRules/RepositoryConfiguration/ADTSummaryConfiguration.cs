using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for ADTSummary table to use JSON on array fields
/// </summary>
public class ADTSummaryConfiguration : ConfigurationBase, IEntityTypeConfiguration<ADTSummary>
{
	public void Configure(EntityTypeBuilder<ADTSummary> builder)
	{
		builder.AsJsonAny(e => e.SystemSummary);
		builder.AsJsonAny(e => e.ExtensionData);
	}
}
