
namespace Willow.AzureDigitalTwins.SDK.Option;

/// <summary>
/// Adt Api Client Options for using the SDK
/// </summary>
public class AdtApiClientOption
{
    /// <summary>
    ///  ADT API base address. Generally http://adt-api/ in production and https://localhost:8001/ if you run locally.
    /// </summary>
    public string BaseAddress { get; set; } = null!;

    /// <summary>
    /// Timeout for http client in timespan string D.HH:MM:SS. Default is 100 seconds.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
}
