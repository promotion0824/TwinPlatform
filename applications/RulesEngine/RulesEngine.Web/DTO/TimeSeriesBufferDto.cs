using RulesEngine.Web;

namespace WillowRules.DTO;

/// <summary>
/// Time series buffer data for examining inputs to rules engine
/// </summary>
public class TimeSeriesBufferDto
{
    /// <summary>
    /// Constructor
    /// </summary>
    public TimeSeriesBufferDto(string twinId, TimeSeriesDto buffer, TrendlineDto trendline, TrendlineDto rawTrendline, string timeZone)
    {
        Id = twinId;
        TimeSeries = buffer;
        Trendline = trendline;
        RawTrendline = rawTrendline;
        TimeZone = timeZone;
    }

    /// <summary>
    /// The twin id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The Timezone for the result
    /// </summary>
    public string TimeZone { get; set; }

    /// <summary>
    /// An Id for the time series (fix name)
    /// </summary>
    public TimeSeriesDto TimeSeries { get; init; }

    /// <summary>
    /// The time series data itself
    /// </summary>
    public TrendlineDto Trendline { get; set; }

    /// <summary>
    /// The time series data itself
    /// </summary>
    public TrendlineDto RawTrendline { get; set; }
}
