namespace Willow.CommandAndControlAPI.SDK.Option;

/// <summary>
/// Options for the command and control client.
/// </summary>
public record CommandAndControlClientOption
{
    /// <summary>
    /// Gets or sets the base address.
    /// </summary>
    public string BaseAddress { get; set; } = null!;
}
