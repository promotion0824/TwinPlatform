using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Willow.Rules.Model;

namespace WillowRules.Migrations;

/// <summary>
/// Configuration for TimeSeriesBuffer table to use JSON on the progress dto object
/// </summary>
public class TimeSeriesBufferConfiguration : ConfigurationBase, IEntityTypeConfiguration<TimeSeries>
{
	public void Configure(EntityTypeBuilder<TimeSeries> builder)
	{
		builder.EnumerableAsCompressedJson(v => v.Points)
			.UsePropertyAccessMode(PropertyAccessMode.Property);

		builder.AsJsonWithDefault(e => e.AveragePeriodEstimator, new());
		builder.AsJsonWithDefault(e => e.ValueOutOfRangeEstimator, new());
		builder.AsJsonWithDefault(e => e.CompressionState, new());
		builder.AsJsonWithDefault(e => e.LatencyEstimator, new());
		builder.Property(e => e.MaxTimeToKeep).HasConversion<TimeSpanToTicksConverter>();
		builder.Property(e => e.EstimatedPeriod).HasConversion<TimeSpanToTicksConverter>();
		builder.Property(e => e.LastGap).HasConversion<TimeSpanToTicksConverter>();
		builder.Property(e => e.Latency).HasConversion<TimeSpanToTicksConverter>();
		builder.ArrayAsJson(e => e.TwinLocations);
	}
}
