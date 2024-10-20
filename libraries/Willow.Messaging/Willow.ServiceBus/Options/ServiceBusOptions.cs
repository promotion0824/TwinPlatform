namespace Willow.ServiceBus.Options;

using System.Collections.Generic;

/// <summary>
/// The service bus options.
/// </summary>
public class ServiceBusOptions
{
    /// <summary>
    /// Gets or sets the service bus Namespaces.
    /// </summary>
    public Dictionary<string, string> Namespaces { get; set; } = default!;

    /// <summary>
    /// Gets or sets the connection string.
    /// </summary>
    public string? ConnectionString { get; set; } = default!;
}
