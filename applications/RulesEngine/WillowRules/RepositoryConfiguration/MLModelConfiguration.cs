using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for MLModel table to use JSON on array fields
/// </summary>
public class MLModelConfiguration : ConfigurationBase, IEntityTypeConfiguration<MLModel>
{
	public void Configure(EntityTypeBuilder<MLModel> builder)
	{
		builder.AsJsonAny(e => e.ExtensionData);
	}
}
