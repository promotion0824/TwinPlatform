namespace Willow.LiveData.Core.Features.Connectors.DTOs;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

/// <summary>
/// A list of connector IDs.
/// </summary>
[ExcludeFromCodeCoverage]
public class ConnectorList
{
    /// <summary>
    /// Gets or sets a list of connector IDs.
    /// </summary>
    public List<string> ConnectorIds { get; set; }
}
