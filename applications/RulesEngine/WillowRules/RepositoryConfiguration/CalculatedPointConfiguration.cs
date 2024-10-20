using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for CalculatedPoint table to use JSON on two fields
/// </summary>
public class CalculatedPointConfiguration : ConfigurationBase, IEntityTypeConfiguration<CalculatedPoint>
{
	public void Configure(EntityTypeBuilder<CalculatedPoint> builder)
	{
		builder.ArrayAsJson(e => e.TwinLocations);
	}
}
