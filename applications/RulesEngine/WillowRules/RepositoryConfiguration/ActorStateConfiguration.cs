using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for <see cref="ActorState"/> table to use JSON on the progress dto object
/// </summary>
public class ActorStateConfiguration : ConfigurationBase, IEntityTypeConfiguration<ActorState>
{
	public void Configure(EntityTypeBuilder<ActorState> builder)
	{
		builder.AsCompressedJsonAnyType(v => v.TimedValues);
		builder.AsCompressedJsonAnyType(v => v.OutputValues);
	}
}
