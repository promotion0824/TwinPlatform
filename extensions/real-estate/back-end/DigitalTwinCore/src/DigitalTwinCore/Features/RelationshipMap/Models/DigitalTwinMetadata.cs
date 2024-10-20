using System.Text.Json.Serialization;

namespace DigitalTwinCore.Features.RelationshipMap.Models;

public class DigitalTwinMetadata
{
    [JsonPropertyName("$model")]
    public string ModelId { get; set; }
}