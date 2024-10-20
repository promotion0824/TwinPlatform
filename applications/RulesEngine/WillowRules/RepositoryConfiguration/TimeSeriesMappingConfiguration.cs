using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for TimeSeriesMapping table to use JSON on two fields
/// </summary>
public class TimeSeriesMappingConfiguration : ConfigurationBase, IEntityTypeConfiguration<TimeSeriesMapping>
{
	public void Configure(EntityTypeBuilder<TimeSeriesMapping> builder)
	{
		builder.ArrayAsJson(e => e.TwinLocations);
	}
}
