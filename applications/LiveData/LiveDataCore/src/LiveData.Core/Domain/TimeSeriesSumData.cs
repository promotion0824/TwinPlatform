namespace Willow.LiveData.Core.Domain;

/// <summary>
/// Represents time series sum data.
/// </summary>
public class TimeSeriesSumData : TimeSeriesData
{
    //TODO: As discussed yes we'll go with using the same fields for now to minimise any impact and when we look at the support for multistate points we can probably do the story to align the field names a bit better etc

    /// <summary>
    /// Gets or sets the Sum.
    /// </summary>
    public decimal? Average { get; set; }
}
