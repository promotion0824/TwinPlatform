namespace Willow.Api.Client.Sdk.EnvironmentRegistry.Options;

/// <summary>
/// The options for the Environment Registry API.
/// </summary>
public class EnvironmentRegistryApiOptions : IApiOptions
{
    /// <summary>
    /// Gets or sets the base address for the Environment Registry API.
    /// </summary>
    public string BaseAddress { get; set; } = default!;
}
