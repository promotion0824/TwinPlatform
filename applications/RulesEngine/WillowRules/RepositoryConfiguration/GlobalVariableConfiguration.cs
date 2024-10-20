using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for GlobalVariable table to use JSON on array fields
/// </summary>
public class GlobalVariableConfiguration : ConfigurationBase, IEntityTypeConfiguration<GlobalVariable>
{
	public void Configure(EntityTypeBuilder<GlobalVariable> builder)
	{
		builder.ArrayAsJson(e => e.Expression);
		builder.ArrayAsJson(e => e.Parameters);
	}
}
