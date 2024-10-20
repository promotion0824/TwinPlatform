using Azure.DigitalTwins.Core;
using System.ComponentModel.DataAnnotations;

namespace Willow.Model.Requests;

public class BulkImportTwinsRequest
{
    /// <summary>
    /// Twins to import
    /// </summary>
    [Required]
    public BasicDigitalTwin[] Twins { get; set; } = Array.Empty<BasicDigitalTwin>();

    /// <summary>
    /// Relationships to import
    /// </summary>
    public IEnumerable<BasicRelationship>? Relationships { get; set; }

    /// <summary>
    /// Indicates if incoming twin relationships should be the only ones left for the owner twin
    /// </summary>
    public bool? TwinRelationshipsOverride { get; set; }
}
