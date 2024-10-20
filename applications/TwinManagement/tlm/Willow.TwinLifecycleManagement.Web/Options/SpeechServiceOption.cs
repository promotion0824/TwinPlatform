namespace Willow.TwinLifecycleManagement.Web.Options;

/// <summary>
/// Speech Service Option.
/// </summary>
public record SpeechServiceOption
{
    /// <summary>
    /// Gets or sets the speech key.
    /// </summary>
    public string SpeechKey { get; set; }

    /// <summary>
    /// Gets or sets the speech service region.
    /// </summary>
    public string Region { get; set; }
}
