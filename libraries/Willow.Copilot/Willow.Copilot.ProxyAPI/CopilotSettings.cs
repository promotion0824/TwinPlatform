namespace Willow.Copilot.ProxyAPI;

/// <summary>
/// Option record for copilot settings.
/// </summary>
public record CopilotSettings
{
    /// <summary>
    /// Gets or sets the base address for the copilot service.
    /// When deployed in ACA env the address is same as the name of the container http://copilot.
    /// </summary>
    public string BaseAddress { get; set; } = "http://localhost:8080";
}
