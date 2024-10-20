namespace Willow.TwinLifecycleManagement.Web.Models;

/// <summary>
/// A request to create documents.
/// </summary>
public class DeleteTwinsRequest
{
    /// <summary>
    /// Gets or sets twin IDs used for deleting twins in ADT and ADX.
    /// </summary>
    public string[] twinIDs { get; set; } = [];

    /// <summary>
    /// Gets or sets external IDs used to delete mapping entries.
    /// </summary>
    public string[] externalIDs { get; set; } = [];
}
