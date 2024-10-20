using CsvHelper.Configuration.Attributes;

namespace Willow.Model.TimeSeries;

public class TimeSeriesRow
{
    public string? ExternalId { get; set; }

    public string? TrendId { get; set; }

    public DateTime? SourceTimestamp { get; set; }

    [Ignore]
    public DateTime? EnqueuedTimestamp { get; set; }

    public double? ScalarValue { get; set; }
}
