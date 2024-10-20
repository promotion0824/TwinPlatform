namespace Willow.AppContext;

#nullable disable

/// <summary>
/// Willow standard version meter that needs to be applied in each app.
/// </summary>
public class WillowMeterOptions
{
    /// <summary>
    /// Gets or Sets the Assembly name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or Sets the Assembly version.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or Sets the Tags.
    /// </summary>
    public IEnumerable<KeyValuePair<string, object>> Tags { get; set; }
}
