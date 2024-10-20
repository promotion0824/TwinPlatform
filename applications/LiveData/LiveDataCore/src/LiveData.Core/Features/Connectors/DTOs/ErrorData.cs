namespace Willow.LiveData.Core.Features.Connectors.DTOs;

using System.Collections.Generic;

/// <summary>
/// Error data.
/// </summary>
public class ErrorData
{
    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the IDs of the operation.
    /// </summary>
    public IEnumerable<dynamic> Ids { get; set; }
}
