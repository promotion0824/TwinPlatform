namespace Willow.Api.Client.Sdk.Marketplace.Options;

/// <summary>
/// The options for the Marketplace API.
/// </summary>
public class MarketplaceApiOptions : IApiOptions
{
    /// <summary>
    /// Gets or sets the base address for the Marketplace API.
    /// </summary>
    public string BaseAddress { get; set; } = default!;
}
