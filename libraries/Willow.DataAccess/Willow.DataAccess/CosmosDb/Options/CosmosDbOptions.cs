namespace Willow.DataAccess.CosmosDb.Options;

/// <summary>
/// Options for connecting to CosmosDb.
/// </summary>
public class CosmosDbOptions
{
    /// <summary>
    /// Gets or sets the endpoint url for the instance.
    /// </summary>
    public string EndpointUrl { get; set; } = default!;

    /// <summary>
    /// Gets or sets the connection string for the instance.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use managed identity.
    /// </summary>
    public bool UseManagedIdentity { get; set; } = true;
}
