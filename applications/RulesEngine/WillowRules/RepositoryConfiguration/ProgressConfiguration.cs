using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for Progress table to use JSON on the progress dto object
/// </summary>
public class ProgressConfiguration : ConfigurationBase, IEntityTypeConfiguration<Progress>
{
	public void Configure(EntityTypeBuilder<Progress> builder)
	{
		builder.ArrayAsJson(e => e.InnerProgress);
	}
}
