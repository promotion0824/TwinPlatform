using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for Insight table to use JSON on array fields
/// </summary>
public class CommandConfiguration : ConfigurationBase, IEntityTypeConfiguration<Command>
{
	public void Configure(EntityTypeBuilder<Command> builder)
	{
		builder.ArrayAsCompressedJson(e => e.Occurrences);
		builder.ArrayAsJson(e => e.Relationships);
	}
}
