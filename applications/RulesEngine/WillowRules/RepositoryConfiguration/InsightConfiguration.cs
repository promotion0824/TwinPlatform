using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for Insight table to use JSON on array fields
/// </summary>
public class InsightConfiguration : ConfigurationBase, IEntityTypeConfiguration<Insight>
{
	public void Configure(EntityTypeBuilder<Insight> builder)
	{
		builder.ArrayAsJson(e => e.Feeds);
		builder.ArrayAsJson(e => e.FedBy);
		builder.ArrayAsJson(e => e.Dependencies);
		builder.ArrayAsJson(e => e.Points);
		builder.ArrayAsJson(e => e.TwinLocations);
		builder.ArrayAsJson(e => e.RuleTags);
	}
}
